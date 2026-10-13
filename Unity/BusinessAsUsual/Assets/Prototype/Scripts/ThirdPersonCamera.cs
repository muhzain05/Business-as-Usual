using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public PrototypeInput input;
        public float targetHeight = 0.85f;
        public float distance = 3.8f;
        public float sensitivity = 0.12f;
        public float minimumPitch = -20f;
        public float maximumPitch = 70f;
        public float yaw;
        public float pitch = 20f;
        public float collisionRadius = 0.18f;
        public LayerMask obstructionMask = ~((1 << 2));

        void LateUpdate()
        {
            if (!target) return;
            if (input && input.GameplayActive) Orbit(input.Look.ReadValue<Vector2>());
            PositionCamera();
        }
        public void Orbit(Vector2 delta)
        {
            // Mouse delta is per-frame displacement; do not multiply by deltaTime.
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minimumPitch, maximumPitch);
        }
        public void PositionCamera()
        {
            if (!target) return;
            Vector3 pivot = target.position + Vector3.up * targetHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float length = distance;
            if (Physics.SphereCast(pivot, collisionRadius, backward, out RaycastHit hit, distance, obstructionMask, QueryTriggerInteraction.Ignore))
                length = Mathf.Max(0.3f, hit.distance - 0.05f);
            transform.SetPositionAndRotation(pivot + backward * length, rotation);
        }
    }
}
