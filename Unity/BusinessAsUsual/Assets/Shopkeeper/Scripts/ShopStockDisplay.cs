using UnityEngine;
namespace BusinessAsUsual.Shopkeeper
{
    // The marked front row shows real shift stock; background assortment is decorative.
    public sealed class ShopStockDisplay : MonoBehaviour
    {
        public ShopShift shift;
        public string sku;
        public GameObject[] units;
        public ShopRestockReach reach;
        int previous=-1;
        void LateUpdate()
        {
            if(!shift || units==null || (reach && reach.HoldsDisplay(sku)))return;int count=shift.Stock.Count("shelf",sku);
            if(count==previous)return;previous=count;
            for(int i=0;i<units.Length;i++)if(units[i])units[i].SetActive(i<count);
        }
    }
}
