using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsmMagic.Attributes;
using CsmMagic.Models;
using Trebuchet;
using Trebuchet.API;

namespace CsmMagic.Transactions
{
    internal class CsmMagicUpdateTransaction<TBusinessObjectModel> : CsmMagicWriteTransaction<TBusinessObjectModel> where TBusinessObjectModel : BusinessObjectModel
    {
        private readonly Dictionary<string, TypedValue> _originalFieldValues = new Dictionary<string, TypedValue>();

        internal CsmMagicUpdateTransaction(TBusinessObjectModel entity, BusinessObject destinationData)
        {
            DestinationData = destinationData;
            NewData = entity;
            var fields = GetFields(entity);
            var trebuchetFields =
                fields.Select(f => DestinationData.Def.Fields.GetFieldByName(f.GetCustomAttribute<FieldAttribute>().Name));
            foreach (var field in trebuchetFields.Where(f => f != null))
            {
                _originalFieldValues.Add(field.Name, DestinationData.GetFieldValue(field.Id));
            }
        }

        internal BusinessObject DestinationData { get; set; }

        internal TBusinessObjectModel NewData { get; set; }

        internal override void Execute()
        {
            Write(NewData, DestinationData);
        }

        internal override void Rollback()
        {
            RestoreOriginalData();
        }

        private void RestoreOriginalData()
        {
            foreach (var fieldInfo in GetFields(NewData))
            {
                var fieldName = fieldInfo.GetCustomAttribute<FieldAttribute>().Name;
                var field = DestinationData.Def.Fields.GetFieldByName(fieldName);
                if (field == null)
                {
                    continue;
                }
                var originalFieldValue = _originalFieldValues[field.Name];
                var updatedFieldValue = DestinationData.GetFieldValue(field.Id);
                if (originalFieldValue.ToText() != updatedFieldValue.ToText())
                {
                    DestinationData.GetField(field.Id).SetValue(originalFieldValue);
                }
            }
        }
    }
}
