using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    [RequireComponent(typeof(PrototypeInput))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public float reach = 1.8f;
        public float originHeight = 0.65f;
        public LayerMask interactionMask = ~((1 << 2));
        public Transform carryAnchor;
        // Optional first-person view: targets inside its aim cone win over merely closer ones.
        public Transform aim;
        public float aimConeDegrees = 32f;
        public Interactable Current { get; private set; }
        public CarryableBox HeldBox { get; internal set; }
        public string Status { get; private set; }
        float statusUntil;
        PrototypeInput input;
        readonly Collider[] nearby = new Collider[48];
        public Vector3 Origin => transform.position + Vector3.up * originHeight;
        void Awake() { input = GetComponent<PrototypeInput>(); }
        void Update()
        {
            FindNearby();
            if (input.GameplayActive && input.Interact.WasPressedThisFrame()) TryInteract();
            if (Time.unscaledTime > statusUntil) Status = null;
        }
        public void FindNearby()
        {
            Current = null;
            Interactable aimed = null;
            float best = float.PositiveInfinity, bestAim = float.PositiveInfinity;
            int count = Physics.OverlapSphereNonAlloc(Origin, reach, nearby, interactionMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Interactable candidate = nearby[i].GetComponentInParent<Interactable>();
                if (!candidate || !candidate.CanInteract(this) || candidate == HeldBox || candidate == Current || candidate == aimed) continue;
                Vector3 point = candidate.InteractionPoint;
                float score = (point - Origin).sqrMagnitude;
                if (score > reach * reach) continue;
                float angle = aim ? Vector3.Angle(aim.forward, point - aim.position) : 180f;
                if (score >= best && (angle > aimConeDegrees || angle >= bestAim)) continue;
                if (Physics.Linecast(Origin, point, out RaycastHit hit, interactionMask, QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<Interactable>() != candidate) continue;
                if (score < best) { best = score; Current = candidate; }
                if (angle <= aimConeDegrees && angle < bestAim) { bestAim = angle; aimed = candidate; }
            }
            if (aimed) Current = aimed;
        }
        public bool TryInteract()
        {
            if (BusinessAsUsual.Multiplayer.NetworkSession.Active)
                return GetComponent<BusinessAsUsual.Multiplayer.NetworkPlayer>().RequestInteraction();
            if (HeldBox) return HeldBox.TryDrop(this);
            FindNearby();
            return Current && Current.Interact(this);
        }
        public void Notify(string message)
        {
            Status = message;
            statusUntil = Time.unscaledTime + 3;
        }
        void OnDisable() { if (HeldBox) HeldBox.ReleaseAtCurrentPosition(this); }
    }
}
