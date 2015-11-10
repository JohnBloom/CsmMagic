using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsmMagic.Attributes;
using CsmMagic.Exceptions;
using CsmMagic.Models;
using CsmMagic.Validation;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic.Transactions
{
    internal abstract class CsmMagicWriteTransaction<TBusinessObjectModel> where TBusinessObjectModel : BusinessObjectModel
    {
        internal Exception FailException { get; set; }

        internal bool WasSuccessful { get { return FailException == null; } }

        internal abstract void Execute();

        internal abstract void Rollback();

        protected void Write(TBusinessObjectModel dataToWrite, BusinessObject destinationBusinessObject)
        {
            foreach (var propertyInfo in GetFields(dataToWrite))
            {
                try
                {
                    SetFieldValue(destinationBusinessObject, dataToWrite, propertyInfo);
                }
                catch (Exception ex)
                {
                    FailException = ex;
                    return;
                }
            }
            var result = destinationBusinessObject.Save();

            if (!result.Success)
            {
                FailException = new CherwellUpdateException(result.ErrorText);
            }

            dataToWrite.RecId = destinationBusinessObject.RecId;
        }

        protected IEnumerable<PropertyInfo> GetFields(TBusinessObjectModel model)
        {
            var entityType = model.GetType();
            return
                entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(pi => Attribute.IsDefined(pi, typeof (FieldAttribute)));
        }

        private void SetFieldValue(BusinessObject entityBo, TBusinessObjectModel entity, PropertyInfo propertyInfo)
        {
            var field = propertyInfo.GetCustomAttribute<FieldAttribute>();
            var validation = propertyInfo.GetCustomAttribute<CsmValidationAttribute>();
            var value = propertyInfo.GetValue(entity, null);

            if (!field.IsWriteable || value == null)
            {
                return;
            }

            if (validation != null && !RunValidator(validation.Validator, propertyInfo.Name, value, entity))
            {
                if (!validation.ThrowException)
                {
                    return;
                }

                var newMessage = "Custom validation failed for " + propertyInfo.Name;

                if (!string.IsNullOrEmpty(validation.Message))
                {
                    newMessage = newMessage + ": " + validation.Message;
                }

                throw new CsmValidationException(newMessage);
            }

            var fieldDefinition = entityBo.Def.Fields.GetFieldByNameThrowIfNotFound(field.Name, entityBo.Def.Name);

            propertyInfo.SetValue(entity, value);
            var cherwellField = entityBo.GetField(fieldDefinition.Id);

            var updateTransaction = cherwellField.SetValue(TypedValue.ForString(value.ToString()));

            if (!updateTransaction.Success)
            {
                throw new CherwellUpdateException(
                    string.Format("Business object update failure for {0}: {1}", fieldDefinition.Name,
                        updateTransaction.ErrorText),
                    updateTransaction.ErrorText,
                    updateTransaction.ErrorText);
            }

            if (ValueIsActuallyUpdated(cherwellField, value.ToString()))
            {
                return;
            }

            // try again, Cherwell will probably work
            cherwellField.SetValue(TypedValue.ForString(value.ToString()));
            if (!ValueIsActuallyUpdated(cherwellField, value.ToString()))
            {
                throw new CherwellUpdateException(
                    string.Format(
                        "Attempted to set {0} to {1} twice, and it failed both times. The previous value was {2}",
                        field.Name,
                        value,
                        cherwellField.Value));
            }
        }

        private bool RunValidator(object validator, string fieldName, object value, TBusinessObjectModel businessObject)
        {
            var validatorType = validator.GetType().BaseType;
            var constraint = validatorType.GenericTypeArguments.First();
            var validatorTypeDefinition = validatorType.GetGenericTypeDefinition();

            if (!(constraint.IsAssignableFrom(typeof(TBusinessObjectModel))) && validatorTypeDefinition.Name == "CsmValidator`1")
            {
                throw new InvalidOperationException(
                    string.Format(
                        "You must use a type in the construction of your CsmValidationAttribute that is a CsmValidator in the decoration of this class {0}",
                        typeof(TBusinessObjectModel)));
            }

            //We are using a dynamic here because we cannot cast the validator typed to the base type as a validator of the derived type.
            //The above checks make sure that the derived type is assignable to the base type and that the class is a CsmValidator
            dynamic dinstance = validator;

            return dinstance.Validate(fieldName, value, businessObject);
        }

        /// <summary>
        /// There appears to be some inexplicably bizarre behavior where Cherwell won't "take" an updated field value on the first try, but WILL on the second attempt!
        /// </summary>
        /// <param name="field"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private bool ValueIsActuallyUpdated(Field field, string fieldValue)
        {
            switch (field.Value.FieldType)
            {
                case FieldSubType.Number:
                    decimal newFieldValue;
                    if (decimal.TryParse(fieldValue, out newFieldValue))
                    {
                        return field.Value.ToNumber() == newFieldValue;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("{0} is not a parseable as a decimal.", fieldValue));
                    }

                case FieldSubType.DateOnly:
                case FieldSubType.DateTime:
                    if (string.IsNullOrWhiteSpace(fieldValue))
                    {
                        // trying to blank time
                        return true;
                    }
                    else
                    {
                        // they change formats when it goes in to cherwell!
                        DateTime setValue;
                        if (!DateTime.TryParse(fieldValue, out setValue))
                        {
                            throw new FormatException(string.Format("{0} is not a parseable datetime", fieldValue));
                        }

                        var theirs = field.Value.ToDateTime();
                        return theirs == setValue;
                    }

                case FieldSubType.Logical:

                    // Cherwell returns "TRUE", .NET uses "True", etc.
                    return string.Equals(
                        field.Value.ToString(),
                        fieldValue,
                        StringComparison.InvariantCultureIgnoreCase);
                default:
                    return field.Value.ToString() == fieldValue;
            }
        }
    }
}
