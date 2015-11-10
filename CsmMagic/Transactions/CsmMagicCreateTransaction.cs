using CsmMagic.Models;
using Trebuchet.API;

namespace CsmMagic.Transactions
{
    internal class CsmMagicCreateTransaction<TBusinessObjectModel> : CsmMagicWriteTransaction<TBusinessObjectModel> where TBusinessObjectModel : BusinessObjectModel
    {
        internal BusinessObject NewTrebuchetObject { get; set; }

        internal TBusinessObjectModel NewDomainData { get; set; }

        internal CsmMagicCreateTransaction(TBusinessObjectModel newData, BusinessObject newTrebuchetObject)
        {
            NewDomainData = newData;
            NewTrebuchetObject = newTrebuchetObject;
        }

        internal override void Execute()
        {
            Write(NewDomainData, NewTrebuchetObject);
        }

        internal override void Rollback()
        {
            if (TrebuchetApi.Api.BusObServices.GetBusinessObjectByRecId(NewTrebuchetObject.Def.Id, NewTrebuchetObject.RecId) != null)
            {
                TrebuchetApi.Api.BusObServices.DeleteBusObById(NewTrebuchetObject.Def.Id, NewTrebuchetObject.RecId);
            }
        }
    }
}
