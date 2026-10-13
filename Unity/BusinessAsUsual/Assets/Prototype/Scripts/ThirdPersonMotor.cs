using UnityEngine;

namespace BusinessAsUsual.Prototype
{
    [RequireComponent(typeof(CharacterController), typeof(PrototypeInput))]
    public sealed class ThirdPersonMotor : MonoBehaviour
    {
        [Header("Movement (metres / seconds)")]
        [Min(0.1f)] public float walkSpeed = 2.2f;
        [Min(0.1f)] public float sprintSpeed = 4.2f;
        [Min(0.1f)] public float acceleration = 16f;
        [Min(1f)] public float turnSpeed = 720f;
        [Min(0.1f)] public float jumpHeight = 0.7f;
        public float gravity = -20f;
        public float terminalVelocity = -40f;
        public Transform cameraTransform;
        public Transform visual;
        public Animator animator;
        public Vector3 respawnPosition;
        public float respawnBelowY = -5f;

        public float PlanarSpeed => planarVelocity.magnitude;
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Controller { get; private set; }
        public bool Grounded => Controller != null && Controller.isGrounded;
        PrototypeInput input;
        Vector3 planarVelocity;
        float verticalVelocity;
        static readonly int Speed = Animator.StringToHash("Speed");

        void Awake()
        {
            QualitySettings.skinWeights = SkinWeights.Unlimited;
            Controller = GetComponent<CharacterController>();
            input = GetComponent<PrototypeInput>();
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        }
        void Update()
        {
            bool active = input.GameplayActive;
            Step(active ? input.Move.ReadValue<Vector2>() : Vector2.zero,
                active && input.Sprint.IsPressed(), active && input.Jump.WasPressedThisFrame(), Time.deltaTime);
            if (transform.position.y < respawnBelowY) Teleport(respawnPosition);
        }
        // The same motor step is used by gameplay and deterministic integration validation.
        public void Step(Vector2 movement, bool sprint, bool jump, float deltaTime)
        {
            if (deltaTime <= 0 || !Controller.enabled) return;
            movement = Vector2.ClampMagnitude(movement, 1);
            Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = right * movement.x + forward * movement.y;
            planarVelocity = Vector3.MoveTowards(planarVelocity, direction * (sprint ? sprintSpeed : walkSpeed), acceleration * deltaTime);
            if (Controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            if (jump && Controller.isGrounded) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity = Mathf.Max(terminalVelocity, verticalVelocity + gravity * deltaTime);
            CollisionFlags collision = Controller.Move((planarVelocity + Vector3.up * verticalVelocity) * deltaTime);
            if ((collision & CollisionFlags.Above) != 0 && verticalVelocity > 0) verticalVelocity = 0;
            if (direction.sqrMagnitude > 0.001f && visual)
                visual.rotation = Quaternion.RotateTowards(visual.rotation, Quaternion.LookRotation(direction), turnSpeed * deltaTime);
            // Clip speeds are prototype pacing; these short-strided cycles visibly slide at town speeds.
            if (animator) animator.SetFloat(Speed, PlanarSpeed, 0.12f, deltaTime);
        }
        public void Teleport(Vector3 position)
        {
            Controller.enabled = false;
            transform.position = position;
            Controller.enabled = true;
            planarVelocity = Vector3.zero;
            verticalVelocity = 0;
            Physics.SyncTransforms();
        }
    }
}
