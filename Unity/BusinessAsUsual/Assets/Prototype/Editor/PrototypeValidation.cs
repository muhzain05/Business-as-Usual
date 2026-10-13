using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace BusinessAsUsual.Prototype.Editor
{
    [InitializeOnLoad]
    public static class PrototypeValidation
    {
        [Serializable] public class Check { public string name; public bool passed; public string details; }
        [Serializable] public class Report
        {
            public string unityVersion, scene, method, timestampUtc;
            public bool humanInputTested;
            public List<Check> checks = new List<Check>();
        }
        static double began;
        static bool waiting;
        const string SessionKey = "BAU.Phase3.Validate";
        static string Diagnostics => Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Docs/Diagnostics/Phase3"));
        static PrototypeValidation() { EditorApplication.playModeStateChanged += StateChanged; }
        [MenuItem("Business as Usual/Phase 3/Validate in Play Mode")]
        public static void Begin()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before starting validation.");
            if(SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the active scene before validation.");
            EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath);
            SessionState.SetBool(SessionKey,true);
            EditorApplication.isPlaying=true;
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(SessionKey,false)) return;
            began=EditorApplication.timeSinceStartup; waiting=true;
            EditorApplication.update += WaitForRuntime;
        }
        static void WaitForRuntime()
        {
            if(!waiting || EditorApplication.timeSinceStartup-began<1.5) return;
            waiting=false; EditorApplication.update-=WaitForRuntime;
            SessionState.SetBool(SessionKey,false);
            Run();
        }
        static void Run()
        {
            Directory.CreateDirectory(Diagnostics);
            var report=new Report { unityVersion=Application.unityVersion,scene=SceneManager.GetActiveScene().path,timestampUtc=DateTime.UtcNow.ToString("o"),humanInputTested=false,method="Live Unity Play mode; synthetic Input System device reads plus deterministic calls to production motor, camera and interaction methods. No human keyboard/mouse session." };
            ThirdPersonMotor motor=null;
            try
            {
                motor=UnityEngine.Object.FindFirstObjectByType<ThirdPersonMotor>();
                var input=motor.GetComponent<PrototypeInput>();
                var player=motor.GetComponent<PlayerInteractor>();
                var camera=motor.GetComponentInChildren<ThirdPersonCamera>();
                motor.enabled=false; player.enabled=false; camera.enabled=false;
                var box=UnityEngine.Object.FindFirstObjectByType<CarryableBox>();
                var door=UnityEngine.Object.FindFirstObjectByType<SimpleDoor>();
                var workstation=UnityEngine.Object.FindObjectsByType<BasicWorkstation>(FindObjectsSortMode.None).First(w=>w.stationName.Contains("garage"));
                Add(report,"scene_spawn",report.scene==PrototypeBuilder.ScenePath && motor.Controller.enabled,"Player, controller and prototype scene loaded in Play mode.");
                Add(report,"prefab_configuration",motor.visual && motor.animator && motor.cameraTransform && player.carryAnchor && motor.animator.avatar.isValid && !motor.animator.avatar.isHuman,"Valid Generic avatar; camera, carry anchor and Animator references present.");
                var model=motor.visual.gameObject;
                var renderers=model.GetComponentsInChildren<SkinnedMeshRenderer>();
                Add(report,"materials_and_skin",renderers.Length==19 && renderers.All(r=>r.bones.Length==53 && r.sharedMaterials.All(m=>m && m.shader.name=="Universal Render Pipeline/Lit")),renderers.Length+" skinned modules; 53 bone references each; no missing materials.");
                foreach(string clipName in new[]{"Walk","Run"})
                {
                    var clip=AssetDatabase.LoadAllAssetsAtPath("Assets/Prototype/Art/Sunny/Sunny_"+clipName+"_v004.fbx").OfType<AnimationClip>().First(c=>c.name==clipName);
                    var bindings=AnimationUtility.GetCurveBindings(clip);
                    bool paths=bindings.All(b=>string.IsNullOrEmpty(b.path) || model.transform.Find(b.path));
                    var foot=model.GetComponentsInChildren<Transform>().First(t=>t.name.Contains("LeftFoot")&&!t.name.EndsWith("_end"));
                    motor.animator.enabled=false;
                    clip.SampleAnimation(model,0); Vector3 a=foot.localPosition; Quaternion q=foot.localRotation;
                    clip.SampleAnimation(model,clip.length*0.25f);
                    bool moving=(foot.localPosition-a).sqrMagnitude>0.000001f || Quaternion.Angle(q,foot.localRotation)>0.1f;
                    Add(report,clipName.ToLower()+"_animation",paths && moving && bindings.Count(b=>b.propertyName.Contains("RearCuffClearance"))==2,"Duration="+clip.length+"; all curve paths resolve; moving foot; two baked cuff curves.");
                }
                motor.animator.enabled=true; motor.animator.Rebind(); motor.animator.Update(0);
                var keyboard=InputSystem.AddDevice<Keyboard>("Phase3ValidationKeyboard");
                try
                {
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.LeftShift,Key.Space,Key.E)); InputSystem.Update();
                    Add(report,"synthetic_input_bindings",input.Move.ReadValue<Vector2>().y>0.9f && input.Sprint.IsPressed() && input.Jump.IsPressed() && input.Interact.IsPressed(),"Virtual keyboard W/Shift/Space/E read through enabled production InputActions; no human input tested.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState()); InputSystem.Update();
                }
                finally { InputSystem.RemoveDevice(keyboard); }
                const float dt=1f/60;
                Action<int,Vector2,bool,bool> step=(count,move,sprint,jump)=> { for(int i=0;i<count;i++) motor.Step(move,sprint,jump&&i==0,dt); };
                camera.yaw=0; camera.pitch=20;
                motor.Teleport(new Vector3(0,0.2f,-6)); camera.PositionCamera(); step(60,Vector2.zero,false,false);
                Add(report,"gravity_grounding",motor.Grounded && Mathf.Abs(motor.transform.position.y)<0.12f,"Settled position="+motor.transform.position+" grounded="+motor.Grounded);
                Vector3 start=motor.transform.position; step(60,Vector2.up,false,false); float walk=Vector3.Distance(start,motor.transform.position);
                motor.Teleport(new Vector3(0,0.1f,-6)); step(30,Vector2.zero,false,false); camera.PositionCamera(); start=motor.transform.position; step(60,Vector2.up,true,false); float sprint=Vector3.Distance(start,motor.transform.position);
                Add(report,"walk_sprint",walk>1.8f && sprint>walk*1.5f,"One second walk="+walk+"m; sprint="+sprint+"m.");
                motor.Teleport(new Vector3(0,0.1f,-6)); step(30,Vector2.zero,false,false); camera.yaw=90; camera.PositionCamera(); start=motor.transform.position; step(60,Vector2.up,false,false);
                Vector3 relative=motor.transform.position-start;
                Add(report,"camera_relative_rotation",relative.x>1.8f && Mathf.Abs(relative.z)<0.1f && Vector3.Dot(motor.visual.forward,Vector3.right)>0.99f,"Yaw 90 displacement="+relative+"; visual faces movement.");
                motor.Teleport(new Vector3(0,0.1f,-6)); step(30,Vector2.zero,false,false); float floor=motor.transform.position.y,peak=floor;
                for(int i=0;i<120;i++) { motor.Step(Vector2.zero,false,i==0,dt); peak=Mathf.Max(peak,motor.transform.position.y); }
                Add(report,"jump_land",peak-floor>0.5f && motor.Grounded,"Jump rise="+(peak-floor)+"m; landed="+motor.Grounded);
                motor.Teleport(new Vector3(-13.5f,0.1f,6)); camera.yaw=90; camera.PositionCamera(); step(150,Vector2.up,true,false);
                Add(report,"wall_collision",motor.transform.position.x<-11.7f && motor.transform.position.x>-12.2f,"Controller blocked by shop west wall at "+motor.transform.position);
                motor.Teleport(new Vector3(-2,0.1f,0.7f)); motor.visual.rotation=Quaternion.identity; step(20,Vector2.zero,false,false); Physics.SyncTransforms(); player.FindNearby();
                Add(report,"nearby_prompt",player.Current==box && player.Current.Prompt.Contains("Pick up"),"Nearest visible box selected for on-screen prompt.");
                bool picked=player.TryInteract(); bool held=picked && box.IsHeld && player.HeldBox==box && box.transform.parent==player.carryAnchor;
                bool dropped=player.TryInteract();
                Add(report,"box_pickup_drop",held && dropped && !box.IsHeld && player.HeldBox==null && box.GetComponent<Collider>().enabled && !box.GetComponent<Rigidbody>().isKinematic,"Production interaction path picked up, attached, placed on ground and restored physics.");
                // Place the box for line-of-sight and reach checks using temporary Play-mode state only.
                box.GetComponent<Rigidbody>().isKinematic=true; box.transform.position=new Vector3(-2,0.3f,1.7f);
                var occluder=GameObject.CreatePrimitive(PrimitiveType.Cube); occluder.name="Validation temporary occluder"; occluder.transform.position=new Vector3(-2,0.6f,1.15f); occluder.transform.localScale=new Vector3(1,1.2f,0.12f);
                Physics.SyncTransforms(); player.FindNearby();
                Add(report,"interaction_occlusion",player.Current!=box,"Temporary wall blocks box detection.");
                UnityEngine.Object.DestroyImmediate(occluder);
                motor.Teleport(new Vector3(0,0.1f,-6)); player.FindNearby(); Add(report,"interaction_range",player.Current==null,"No prompt beyond interaction reach.");
                motor.Teleport(new Vector3(-7.3f,0.1f,2.35f)); step(20,Vector2.zero,false,false); Physics.SyncTransforms(); player.FindNearby();
                bool foundDoor=player.Current==door; bool opened=player.TryInteract(); door.Advance(1); bool openState=door.IsOpen; float openAngle=Quaternion.Angle(door.hinge.localRotation,Quaternion.identity);
                bool closed=door.Interact(player); door.Advance(1);
                Add(report,"door_open_close",foundDoor && opened && openState && openAngle>90 && closed && !door.IsOpen && Quaternion.Angle(door.hinge.localRotation,Quaternion.identity)<0.1f,"Detected door; rotated "+openAngle+" degrees; closed to original rotation.");
                motor.Teleport(new Vector3(8,0.1f,6.25f)); step(20,Vector2.zero,false,false); Physics.SyncTransforms(); player.FindNearby(); int before=workstation.UseCount;
                bool used=player.Current==workstation && player.TryInteract();
                Add(report,"workstation",used && workstation.UseCount==before+1 && player.Status.Contains("checked"),"Nearest workstation increments use count and sends HUD feedback.");
                camera.yaw=0; camera.pitch=20; motor.Teleport(new Vector3(-10,0.1f,5.3f)); camera.PositionCamera(); float cameraDistance=Vector3.Distance(camera.transform.position,motor.transform.position+Vector3.up*camera.targetHeight);
                Add(report,"camera_obstruction",cameraDistance<camera.distance-0.5f && cameraDistance>0.25f,"Shop front wall shortens camera boom to "+cameraDistance+"m.");
                camera.Orbit(new Vector2(300,10000)); Add(report,"mouse_orbit_clamp",Mathf.Abs(camera.yaw-36)<0.01f && Mathf.Abs(camera.pitch-camera.minimumPitch)<0.01f,"Synthetic mouse delta changed yaw and clamped pitch.");
                motor.Teleport(motor.respawnPosition); motor.visual.rotation=Quaternion.identity; camera.yaw=0; camera.pitch=20; camera.PositionCamera(); motor.animator.Rebind(); motor.animator.Update(0);
                Capture(camera.GetComponent<Camera>(),Path.Combine(Diagnostics,"player-camera.png"));
                var overviewObject=new GameObject("Validation overview camera"); var overview=overviewObject.AddComponent<Camera>(); overview.backgroundColor=camera.GetComponent<Camera>().backgroundColor; overview.clearFlags=CameraClearFlags.SolidColor; overview.fieldOfView=50; overview.transform.position=new Vector3(21,25,-24); overview.transform.LookAt(new Vector3(0,0,2));
                Capture(overview,Path.Combine(Diagnostics,"town-overview.png")); UnityEngine.Object.DestroyImmediate(overviewObject);
            }
            catch(Exception e) { Add(report,"unexpected_exception",false,e.ToString()); Debug.LogException(e); }
            finally
            {
                File.WriteAllText(Path.Combine(Diagnostics,"playmode-validation.json"),JsonUtility.ToJson(report,true));
                Debug.Log("Phase 3 validation: "+report.checks.Count(c=>c.passed)+"/"+report.checks.Count+" checks passed. Report: "+Diagnostics);
                EditorApplication.isPlaying=false;
            }
        }
        static void Add(Report report,string name,bool passed,string details) { report.checks.Add(new Check{name=name,passed=passed,details=details}); }
        [MenuItem("Business as Usual/Phase 3/Build Windows Prototype")]
        public static void BuildWindows()
        {
            if(EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Stop Play and wait for compilation before building.");
            Directory.CreateDirectory(Diagnostics);
            string folder=Path.GetFullPath("Builds/Phase3"); Directory.CreateDirectory(folder);
            var report=BuildPipeline.BuildPlayer(new[]{PrototypeBuilder.ScenePath},Path.Combine(folder,"BusinessAsUsual.exe"),BuildTarget.StandaloneWindows64,BuildOptions.Development);
            var summary=report.summary;
            File.WriteAllText(Path.Combine(Diagnostics,"windows-build.json"),"{\n  \"result\": \""+summary.result+"\",\n  \"errors\": "+summary.totalErrors+",\n  \"warnings\": "+summary.totalWarnings+",\n  \"bytes\": "+summary.totalSize+",\n  \"durationSeconds\": "+summary.totalTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\n  \"output\": \""+summary.outputPath.Replace("\\","/")+"\"\n}");
            if(summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Windows prototype build failed; inspect build report.");
            Debug.Log("Phase 3 Windows build succeeded: "+summary.outputPath);
        }
        public static void Capture(Camera camera,string path)
        {
            var previousTarget=camera.targetTexture; var previousActive=RenderTexture.active;
            var rt=RenderTexture.GetTemporary(1280,720,24); var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try { camera.targetTexture=rt; camera.Render(); RenderTexture.active=rt; texture.ReadPixels(new Rect(0,0,1280,720),0,0); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); }
            finally { camera.targetTexture=previousTarget; RenderTexture.active=previousActive; RenderTexture.ReleaseTemporary(rt); UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
