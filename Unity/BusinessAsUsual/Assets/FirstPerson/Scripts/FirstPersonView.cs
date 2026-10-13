using UnityEngine;
using UnityEngine.Rendering;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.FirstPerson
{
    // Owner-only first-person camera. Movement stays server-authoritative: this only aims the
    // existing player camera, supplies the look yaw sent with movement input and hides the
    // owner's own body (other processes keep rendering the full third-person Sunny).
    [DefaultExecutionOrder(-200)]
    public sealed class FirstPersonView : MonoBehaviour
    {
        [Header("View")]
        public float eyeHeight = 1.0f;
        public float minimumPitch = -78f;
        public float maximumPitch = 80f;
        public float nearClip = .03f;
        [Header("Comfort")]
        public float bobAmount = .014f;
        public float verticalSmoothing = .06f;
        [Header("Held loads in view")]
        public Vector3 carryOffset = new Vector3(0, .5f, .5f);

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public Camera ViewCamera { get; private set; }
        public bool Active { get; private set; }
        public float PlanarSpeed { get; private set; }
        public bool HiddenBody { get; private set; }
        public static FirstPersonView Local { get; private set; }

        PrototypeInput input;
        PlayerInteractor interactor;
        ThirdPersonCamera orbit;
        Renderer[] body;
        Transform carryAnchor;
        Vector3 anchorLocal;
        Quaternion anchorRotation;
        float smoothedY, yVelocity, bobPhase;
        Vector3 lastPosition;
        Vector2 lookDelta;
        public Vector2 LookDelta => lookDelta;

        void Awake()
        {
            input = GetComponent<PrototypeInput>();
            interactor = GetComponent<PlayerInteractor>();
            orbit = GetComponentInChildren<ThirdPersonCamera>(true);
            enabled = false; // NetworkPlayer enables this for the owning process only.
        }

        void OnEnable()
        {
            GameSettings.EnsureLoaded();
            if (!orbit) orbit = GetComponentInChildren<ThirdPersonCamera>(true);
            ViewCamera = orbit ? orbit.GetComponent<Camera>() : GetComponentInChildren<Camera>(true);
            if (orbit) { orbit.enabled = false; Yaw = orbit.yaw; }
            Pitch = 0;
            ViewCamera.nearClipPlane = nearClip;
            ViewCamera.fieldOfView = GameSettings.FieldOfView;
            smoothedY = transform.position.y;
            lastPosition = transform.position;
            body = GetComponentsInChildren<Renderer>(true);
            foreach (var r in body) r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            HiddenBody = true;
            var animator = GetComponentInChildren<Animator>();
            if (animator) animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // shadows still need the pose.
            if (interactor) { interactor.aim = ViewCamera.transform; carryAnchor = interactor.carryAnchor; }
            if (carryAnchor) { anchorLocal = carryAnchor.localPosition; anchorRotation = carryAnchor.localRotation; }
            GameSettings.Changed += ApplySettings;
            Local = this;
            Active = true;
            Place(0);
        }

        void OnDisable()
        {
            GameSettings.Changed -= ApplySettings;
            if (body != null) foreach (var r in body) if (r) r.shadowCastingMode = ShadowCastingMode.On;
            HiddenBody = false;
            if (interactor) interactor.aim = null;
            if (carryAnchor) { carryAnchor.localPosition = anchorLocal; carryAnchor.localRotation = anchorRotation; }
            if (Local == this) Local = null;
            Active = false;
        }

        void ApplySettings() { if (ViewCamera) ViewCamera.fieldOfView = GameSettings.FieldOfView; }

        // Validation and the opt-in network harness set an explicit view.
        public void SetView(float yaw, float pitch) { Yaw = Mathf.Repeat(yaw, 360); Pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch); Place(0); }

        void Update()
        {
            lookDelta = Vector2.zero;
            if (input && input.GameplayActive && !(BusinessAsUsual.Town.LivingWorkday.Instance && BusinessAsUsual.Town.LivingWorkday.Instance.SuppressLook))
            {
                // Mouse delta is a per-frame displacement; never scale it by deltaTime.
                lookDelta = input.Look.ReadValue<Vector2>() * GameSettings.Sensitivity;
                Yaw = Mathf.Repeat(Yaw + lookDelta.x, 360);
                Pitch = Mathf.Clamp(Pitch + (GameSettings.InvertY ? lookDelta.y : -lookDelta.y), minimumPitch, maximumPitch);
            }
        }

        void LateUpdate() { Place(Time.deltaTime); }

        void Place(float deltaTime)
        {
            if (!ViewCamera) return;
            Vector3 position = transform.position;
            if (deltaTime > 0)
            {
                Vector3 planar = position - lastPosition; planar.y = 0;
                PlanarSpeed = Mathf.Lerp(PlanarSpeed, planar.magnitude / deltaTime, 1 - Mathf.Exp(-12 * deltaTime));
                // Step offsets and server corrections shouldn't jolt the view; jumps stay responsive.
                smoothedY = Mathf.Abs(position.y - smoothedY) > .6f ? position.y : Mathf.SmoothDamp(smoothedY, position.y, ref yVelocity, verticalSmoothing);
                bobPhase += deltaTime * Mathf.Lerp(0, 11f, Mathf.Clamp01(PlanarSpeed / 4.2f)) * (PlanarSpeed > .25f ? 1 : 0);
            }
            else smoothedY = position.y;
            lastPosition = position;
            float bob = Mathf.Clamp01(PlanarSpeed / 4.2f) * bobAmount;
            Quaternion yaw = Quaternion.Euler(0, Yaw, 0);
            Vector3 eye = new Vector3(position.x, smoothedY + eyeHeight, position.z)
                + Vector3.up * (Mathf.Sin(bobPhase * 2) * bob) + yaw * Vector3.right * (Mathf.Cos(bobPhase) * bob * .5f);
            ViewCamera.transform.SetPositionAndRotation(eye, Quaternion.Euler(Pitch, Yaw, 0));
            if (orbit) { orbit.yaw = Yaw; orbit.pitch = Pitch; }
            // Held crates and bags sit in front of the view rather than the walking direction.
            if (carryAnchor) carryAnchor.SetPositionAndRotation(new Vector3(position.x, smoothedY, position.z) + yaw * carryOffset, yaw);
        }

        void OnGUI()
        {
            if (!Active || !input || !input.GameplayActive || PauseMenu.IsOpen) return;
            bool target = interactor && interactor.Current;
            float size = target ? 9 : 5;
            var c = new Rect(Screen.width / 2f - size / 2, Screen.height / 2f - size / 2, size, size);
            var old = GUI.color;
            GUI.color = new Color(.14f, .25f, .28f, .55f); GUI.DrawTexture(new Rect(c.x - 1, c.y - 1, c.width + 2, c.height + 2), Texture2D.whiteTexture);
            GUI.color = target ? new Color(.94f, .78f, .39f) : new Color(.96f, .91f, .79f); GUI.DrawTexture(c, Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
