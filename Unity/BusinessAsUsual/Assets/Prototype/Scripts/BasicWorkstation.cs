using UnityEngine;
using UnityEngine.Events;

namespace BusinessAsUsual.Prototype
{
    public sealed class BasicWorkstation : Interactable
    {
        public string stationName = "garage workbench";
        public UnityEvent onUsed = new UnityEvent();
        public int UseCount { get; private set; }
        public void RestoreNetworkState(int uses) { UseCount=uses; }
        public override string Prompt => "Use " + stationName;
        public override bool Interact(PlayerInteractor player)
        {
            UseCount++;
            onUsed.Invoke();
            player.Notify(stationName + " checked. Uses: " + UseCount);
            return true;
        }
    }
}
