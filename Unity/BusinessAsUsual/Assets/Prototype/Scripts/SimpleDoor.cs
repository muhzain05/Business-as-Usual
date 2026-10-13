using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    public sealed class SimpleDoor : Interactable
    {
        public Transform hinge;
        public Transform handle;
        public float openAngle = -100;
        public float angularSpeed = 220;
        public bool IsOpen { get; private set; }
        public void RestoreNetworkState(bool open) { IsOpen=open; }
        Quaternion closedRotation;
        public override Vector3 InteractionPoint => handle ? handle.position : transform.position;
        public override string Prompt => IsOpen ? "Close shop door" : "Open shop door";
        void Awake() { closedRotation = hinge.localRotation; }
        public override bool Interact(PlayerInteractor player)
        {
            IsOpen = !IsOpen;
            player.Notify(IsOpen ? "Shop door opened." : "Shop door closed.");
            return true;
        }
        void Update() { Advance(Time.deltaTime); }
        public void Advance(float deltaTime)
        {
            if (!hinge) return;
            Quaternion target = closedRotation * Quaternion.Euler(0, IsOpen ? openAngle : 0, 0);
            hinge.localRotation = Quaternion.RotateTowards(hinge.localRotation, target, angularSpeed * deltaTime);
        }
    }
}
