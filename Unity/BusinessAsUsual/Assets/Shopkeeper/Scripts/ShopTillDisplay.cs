using System.Linq;
using UnityEngine;
namespace BusinessAsUsual.Shopkeeper
{
    public sealed class ShopTillDisplay : MonoBehaviour
    {
        public ShopShift shift;
        public TextMesh display;
        void LateUpdate(){if(shift&&display)display.text="$"+((shift.Transactions.Receipts.LastOrDefault()?.totalCents??0)/100f).ToString("F2");}
    }
}
