using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class CarryableBox : Interactable
    {
        public string displayName = "delivery box";
        Rigidbody body;
        Collider[] colliders;
        bool[] colliderStates;
        bool oldKinematic, oldGravity;
        Transform originalParent;
        public bool IsHeld { get; private set; }
        public override string Prompt => "Pick up " + displayName;
        public override bool CanInteract(PlayerInteractor player) => !IsHeld && !player.HeldBox;
        void Awake() { body = GetComponent<Rigidbody>(); colliders = GetComponentsInChildren<Collider>(); }
        public override bool Interact(PlayerInteractor player)
        {
            if (!CanInteract(player) || !player.carryAnchor) return false;
            oldKinematic = body.isKinematic; oldGravity = body.useGravity;
            originalParent = transform.parent;
            colliderStates = new bool[colliders.Length];
            for (int i = 0; i < colliders.Length; i++) { colliderStates[i] = colliders[i].enabled; colliders[i].enabled = false; }
            body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            body.isKinematic = true; body.useGravity = false;
            transform.SetParent(player.carryAnchor, false);
            transform.localPosition = Vector3.zero; transform.localRotation = Quaternion.identity;
            IsHeld = true; player.HeldBox = this;
            player.Notify("Carrying box. Press E to drop it on clear ground.");
            return true;
        }
        public bool TryDrop(PlayerInteractor player)
        {
            if (!IsHeld || player.HeldBox != this) return false;
            // Ground placement avoids throwing the box into a wall or under the player.
            BoxCollider box = GetComponent<BoxCollider>();
            Vector3 half = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
            for (int i = 0; i < 5; i++)
            {
                float angle = i == 0 ? 0 : (i % 2 == 1 ? 1 : -1) * ((i + 1) / 2) * 35;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * player.carryAnchor.forward;
                direction.y = 0; direction.Normalize();
                Vector3 sample = player.transform.position + direction * 1.15f + Vector3.up * 2;
                if (!Physics.Raycast(sample, Vector3.down, out RaycastHit ground, 4, player.interactionMask, QueryTriggerInteraction.Ignore)) continue;
                Vector3 location = ground.point + Vector3.up * (half.y + 0.025f);
                Quaternion rotation = Quaternion.Euler(0, player.carryAnchor.eulerAngles.y + angle, 0);
                if (Physics.CheckBox(location, half * 0.95f, rotation, player.interactionMask, QueryTriggerInteraction.Ignore)) continue;
                ReleaseAtCurrentPosition(player);
                transform.SetPositionAndRotation(location, rotation);
                Physics.SyncTransforms();
                player.Notify("Box dropped.");
                return true;
            }
            player.Notify("No clear space. Move away from walls to drop the box.");
            return false;
        }
        public void ReleaseAtCurrentPosition(PlayerInteractor player)
        {
            if (!IsHeld) return;
            transform.SetParent(originalParent, true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = colliderStates[i];
            body.isKinematic = oldKinematic; body.useGravity = oldGravity;
            IsHeld = false;
            if (player.HeldBox == this) player.HeldBox = null;
        }
    }
}
