using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Town;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.FirstPerson
{
    // Owner-only viewmodel built from Sunny's own rig and forearm/hand meshes (no new character).
    // Poses are procedural two-bone IK. Every gesture is presentation: it reacts to authoritative
    // state (holder, work, restock RPCs, server feedback) and never sends commands or moves stock.
    [DefaultExecutionOrder(200)]
    public sealed class FirstPersonHands : MonoBehaviour
    {
        public enum Gesture { None, Press, Reach, Pickup, Place, Restock, ToolTurn, Probe, Sort }
        public GameObject armsSource;
        [Header("Rig placement in camera space")]
        // Sunny's fists are large; a modest scale keeps them readable without filling the view.
        public float rigScale = 1f;
        public Vector3 shoulderOffset = new Vector3(0, -.30f, .30f);
        [Range(.5f,1f)] public float handProportion = .65f;
        public bool proportionsPrepared;
        [Header("Rest hands in camera space")]
        public Vector3 restRight = new Vector3(.24f, -.25f, .46f);
        public Vector3 restLeft = new Vector3(-.24f, -.25f, .46f);
        public float wallClearance = .55f;
        [Header("Hand tools (local viewmodel only)")]
        public Material toolMetal;
        public Material toolHandle;

        public bool Built => rig;
        public Gesture Current { get; private set; }
        public string Pose { get; private set; } = "idle";
        public int GesturesPlayed { get; private set; }
        public bool Visible => rig && rig.activeSelf;
        // Editor evidence/tuning only: shows a pose without authoritative state. Never set in play.
        [System.NonSerialized] public TownStation previewStation;
        [System.NonSerialized] public int previewCarry;

        FirstPersonView view;
        NetworkPlayer player;
        GameObject rig;
        Transform root;
        Arm left, right;
        Transform wrench, magnifier;
        float gestureTime, gestureLength, bob, landKick, jumpLift, pullBack, stepDistance;
        Vector3 gestureTarget, sway;
        Vector3 rigBasePosition;
        readonly List<Mesh> viewmodelMeshes = new List<Mesh>();
        bool wasCarrying, wasGrounded = true;
        float lastY, verticalSpeed;
        int workTicks;

        sealed class Arm
        {
            public Transform shoulder, upper, lower, hand;
            public Quaternion shoulderRest, upperRest, lowerRest, handRest;
            public float upperLength, lowerLength;
            public float side;
            public void Reset() { shoulder.localRotation = shoulderRest; upper.localRotation = upperRest; lower.localRotation = lowerRest; hand.localRotation = handRest; }
        }

        void Awake() { view = GetComponent<FirstPersonView>(); player = GetComponent<NetworkPlayer>(); enabled = false; }

        void OnEnable()
        {
            if (!view) view = GetComponent<FirstPersonView>();
            if (!rig) Build();
            if (rig) rig.SetActive(true);
            lastY = transform.position.y;
        }
        void OnDisable() { if (rig) rig.SetActive(false); }
        void OnDestroy() { if (rig) Destroy(rig); ClearMeshes(); }
        void ClearMeshes(){foreach(var mesh in viewmodelMeshes)if(mesh)Destroy(mesh);viewmodelMeshes.Clear();}
        // Re-creates the viewmodel after placement fields change (tuning and validation).
        public void Rebuild() { if (rig) DestroyImmediate(rig); ClearMeshes(); rig = null; left = right = null; if (isActiveAndEnabled) Build(); }

        void Build()
        {
            if (!armsSource || !view || !view.ViewCamera) return;
            rig = Instantiate(armsSource);
            rig.name = "First-person Sunny arms (local only)";
            rig.hideFlags = HideFlags.DontSave;
            foreach (var a in rig.GetComponentsInChildren<Animator>(true)) a.enabled = false;
            foreach (var c in rig.GetComponentsInChildren<Collider>(true)) Destroy(c);
            foreach (var r in rig.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                bool keep = r.name.StartsWith("ForearmHand");
                r.enabled = keep;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.updateWhenOffscreen = true;
                if (keep && !proportionsPrepared && r.sharedMesh.isReadable)
                {var mesh=RefinedMesh(r,handProportion);if(mesh){r.sharedMesh=mesh;viewmodelMeshes.Add(mesh);}}
            }
            root = rig.transform;
            root.SetParent(view.ViewCamera.transform, false);
            root.localScale = Vector3.one * rigScale;
            left = MakeArm("Left", -1);
            right = MakeArm("Right", 1);
            if (left == null || right == null) { Destroy(rig); rig = null; return; }
            // Put the midpoint between the shoulders at the requested camera-space offset.
            root.localRotation = Quaternion.identity;
            root.localPosition = Vector3.zero;
            Vector3 mid = view.ViewCamera.transform.InverseTransformPoint((left.upper.position + right.upper.position) * .5f);
            root.localPosition = shoulderOffset - mid;
            rigBasePosition = root.localPosition;
            wrench = Tool("Wrench", new[] { (new Vector3(0, 0, -.02f), new Vector3(.022f, .014f, .15f), true), (new Vector3(0, 0, .07f), new Vector3(.06f, .016f, .035f), true), (new Vector3(0, 0, -.06f), new Vector3(.03f, .024f, .07f), false) });
            magnifier = Tool("Magnifier", new[] { (new Vector3(0, 0, .065f), new Vector3(.075f, .008f, .075f), true), (new Vector3(0, 0, -.02f), new Vector3(.02f, .02f, .09f), false) }, PrimitiveType.Cylinder);
        }

        // Export-copy proportions only: preserve UVs, rig reach and source meshes. Blend
        // the reduction through hand weights so the wrist tapers instead of forming a cuff gap.
        public static Mesh RefinedMesh(SkinnedMeshRenderer renderer,float handProportion)
        {
            int index=System.Array.FindIndex(renderer.bones,b=>b&&b.name.EndsWith(":LeftHand")||b&&b.name.EndsWith(":RightHand"));
            if(index<0||!renderer.sharedMesh)return null;
            var mesh=Instantiate(renderer.sharedMesh);mesh.name="Local refined "+renderer.name;mesh.hideFlags=HideFlags.DontSave;
            var vertices=mesh.vertices;var weights=mesh.boneWeights;
            Vector3 wrist=renderer.transform.InverseTransformPoint(renderer.bones[index].position);
            for(int i=0;i<vertices.Length;i++)
            {
                var w=weights[i];float weight=(w.boneIndex0==index?w.weight0:0)+(w.boneIndex1==index?w.weight1:0)+(w.boneIndex2==index?w.weight2:0)+(w.boneIndex3==index?w.weight3:0);
                float proportion=Mathf.Lerp(1,handProportion,Mathf.SmoothStep(0,1,weight));vertices[i]=wrist+(vertices[i]-wrist)*proportion;
            }
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        // Tiny tool props built from primitives in the existing town palette (no new assets to license).
        Transform Tool(string name, (Vector3 position, Vector3 size, bool metal)[] parts, PrimitiveType head = PrimitiveType.Cube)
        {
            var tool = new GameObject(name).transform; tool.SetParent(root, false);
            for (int i = 0; i < parts.Length; i++)
            {
                var part = GameObject.CreatePrimitive(i == 0 ? head : PrimitiveType.Cube);
                Destroy(part.GetComponent<Collider>());
                part.transform.SetParent(tool, false); part.transform.localPosition = parts[i].position; part.transform.localScale = parts[i].size;
                var r = part.GetComponent<Renderer>(); r.shadowCastingMode = ShadowCastingMode.Off;
                var m = parts[i].metal ? toolMetal : toolHandle; if (m) r.sharedMaterial = m;
            }
            tool.gameObject.SetActive(false);
            return tool;
        }

        Arm MakeArm(string side, float sign)
        {
            Transform Find(string name) { foreach (var t in rig.GetComponentsInChildren<Transform>(true)) if (t.name == "mixamorig:" + side + name) return t; return null; }
            var arm = new Arm { shoulder = Find("Shoulder"), upper = Find("Arm"), lower = Find("ForeArm"), hand = Find("Hand"), side = sign };
            if (!arm.shoulder || !arm.upper || !arm.lower || !arm.hand) return null;
            arm.shoulderRest = arm.shoulder.localRotation; arm.upperRest = arm.upper.localRotation; arm.lowerRest = arm.lower.localRotation; arm.handRest = arm.hand.localRotation;
            arm.upperLength = Vector3.Distance(arm.upper.position, arm.lower.position);
            arm.lowerLength = Vector3.Distance(arm.lower.position, arm.hand.position);
            return arm;
        }

        // Called from authoritative events.
        public void Play(Gesture gesture, Vector3? worldTarget = null)
        {
            Current = gesture; gestureTime = 0; GesturesPlayed++;
            gestureLength = gesture == Gesture.Press ? .32f : gesture == Gesture.Restock ? ShopRestockReach.Duration : gesture == Gesture.Reach ? .45f : .5f;
            gestureTarget = worldTarget ?? (view.ViewCamera.transform.position + view.ViewCamera.transform.forward * .6f);
        }

        void LateUpdate()
        {
            if (!rig || !view || !view.ViewCamera || left == null || right == null) return;
            float dt = Time.deltaTime;
            var cam = view.ViewCamera.transform;
            left.Reset(); right.Reset();

            // Locomotion signals from the interpolated authoritative position.
            verticalSpeed = dt > 0 ? Mathf.Lerp(verticalSpeed, (transform.position.y - lastY) / dt, 1 - Mathf.Exp(-15 * dt)) : 0;
            lastY = transform.position.y;
            bool grounded = player ? player.Grounded.Value : true;
            if (grounded && !wasGrounded) { landKick = .055f; TownAudio.Play(TownAudio.Cue.Land, transform.position, .6f); }
            wasGrounded = grounded;
            landKick = Mathf.MoveTowards(landKick, 0, dt * .3f);
            jumpLift = Mathf.Lerp(jumpLift, grounded ? 0 : Mathf.Clamp(verticalSpeed * .012f, -.03f, .04f), 1 - Mathf.Exp(-10 * dt));
            float speed = view.PlanarSpeed, run = Mathf.Clamp01((speed - 2.4f) / 1.8f);
            if (grounded) bob += dt * (speed > .2f ? Mathf.Lerp(7.5f, 10.5f, run) : 1.4f);
            float walk = Mathf.Clamp01(speed / 2.2f);
            Vector3 motion = new Vector3(Mathf.Cos(bob) * .012f * walk, -Mathf.Abs(Mathf.Sin(bob)) * (.012f + .018f * run) * walk + Mathf.Sin(bob * .5f) * .003f * (1 - walk), 0);
            sway = Vector3.Lerp(sway, new Vector3(-view.LookDelta.x, -view.LookDelta.y, 0) * .006f, 1 - Mathf.Exp(-8 * dt));
            sway = Vector3.ClampMagnitude(sway, .04f);
            Vector3 common = motion + sway + Vector3.up * (jumpLift - landKick) + Vector3.down * (run * .03f);

            // Keep the viewmodel out of walls rather than rendering an extra camera.
            float wanted = 0;
            if (Physics.SphereCast(cam.position, .08f, cam.forward, out var hit, wallClearance, player ? player.Interactor.interactionMask : ~0, QueryTriggerInteraction.Ignore)) wanted = (wallClearance - hit.distance) * .7f;
            pullBack = Mathf.Lerp(pullBack, wanted, 1 - Mathf.Exp(-14 * dt));
            Vector3 avoid = Vector3.back * pullBack + Vector3.down * pullBack * .4f;

            Vector3 r = restRight + common + avoid, l = restLeft + common + avoid;
            ulong me = player ? player.OwnerClientId : 0;
            var shop = NetworkShop.Instance; var jobs = shop ? shop.Town : null;
            bool crate = shop && shop.Holder == me || previewCarry == 1, bag = jobs && jobs.HasBag(me) || shop && shop.Workday && shop.Workday.Holding(me) || previewCarry == 2, carrying = crate || bag;
            root.localPosition = rigBasePosition + (carrying ? new Vector3(0,-.14f,.16f) : Vector3.zero);
            if (carrying != wasCarrying) { Play(carrying ? Gesture.Pickup : Gesture.Place); TownAudio.Play(carrying ? TownAudio.Cue.Pickup : TownAudio.Cue.Place, transform.position, .8f); }
            wasCarrying = carrying;
            Pose = "idle";
            if (carrying && player && player.Interactor.carryAnchor)
            {
                // Hands grip the sides of the real carried object (crate) or its neck (bag).
                Vector3 anchor = cam.InverseTransformPoint(player.Interactor.carryAnchor.position);
                float half = crate ? .3f : .16f;
                r = anchor + new Vector3(half, crate ? .27f : .2f, crate ? -.22f : -.02f) + common * .5f;
                l = anchor + new Vector3(-half, crate ? .27f : .2f, crate ? -.22f : -.02f) + common * .5f;
                Pose = crate ? "carry-crate" : "carry-bag";
            }
            bool working = jobs && jobs.IsWorking(me) || previewStation;
            if (working)
            {
                var station = previewStation ? previewStation : player.Interactor.Current as TownStation;
                var kind = station ? station.kind : TownStation.Kind.Repair;
                float t = Time.time;
                Vector3 target = station ? cam.InverseTransformPoint(station.InteractionPoint) : new Vector3(0, -.15f, .5f);
                target = new Vector3(Mathf.Clamp(target.x, -.12f, .12f), Mathf.Clamp(target.y, -.22f, -.02f), Mathf.Clamp(target.z, .36f, .44f));
                if (kind == TownStation.Kind.Repair)
                {
                    // Wrench rhythm: right hand strikes, left hand braces the panel.
                    float strike = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 5.2f)), 3);
                    r = target + new Vector3(.15f, .04f - strike * .07f, -.03f + strike * .03f);
                    l = target + new Vector3(-.2f, -.03f, .0f);
                    int tick = Mathf.FloorToInt(t * 5.2f / Mathf.PI); if (tick != workTicks) { workTicks = tick; TownAudio.Play(TownAudio.Cue.Tool, station ? station.InteractionPoint : cam.position, .55f); }
                    Pose = "repair";
                }
                else if (kind == TownStation.Kind.Inspect)
                {
                    // Inspection: left hand shades a careful look, right hand traces the panel.
                    r = target + new Vector3(.06f + Mathf.Sin(t * 1.7f) * .06f, Mathf.Cos(t * 1.3f) * .03f, 0);
                    l = new Vector3(-.13f, -.08f, .26f) + common;
                    Pose = "inspect";
                }
                else
                {
                    // Sorting: keep the carry grip and heave the bag forward and up into the bin.
                    float push = (Mathf.Sin(t * 3.2f) + 1) * .5f;
                    Vector3 heave = new Vector3(0, .03f + push * .05f, .04f + push * .07f);
                    r = (carrying ? r : restRight) + heave; l = (carrying ? l : restLeft) + heave;
                    Pose = "sort";
                }
            }

            if (Current != Gesture.None)
            {
                gestureTime += dt;
                float n = Mathf.Clamp01(gestureTime / gestureLength);
                float w = n < .45f ? Mathf.SmoothStep(0, 1, n / .45f) : Mathf.SmoothStep(1, 0, (n - .55f) / .45f);
                Vector3 target = cam.InverseTransformPoint(gestureTarget);
                target = new Vector3(Mathf.Clamp(target.x, -.2f, .22f), Mathf.Clamp(target.y, -.3f, .05f), Mathf.Clamp(target.z, .3f, .48f));
                switch (Current)
                {
                    case Gesture.Press: r = Vector3.Lerp(r, new Vector3(.08f, -.12f, .42f) + common, w); Pose = "press"; break;
                    case Gesture.Reach: case Gesture.Restock: r = Vector3.Lerp(r, target, w); Pose = Current == Gesture.Restock ? "restock" : "reach"; break;
                    case Gesture.Pickup: r = Vector3.Lerp(r, r + new Vector3(0, -.12f, .06f), w * (1 - n)); l = Vector3.Lerp(l, l + new Vector3(0, -.12f, .06f), w * (1 - n)); Pose = "pickup"; break;
                    case Gesture.Place: r = Vector3.Lerp(r, new Vector3(.2f, -.3f, .42f), w); l = Vector3.Lerp(l, new Vector3(-.2f, -.3f, .42f), w); Pose = "place"; break;
                    case Gesture.ToolTurn: r=Vector3.Lerp(r,target,w)+new Vector3(Mathf.Sin(n*Mathf.PI*2)*.04f,0,0);Pose="repair";break;
                    case Gesture.Probe: r=Vector3.Lerp(r,target,w);Pose="inspect";break;
                    case Gesture.Sort: r+=Vector3.forward*w*.12f;l+=Vector3.forward*w*.12f;Pose="sort";break;
                }
                if (n >= 1) Current = Gesture.None;
            }

            // Keep projected composition steady at 60–95 degrees without a second camera.
            // Carry grips follow the real object and therefore must not use this correction.
            if (!carrying)
            {
                float framing = Mathf.Tan(view.ViewCamera.fieldOfView * Mathf.Deg2Rad * .5f) / Mathf.Tan(74 * Mathf.Deg2Rad * .5f);
                r.x *= framing; r.y *= framing; l.x *= framing; l.y *= framing;
            }
            Solve(right, cam.TransformPoint(r), cam);
            Solve(left, cam.TransformPoint(l), cam);
            bool showWrench = Pose == "repair", showLens = Pose == "inspect";
            if (wrench) { wrench.gameObject.SetActive(showWrench); if (showWrench) wrench.SetPositionAndRotation(right.hand.position + (cam.forward * .07f + cam.up * .04f - cam.right * .02f) * rigScale, Quaternion.LookRotation(cam.up * .85f + cam.forward * .5f - cam.right * .3f, -cam.forward)); }
            if (magnifier) { magnifier.gameObject.SetActive(showLens); if (showLens) magnifier.SetPositionAndRotation(right.hand.position + cam.forward * .04f * rigScale, Quaternion.LookRotation(cam.up * .9f + cam.forward * .4f, -cam.forward)); }
        }

        static void Solve(Arm arm, Vector3 target, Transform cam)
        {
            Vector3 shoulder = arm.upper.position;
            // Lengths were measured in world space at build time, with the rig already scaled.
            float upper = arm.upperLength, lower = arm.lowerLength;
            Vector3 toTarget = target - shoulder;
            float distance = Mathf.Clamp(toTarget.magnitude, Mathf.Abs(upper - lower) + .002f, upper + lower - .002f);
            Vector3 direction = toTarget.normalized;
            Vector3 pole = Vector3.ProjectOnPlane(cam.right * arm.side * .8f + -cam.up, direction).normalized;
            float along = (upper * upper - lower * lower + distance * distance) / (2f * distance);
            Vector3 elbow = shoulder + direction * along + pole * Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
            arm.upper.rotation = Quaternion.FromToRotation(arm.lower.position - shoulder, elbow - shoulder) * arm.upper.rotation;
            arm.lower.rotation = Quaternion.FromToRotation(arm.hand.position - arm.lower.position, shoulder + direction * distance - arm.lower.position) * arm.lower.rotation;
            // Hands face forward-down, palms slightly inward, as if ready to work.
            Vector3 fingers = (cam.forward * 1f - cam.up * .25f + cam.right * -arm.side * .15f).normalized;
            arm.hand.rotation = Quaternion.FromToRotation(arm.hand.up, fingers) * arm.hand.rotation;
        }
    }
}
