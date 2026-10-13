using UnityEngine;
using BusinessAsUsual.Multiplayer;

namespace BusinessAsUsual.FirstPerson
{
    // Presentation for every copy of Sunny (server, owner shadow, remote clients):
    // stride-matched cadence, footsteps, airborne/landing pose, idle breathing and head look.
    // Runs after the Animator and the restock reach; it only adds small offsets to bones the
    // locomotion clips animate every frame, so nothing accumulates.
    [DefaultExecutionOrder(100)]
    public sealed class AvatarPresentation : MonoBehaviour
    {
        // The v004 clips are short-strided; cadence rises with speed to reduce, not hide, foot slide.
        public float walkCadence = 1.45f, runCadence = 1.85f;
        public float stepLength = .62f;
        public float maxHeadYaw = 65f, maxHeadPitch = 35f;
        public float Cadence { get; private set; } = 1;
        public float AirWeight { get; private set; }
        public int Steps { get; private set; }

        NetworkPlayer player;
        Animator animator;
        Transform spine, neck, head, leftThigh, rightThigh, leftKnee, rightKnee, leftArm, rightArm;
        float distance, breathe, landSquash, airWeight;
        bool wasGrounded = true;
        Vector3 last;

        void Awake()
        {
            player = GetComponent<NetworkPlayer>();
            animator = GetComponentInChildren<Animator>();
            if (!animator) return;
            foreach (var t in animator.GetComponentsInChildren<Transform>(true))
                switch (t.name)
                {
                    case "mixamorig:Spine2": spine = t; break;
                    case "mixamorig:Neck": neck = t; break;
                    case "mixamorig:Head": head = t; break;
                    case "mixamorig:LeftUpLeg": leftThigh = t; break;
                    case "mixamorig:RightUpLeg": rightThigh = t; break;
                    case "mixamorig:LeftLeg": leftKnee = t; break;
                    case "mixamorig:RightLeg": rightKnee = t; break;
                    case "mixamorig:LeftArm": leftArm = t; break;
                    case "mixamorig:RightArm": rightArm = t; break;
                }
            last = transform.position;
        }

        void Update()
        {
            if (!animator || !player || !player.IsSpawned) return;
            float speed = player.Speed.Value;
            float run = Mathf.InverseLerp(2.2f, 4.2f, speed), walk = Mathf.Clamp01(speed / 2.2f);
            Cadence = Mathf.Lerp(1, Mathf.Lerp(walkCadence, runCadence, run), walk);
            animator.speed = Cadence;
            // Footsteps from actual travelled distance on the ground.
            Vector3 p = transform.position, planar = p - last; planar.y = 0; last = p;
            bool grounded = player.Grounded.Value;
            if (grounded && planar.magnitude < 1)
            {
                distance += planar.magnitude;
                float stride = Mathf.Lerp(stepLength, stepLength * 1.35f, run);
                if (distance >= stride) { distance = 0; Steps++; TownAudio.Play(TownAudio.Cue.Step, p, player.IsOwner ? .45f : .7f); }
            }
            if (grounded && !wasGrounded) { landSquash = 1; if (!player.IsOwner) TownAudio.Play(TownAudio.Cue.Land, p, .7f); }
            wasGrounded = grounded;
            landSquash = Mathf.MoveTowards(landSquash, 0, Time.deltaTime * 5);
            airWeight = Mathf.MoveTowards(airWeight, grounded ? 0 : 1, Time.deltaTime * (grounded ? 8 : 5));
            AirWeight = airWeight;
            breathe += Time.deltaTime * Mathf.Lerp(1.6f, 3.2f, walk);
        }

        void LateUpdate()
        {
            if (!animator || !player || !player.IsSpawned) return;
            var body = player.Motor.visual;
            float walk = Mathf.Clamp01(player.Speed.Value / 2.2f);
            // Idle breathing and a small landing squash on the chest.
            if (spine) spine.rotation = Quaternion.AngleAxis(Mathf.Sin(breathe) * 1.6f * (1 - walk) + landSquash * 9f, body.right) * spine.rotation;
            // Airborne tuck: knees lift and arms rise a little, then settle on landing.
            float air = airWeight * airWeight * (3 - 2 * airWeight);
            if (air > .001f)
            {
                if (leftThigh) leftThigh.rotation = Quaternion.AngleAxis(-28 * air, body.right) * leftThigh.rotation;
                if (rightThigh) rightThigh.rotation = Quaternion.AngleAxis(-18 * air, body.right) * rightThigh.rotation;
                if (leftKnee) leftKnee.rotation = Quaternion.AngleAxis(40 * air, body.right) * leftKnee.rotation;
                if (rightKnee) rightKnee.rotation = Quaternion.AngleAxis(30 * air, body.right) * rightKnee.rotation;
                if (leftArm) leftArm.rotation = Quaternion.AngleAxis(-25 * air, body.forward) * leftArm.rotation;
                if (rightArm) rightArm.rotation = Quaternion.AngleAxis(25 * air, body.forward) * rightArm.rotation;
            }
            // Remote copies show where their player looks (public, visible information only).
            if (!player.IsOwner && head)
            {
                float yaw = Mathf.Clamp(Mathf.DeltaAngle(body.eulerAngles.y, player.LookYaw.Value), -maxHeadYaw, maxHeadYaw);
                float pitch = Mathf.Clamp(player.LookPitch.Value, -maxHeadPitch, maxHeadPitch);
                var turn = Quaternion.AngleAxis(yaw * .6f, Vector3.up) * Quaternion.AngleAxis(pitch * .6f, body.right);
                if (neck) neck.rotation = Quaternion.Slerp(Quaternion.identity, turn, .4f) * neck.rotation;
                head.rotation = Quaternion.Slerp(Quaternion.identity, turn, .6f) * head.rotation;
            }
        }
    }
}
