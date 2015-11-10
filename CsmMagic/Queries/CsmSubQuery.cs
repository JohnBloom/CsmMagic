using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsmMagic.Models;

namespace CsmMagic.Queries
{
    internal abstract class CsmSubQuery
    {
        internal string RelationshipName { get; set; }

        internal string DestinationPropertyName { get; set; }

        internal Type Type { get; set; }

        internal bool IsCollection { get; set; }

        internal abstract void SetValue(PropertyInfo prop, BusinessObjectModel businessObjectModel, IEnumerable<BusinessObjectModel> subResults);

        internal abstract void SetValue(PropertyInfo prop, BusinessObjectModel businessObjectModel, BusinessObjectModel subResults);
    }

    internal class CsmSubQuery<T> : CsmSubQuery where T : class
    {
        internal override void SetValue(PropertyInfo prop, BusinessObjectModel businessObjectModel, IEnumerable<BusinessObjectModel> subResults)
        {            
            prop.SetValue(businessObjectModel, subResults.Select(x => x as T).ToList());
        }

        internal override void SetValue(PropertyInfo prop, BusinessObjectModel businessObjectModel, BusinessObjectModel subResults)
        {
            prop.SetValue(businessObjectModel, subResults as T);
        }
    }
}
