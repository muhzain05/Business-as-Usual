using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace BusinessAsUsual.Multiplayer
{
    // Opt-in standalone test harness. Inert during ordinary play.
    public sealed class NetworkTestDriver : MonoBehaviour
    {
        public static bool Automated { get; private set; }
        // Opt-in: the harness stops synthesizing movement so the production keyboard path drives the player.
        public static bool RealInput { get; private set; }
        // Available before any scene Awake runs, so fixture visibility is deterministic.
        public static bool Requested
        {
            get {
                var args=Environment.GetCommandLineArgs();
                return new[]{"--bau-role","--bau-evidence","--bau-control"}.All(flag=> {
                    int index=Array.IndexOf(args,flag);
                    return index>=0&&index+1<args.Length&&!string.IsNullOrEmpty(args[index+1]);
                });
            }
        }
        static NetworkTestDriver instance;
        string role,output,controlPath;
        int serial=-1;
        float nextReport,nextMove,moveUntil;
        Control movement;
        [Serializable] public class Control { public int serial; public string action,target,order; public float x,y,z,yaw,duration; public bool run,jump; public uint sequence; public int revision; }
        [Serializable] public class PlayerRecord { public ulong id,owner; public bool local,spawned,grounded,running,jumping,bodyVisible; public Vector3 position,leftFoot; public float speed,lookYaw,lookPitch,facing; public int inputs,footPlants; }
        static Vector3 Foot(NetworkPlayer player)
        {
            var visual=player.Motor.visual;var foot=visual.Find("SunnyRig/mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot");
            return foot?visual.InverseTransformPoint(foot.position):Vector3.zero;
        }
        [Serializable] public class Report { public BusinessAsUsual.Phase8B.PrivateDayCopy dayPrivate;public float music,effects,master;public BusinessAsUsual.RescueV2.RescuePerformance.Evidence performance; public string role,status,feedback,currentTarget; public bool listening,connected,host,localCarried; public ulong localId; public int serial,snapshots,bytes,accepted,rejected; public PlayerRecord[] players; public NetworkShop.Snapshot state; public string[] targetIds; public Vector3[] targetPoints; public int cameras,listeners,inputs,huds,townHuds; public float frameMilliseconds; public GuidanceRecord guidance; public int feedbackCount; public FirstPersonRecord firstPerson; public BusinessAsUsual.Town.WorkRecord[] workRecords; public int serverAuditCount; public int[] workSupport; public BusinessAsUsual.RescueV2.RescueUIEvidence rescueUI; }
        [Serializable] public class FirstPersonRecord { public bool active,hiddenBody,handsVisible,paused,gameplayActive,inputBlocked,mapBaked,mapExpanded; public float yaw,pitch,cameraHeight,fov; public string handPose,pausePage; public int gestures,viewmodels,visibleColleagues,mapObjectives,mapClamped,remoteHeadsTurned,pauseOpened,sfx,vSyncCount,targetFrameRate,qualityLevel; public float listenerVolume,sensitivity; public int screenWidth,screenHeight; public bool windowed; public Vector2 mapPlayer; public Vector3 cameraPosition; public string remoteHandPose; public int internalWidth,internalHeight,renderSamples;public float renderScale,cadence,townPhase;public double townClock;public bool dynamicResolution,serverClock;public string[] mapMarkerIds;public Vector3[] mapMarkerWorld; }
        [Serializable] public class ObjectiveRecord { public string id,title,icon; public bool active; public Vector3 position; }
        [Serializable] public class GuidanceRecord { public string job,note,detail; public ObjectiveRecord[] objectives; public Vector3 mapPlayer; public int markerCount; public float paperFraction; public bool bookOpen,staffDesk,cursorLocked; }
        static GuidanceRecord Guidance(NetworkPlayer player) {
            if(!player)return null;
            var book=player.GetComponent<BusinessAsUsual.Town.TownHandbook>();var guide=book?book.Guide:null;if(guide==null)return null;
            return new GuidanceRecord {bookOpen=book.Expanded,staffDesk=book.Choosing,cursorLocked=Cursor.lockState==CursorLockMode.Locked,job=guide.job.ToString(),note=guide.note,detail=guide.detail,mapPlayer=BusinessAsUsual.Town.TownGuidance.MapPosition(player.transform.position),markerCount=book.MarkerCount,paperFraction=book.HudPaperFraction,objectives=guide.objectives.Select(o=>new ObjectiveRecord{id=o.id,title=o.title,icon=o.icon,active=o.active,position=o.Position}).ToArray()};
        }
        static FirstPersonRecord FirstPerson(NetworkPlayer player) {
            if(!player)return null;
            var view=player.GetComponent<BusinessAsUsual.FirstPerson.FirstPersonView>();if(!view)return null;
            var hands=player.GetComponent<BusinessAsUsual.FirstPerson.FirstPersonHands>();var menu=player.GetComponent<BusinessAsUsual.FirstPerson.PauseMenu>();var book=player.GetComponent<BusinessAsUsual.Town.TownHandbook>();var input=player.GetComponent<Prototype.PrototypeInput>();
            var authored=player.GetComponent<BusinessAsUsual.RescueV2.RescueHands>();
            var atmosphere=FindFirstObjectByType<BusinessAsUsual.Town.TownAtmosphere>();
            var markers=book&&book.Minimap!=null?BusinessAsUsual.FirstPerson.TownMinimap.ActiveMarkers(book.Minimap.RegionFor(player.transform.position),book.Guide):new System.Collections.Generic.List<BusinessAsUsual.FirstPerson.TownMinimap.MapObjective>();
            return new FirstPersonRecord{active=view.Active,hiddenBody=view.HiddenBody,yaw=view.Yaw,pitch=view.Pitch,fov=view.ViewCamera?view.ViewCamera.fieldOfView:0,cameraPosition=view.ViewCamera?view.ViewCamera.transform.position:Vector3.zero,cameraHeight=view.ViewCamera?view.ViewCamera.transform.position.y-player.transform.position.y:0,
                internalWidth=BusinessAsUsual.Town.NativeRenderProbe.Width,internalHeight=BusinessAsUsual.Town.NativeRenderProbe.Height,renderScale=BusinessAsUsual.Town.NativeRenderProbe.Scale,renderSamples=BusinessAsUsual.Town.NativeRenderProbe.Samples,dynamicResolution=view.ViewCamera&&view.ViewCamera.allowDynamicResolution,cadence=player.GetComponent<BusinessAsUsual.FirstPerson.AvatarPresentation>()?.Cadence??0,townClock=atmosphere?atmosphere.ClockSeconds:0,townPhase=atmosphere?atmosphere.CurrentPhase:0,serverClock=atmosphere&&atmosphere.UsingServerClock,mapMarkerIds=markers.Select(m=>m.objective.id).ToArray(),mapMarkerWorld=markers.Select(m=>m.world).ToArray(),
                handsVisible=authored?authored.Visible:hands&&hands.Visible,handPose=authored?authored.Pose:hands?hands.Pose:"",gestures=authored?authored.Gestures:hands?hands.GesturesPlayed:0,viewmodels=FindObjectsByType<BusinessAsUsual.FirstPerson.FirstPersonHands>(FindObjectsSortMode.None).Count(h=>h.Visible)+FindObjectsByType<BusinessAsUsual.RescueV2.RescueHands>(FindObjectsSortMode.None).Count(h=>h.Visible),
                paused=BusinessAsUsual.FirstPerson.PauseMenu.IsOpen,pausePage=menu?menu.CurrentPage.ToString():"",pauseOpened=menu?menu.Opened:0,gameplayActive=input&&input.GameplayActive,inputBlocked=input&&input.Blocked,
                mapBaked=book&&book.Minimap!=null&&book.Minimap.Baked,mapExpanded=book&&book.MapExpanded,visibleColleagues=book&&book.Minimap!=null?book.Minimap.VisibleColleagues:0,mapObjectives=book&&book.Minimap!=null?book.Minimap.DrawnObjectives:0,mapClamped=book&&book.Minimap!=null?book.Minimap.ClampedObjectives:0,mapPlayer=book&&book.Minimap!=null?book.Minimap.LastPlayerPoint:Vector2.zero,
                sfx=BusinessAsUsual.FirstPerson.TownAudio.Played,vSyncCount=QualitySettings.vSyncCount,targetFrameRate=Application.targetFrameRate,qualityLevel=QualitySettings.GetQualityLevel(),listenerVolume=AudioListener.volume,screenWidth=Screen.width,screenHeight=Screen.height,windowed=Screen.fullScreenMode==FullScreenMode.Windowed,sensitivity=BusinessAsUsual.FirstPerson.GameSettings.Sensitivity,
                remoteHeadsTurned=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Count(p=>!p.IsOwner&&p.IsSpawned&&Mathf.Abs(Mathf.DeltaAngle(p.Motor.visual.eulerAngles.y,p.LookYaw.Value))>1f)};
        }
        void Awake()
        {
            if(!Requested)return;
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&instance&&instance!=this){Destroy(this);return;}instance=this;if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)DontDestroyOnLoad(gameObject);
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++) { if(args[i]=="--bau-role")role=args[i+1]; if(args[i]=="--bau-evidence")output=args[i+1]; if(args[i]=="--bau-control")controlPath=args[i+1]; }
            Automated=!string.IsNullOrEmpty(role)&&!string.IsNullOrEmpty(output)&&!string.IsNullOrEmpty(controlPath);
            if(Automated&&BusinessAsUsual.RescueV2.RescueInterface.Requested)gameObject.AddComponent<BusinessAsUsual.RescueV2.RescuePerformance>();
        }
        IEnumerator Start() { if(!Automated||System.Environment.GetCommandLineArgs().Contains("--bau-ui-only"))yield break; yield return new WaitForSeconds(1);if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)BusinessAsUsual.Phase8B.Phase8BFlow.Launch(role=="host"?"host":"client");else NetworkSession.Instance.StartSession(role=="host"); }
        void Update()
        {
            if(!Automated)return;
            var session=NetworkSession.Instance; var manager=session?session.Manager:null;
            if(!manager){FrontendEvidence();return;}
            var player=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.IsOwner);
            try {
                if(File.Exists(controlPath)) {
                    var c=JsonUtility.FromJson<Control>(File.ReadAllText(controlPath));
                    if(c.serial!=serial) {
                        serial=c.serial;
                        switch(c.action) {
                            case "interact": if(player)player.SendCommand(c.target); break;
                            case "e": if(player)player.Interactor.TryInteract();break;
                            case "hold": if(player)player.TestHold(c.run);break;
                            case "raw": if(player)player.TestCommand(c.target,c.sequence,c.revision,c.order??""); break;
                            case "teleport": if(player){player.TestTeleportServerRpc(new Vector3(c.x,c.y,c.z));var fp=player.GetComponent<BusinessAsUsual.FirstPerson.FirstPersonView>();if(fp&&fp.Active)fp.SetView(c.yaw,0);} break; // Placement faces north (+Z) toward the target, as the harness places 1.1 m south of it.
                            case "move": movement=c; moveUntil=Time.unscaledTime+c.duration; break;
                            case "disconnect": manager.Shutdown(); break;
                            case "reconnect": if(!manager.IsListening){session.Status="Reconnecting test client";manager.StartClient();}break;
                            case "quit": Application.Quit();break;
                            case "capture": StartCoroutine(Capture(c.target)); break;
                            case "rescuecapture": StartCoroutine(BusinessAsUsual.RescueV2.RescueCapture.Offscreen((int)c.x,(int)c.y,Path.Combine(Path.GetDirectoryName(output),role+"-"+Path.GetFileName(c.target)+".png")));break;
                            case "preview": StartCoroutine(CaptureSequence(c.target));break;
                            case "turn": if(player)StartCoroutine(TurnTerminal(player));break;
                            case "panel": if(player){var hud=player.GetComponent<BusinessAsUsual.Town.TownHandbook>();if(hud){hud.Expanded=c.run;hud.CurrentPage=(BusinessAsUsual.Town.TownHandbook.Page)c.x;}var shopHud=player.GetComponent<Shopkeeper.ShopHUD>();if(shopHud.enabled)shopHud.ShowPanel(c.run?(c.x>=3?Shopkeeper.ShopHUD.Panel.Report:Shopkeeper.ShopHUD.Panel.Tasks):Shopkeeper.ShopHUD.Panel.None);}break;
                            case "rescueui": if(BusinessAsUsual.RescueV2.RescueInterface.Instance)BusinessAsUsual.RescueV2.RescueInterface.Instance.Preview(c.target,(int)c.x,(int)c.y);break;
                            case "solo": if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)BusinessAsUsual.Phase8B.Phase8BFlow.Launch("solo");else session.StartSolo();break;
                            case "click": StartCoroutine(ClickUI(c.x,c.y));break;
                            case "key": StartCoroutine(PressKey(c.target,c.duration));break;
                            case "realinput": RealInput=c.run;break;
                            case "capturecursor": if(player)player.GetComponent<Prototype.PrototypeInput>().SetCursor(true);break;
                            case "pausepage": if(player){var menu=player.GetComponent<BusinessAsUsual.FirstPerson.PauseMenu>();if(menu)menu.ShowPage((BusinessAsUsual.FirstPerson.PauseMenu.Page)(int)c.x);}break;
                            case "leave": if(player){var menu=player.GetComponent<BusinessAsUsual.FirstPerson.PauseMenu>();if(menu){menu.SetOpen(true);menu.Leave(false);}}break;
                            case "camera": if(player){var view=player.GetComponent<BusinessAsUsual.FirstPerson.FirstPersonView>();if(view&&view.Active)view.SetView(c.x,c.order=="pitch"?c.y:0);else{var camera=player.GetComponentInChildren<Prototype.ThirdPersonCamera>(true);camera.yaw=c.x;camera.pitch=c.y;if(c.z>0)camera.distance=c.z;}}break;
                            case "look": if(player){var view=player.GetComponent<BusinessAsUsual.FirstPerson.FirstPersonView>();if(view)view.SetView(c.x,c.y);}break;
                            case "pause": if(player){var menu=player.GetComponent<BusinessAsUsual.FirstPerson.PauseMenu>();if(menu)menu.SetOpen(c.run);}break;
                            case "settings": BusinessAsUsual.FirstPerson.GameSettings.SetMaster(c.x);BusinessAsUsual.FirstPerson.GameSettings.SetSensitivity(c.y);BusinessAsUsual.FirstPerson.GameSettings.SetFrameCap((int)c.z);BusinessAsUsual.FirstPerson.GameSettings.SetVSync(c.run);break;
                            case "display": BusinessAsUsual.FirstPerson.GameSettings.SetDisplay(c.run?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow,new Vector2Int((int)c.x,(int)c.y));break;
                            case "reloadsettings": BusinessAsUsual.FirstPerson.GameSettings.Load();break;
                            case "fov": BusinessAsUsual.FirstPerson.GameSettings.SetFieldOfView(c.x);break;
                            case "quality": BusinessAsUsual.FirstPerson.GameSettings.SetQuality((int)c.x);break;
                            case "map": if(player){var book=player.GetComponent<BusinessAsUsual.Town.TownHandbook>();if(book)book.MapExpanded=c.run;}break;
                        }
                    }
                }
                if(player&&!RealInput&&Time.unscaledTime>=nextMove) { nextMove=Time.unscaledTime+1f/30; bool active=movement!=null&&Time.unscaledTime<moveUntil; player.TestMove(active?new Vector2(movement.x,movement.y):Vector2.zero,active&&movement.run,active&&movement.jump,active?movement.yaw:0); if(movement!=null)movement.jump=false; }
                if(Time.unscaledTime>=nextReport) {
                    nextReport=Time.unscaledTime+.15f; var shop=NetworkShop.Instance;
                    var report=new Report {dayPrivate=shop.Day?shop.Day.Local:null,music=BusinessAsUsual.FirstPerson.GameSettings.Music,effects=BusinessAsUsual.FirstPerson.GameSettings.Effects,master=BusinessAsUsual.FirstPerson.GameSettings.Master,performance=BusinessAsUsual.RescueV2.RescuePerformance.Instance?BusinessAsUsual.RescueV2.RescuePerformance.Instance.Capture():null,rescueUI=BusinessAsUsual.RescueV2.RescueUIEvidence.Capture(),role=role,status=session.Status,feedback=player?player.LastFeedback:"",listening=manager.IsListening,connected=manager.IsConnectedClient,host=manager.IsHost,localCarried=manager.IsConnectedClient&&shop.Holder==manager.LocalClientId,localId=manager.LocalClientId,serial=serial,
                        workSupport=shop.Workday?(manager.IsServer?shop.Workday.SupportFor(manager.LocalClientId):shop.Workday.LocalSupport):null,
                        workRecords=shop.Workday?(manager.IsServer?shop.Workday.RecordsFor(manager.LocalClientId):shop.Workday.LocalRecords):null,serverAuditCount=shop.Workday&&manager.IsServer?shop.Workday.Domain.Audit.Count:0,
                        guidance=Guidance(player),firstPerson=FirstPerson(player),feedbackCount=player?player.FeedbackReceived:0,currentTarget=player&&player.Interactor.Current?NetworkShop.Id(player.Interactor.Current):"",frameMilliseconds=Time.unscaledDeltaTime*1000,townHuds=FindObjectsByType<BusinessAsUsual.Town.TownHandbook>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled),snapshots=shop.SnapshotsSent,bytes=shop.SnapshotBytes,accepted=shop.CommandsAccepted,rejected=shop.CommandsRejected,state=shop.Capture(),
                        players=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Select(p=>new PlayerRecord {id=p.NetworkObjectId,owner=p.OwnerClientId,local=p.IsOwner,spawned=p.IsSpawned,position=p.transform.position,leftFoot=Foot(p),speed=p.Speed.Value,grounded=p.Grounded.Value,running=p.Running.Value,jumping=p.Jumping.Value,inputs=p.InputsAccepted,footPlants=p.GetComponent<BusinessAsUsual.RescueV2.RescueFootPlant>()?.Plants??0,lookYaw=p.LookYaw.Value,lookPitch=p.LookPitch.Value,facing=p.Motor.visual.eulerAngles.y,bodyVisible=p.Motor.visual.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)}).ToArray(),
                        targetIds=shop.Targets.Keys.ToArray(),targetPoints=shop.Targets.Values.Select(t=>t.InteractionPoint).ToArray(),
                        cameras=FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled),listeners=FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled),inputs=FindObjectsByType<Prototype.PrototypeInput>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled),huds=FindObjectsByType<Shopkeeper.ShopHUD>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled) };
                    File.WriteAllText(output+".tmp",JsonUtility.ToJson(report,true));
                    if(File.Exists(output))File.Replace(output+".tmp",output,null);else File.Move(output+".tmp",output);
                }
            } catch(IOException) { /* Controller may be atomically replacing the command file. */ }
        }
        void FrontendEvidence(){
            try{if(File.Exists(controlPath)){var c=JsonUtility.FromJson<Control>(File.ReadAllText(controlPath));if(c.serial!=serial){serial=c.serial;switch(c.action){case "click":StartCoroutine(ClickUI(c.x,c.y));break;case "key":StartCoroutine(PressKey(c.target,c.duration));break;case "capture":StartCoroutine(Capture(c.target));break;case "quit":Application.Quit();break;case "rescueui":BusinessAsUsual.RescueV2.RescueInterface.Instance.Preview(c.target);break;case "solo":BusinessAsUsual.Phase8B.Phase8BFlow.Launch("solo");break;case "settings":BusinessAsUsual.FirstPerson.GameSettings.SetMaster(c.x);BusinessAsUsual.FirstPerson.GameSettings.SetMusic(c.y);BusinessAsUsual.FirstPerson.GameSettings.SetEffects(c.z);break;}}}
                if(Time.unscaledTime<nextReport)return;nextReport=Time.unscaledTime+.15f;var r=new Report{role=role,serial=serial,music=BusinessAsUsual.FirstPerson.GameSettings.Music,effects=BusinessAsUsual.FirstPerson.GameSettings.Effects,master=BusinessAsUsual.FirstPerson.GameSettings.Master,rescueUI=BusinessAsUsual.RescueV2.RescueUIEvidence.Capture(),status="Main menu",players=Array.Empty<PlayerRecord>()};File.WriteAllText(output+".tmp",JsonUtility.ToJson(r,true));if(File.Exists(output))File.Replace(output+".tmp",output,null);else File.Move(output+".tmp",output);
            }catch(IOException){}
        }
        IEnumerator ClickUI(float x,float y)
        {
            var settings=UnityEngine.InputSystem.InputSystem.settings;var previous=settings.backgroundBehavior;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            var mouse=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("Workday UI validation mouse");
            var state=new UnityEngine.InputSystem.LowLevel.MouseState{position=new Vector2(x,y)};UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,state);yield return null;
            state=state.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,true);UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,state);yield return null;yield return null;
            state=state.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,false);UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,state);yield return null;
            UnityEngine.InputSystem.InputSystem.RemoveDevice(mouse);settings.backgroundBehavior=previous;
        }
        IEnumerator PressKey(string key,float duration=0)
        {
            // Opt-in controller only: real InputAction bindings with a virtual keyboard in this process.
            var settings=UnityEngine.InputSystem.InputSystem.settings;var previous=settings.backgroundBehavior;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("Town validation keyboard");
            if(!Enum.TryParse<UnityEngine.InputSystem.Key>(key,true,out var value))value=UnityEngine.InputSystem.Key.Tab;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(value));yield return null;
            // Held keys (W) stay down for the requested duration, as a person holding the key would.
            for(float until=Time.unscaledTime+duration;Time.unscaledTime<until;)yield return null;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return null;
            UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=previous;
        }
        IEnumerator Capture(string suffix) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(string.IsNullOrEmpty(suffix)?Path.ChangeExtension(output,"png"):Path.Combine(Path.GetDirectoryName(output),role+"-"+Path.GetFileName(suffix)+".png")); }
        IEnumerator CaptureSequence(string name)
        {
            for(int i=0;i<16;i++){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(output),role+"-preview-"+Path.GetFileName(name)+"-"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.10f);}
        }
        IEnumerator TurnTerminal(NetworkPlayer player)
        {
            bool previous=RealInput;RealInput=true;player.GetComponent<Prototype.PrototypeInput>().SetCursor(true);
            var settings=UnityEngine.InputSystem.InputSystem.settings;var focus=settings.backgroundBehavior;settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            var mouse=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("Workday validation mouse");
            try {
                player.Interactor.TryInteract();yield return null;
                for(int i=0;i<10;i++) {
                    var state=new UnityEngine.InputSystem.LowLevel.MouseState {delta=new Vector2(20,0)};
                    state=state.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left);
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,state);yield return new WaitForSecondsRealtime(.08f);
                    if(!BusinessAsUsual.Town.LivingWorkday.Instance.Turning)break;
                }
            } finally {UnityEngine.InputSystem.InputSystem.RemoveDevice(mouse);settings.backgroundBehavior=focus;RealInput=previous;}
        }
        void OnDestroy() { if(instance!=this)return;instance=null;Automated=false;RealInput=false; }
    }
}
