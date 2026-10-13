using UnityEngine;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper
{
    // Presentation only: UseShelf remains the single synchronous inventory transaction.
    // No animation callback can transfer inventory, including cancellation/disable.
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(ThirdPersonMotor))]
    public sealed class ShopRestockReach : MonoBehaviour
    {
        public const float Duration = .65f;
        public const float PlacementTime = .28f;
        ThirdPersonMotor motor;
        Transform spine, arm, forearm, hand;
        readonly Quaternion[] basePose = new Quaternion[3];
        bool posed, cancelling;
        float elapsed, cancelElapsed, cancelWeight;
        Vector3 target, beganAt;
        string sku;
        public bool IsActive { get; private set; }
        public string ActivityLabel { get; private set; }
        public bool RigSupported => spine && arm && forearm && hand;

        void Awake()
        {
            motor = GetComponent<ThirdPersonMotor>();
            if (!motor.animator) return;
            foreach (var bone in motor.animator.GetComponentsInChildren<Transform>())
            {
                switch (bone.name)
                {
                    case "mixamorig:Spine2": spine = bone; break;
                    case "mixamorig:RightArm": arm = bone; break;
                    case "mixamorig:RightForeArm": forearm = bone; break;
                    case "mixamorig:RightHand": hand = bone; break;
                }
            }
        }

        public void Begin(ShopStation station)
        {
            RestorePose();
            if (!enabled || !RigSupported) return;
            sku = station.sku;
            ActivityLabel = "Stocking " + ShopShift.Products[System.Array.FindIndex(ShopShift.Products, p => p.sku == sku)].name + "…";
            target = station.InteractionPoint + Vector3.up * .12f + Vector3.back * .35f;
            beganAt = transform.position;
            elapsed = cancelElapsed = 0;
            cancelling = false;
            IsActive = true;
        }

        public bool HoldsDisplay(string productSku) => IsActive && !cancelling && elapsed < PlacementTime && sku == productSku;

        static float Weight(float time)
        {
            if (time < PlacementTime) return Mathf.SmoothStep(0, 1, time / PlacementTime);
            return Mathf.SmoothStep(1, 0, (time - .38f) / (Duration - .38f));
        }

        void Update()
        {
            // Remove our last delta before Animator evaluation; never accumulate bone rotation.
            RestorePose();
            if (IsActive && !cancelling && (motor.PlanarSpeed > .2f || (transform.position - beganAt).sqrMagnitude > .0064f))
                Cancel();
            Advance(Time.deltaTime);
        }

        // Deterministic time stepping for the existing synchronous integration validator.
        public void Advance(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0) return;
            if (cancelling) cancelElapsed += deltaTime;
            else elapsed += deltaTime;
            if ((cancelling && cancelElapsed >= .12f) || elapsed >= Duration)
            {
                RestorePose();
                IsActive = false;
            }
        }

        public void Cancel()
        {
            if (!IsActive || cancelling) return;
            cancelWeight = Weight(elapsed);
            cancelling = true;
            cancelElapsed = 0;
        }

        void LateUpdate()
        {
            RestorePose();
            if (!IsActive || !RigSupported) return;
            float weight = cancelling ? cancelWeight * Mathf.SmoothStep(1, 0, cancelElapsed / .12f) : Weight(elapsed);
            basePose[0] = spine.localRotation; basePose[1] = arm.localRotation; basePose[2] = forearm.localRotation;
            posed = true;
            spine.rotation = Quaternion.AngleAxis(5f * weight, motor.visual.right) * spine.rotation;
            // Generic-rig two-bone reach, blended over the evaluated locomotion pose.
            Vector3 shoulder = arm.position;
            float upper = Vector3.Distance(shoulder, forearm.position);
            float lower = Vector3.Distance(forearm.position, hand.position);
            Vector3 direction = target - shoulder;
            float distance = Mathf.Clamp(direction.magnitude, Mathf.Abs(upper - lower) + .002f, upper + lower - .002f);
            direction.Normalize();
            Vector3 pole = motor.visual.right * .7f + Vector3.down;
            pole = Vector3.ProjectOnPlane(pole, direction).normalized;
            float along = (upper * upper - lower * lower + distance * distance) / (2f * distance);
            Vector3 elbow = shoulder + direction * along + pole * Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
            Quaternion aimedArm = Quaternion.FromToRotation(forearm.position - shoulder, elbow - shoulder) * arm.rotation;
            arm.rotation = Quaternion.Slerp(arm.rotation, aimedArm, weight);
            Quaternion aimedForearm = Quaternion.FromToRotation(hand.position - forearm.position, shoulder + direction * distance - forearm.position) * forearm.rotation;
            forearm.rotation = Quaternion.Slerp(forearm.rotation, aimedForearm, weight);
        }

        void RestorePose()
        {
            if (!posed) return;
            if (spine) spine.localRotation = basePose[0];
            if (arm) arm.localRotation = basePose[1];
            if (forearm) forearm.localRotation = basePose[2];
            posed = false;
        }

        void OnDisable() { RestorePose(); IsActive = false; }
    }
}
