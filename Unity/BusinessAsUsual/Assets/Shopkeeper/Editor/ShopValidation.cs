using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BusinessAsUsual.Prototype;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    [InitializeOnLoad] public static class ShopValidation
    {
        [Serializable] public class Check { public string name,details; public bool passed; }
        [Serializable] public class Report { public string timestampUtc,unityVersion,scene,method; public List<Check> checks=new List<Check>();public ShopShift.ShiftSummary summary; }
        static readonly string Output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Docs/Diagnostics/Phase4"));
        static double began;
        static ShopValidation(){ EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BAU.Phase4.Test",false)){began=EditorApplication.timeSinceStartup;EditorApplication.update+=Wait;}}; }
        [MenuItem("Business as Usual/Phase 4/Validate Complete Shift")]
        public static void Begin()
        {
            if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Stop Play and save scene first.");
            EditorSceneManager.OpenScene(ShopBuilder.ScenePath);SessionState.SetBool("BAU.Phase4.Test",true);EditorApplication.isPlaying=true;
        }
        static void Wait(){if(EditorApplication.timeSinceStartup-began<2)return;EditorApplication.update-=Wait;SessionState.SetBool("BAU.Phase4.Test",false);Run();}
        public static void Run()
        {
            Directory.CreateDirectory(Output);var r=new Report{timestampUtc=DateTime.UtcNow.ToString("o"),unityVersion=Application.unityVersion,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,method="Live Unity Play mode; production PlayerInteractor and motor calls, deterministic synthetic traversal; no human session."};
            Action<string,bool,string> check=(n,p,d)=>r.checks.Add(new Check{name=n,passed=p,details=d});
            try {
                if(!Application.isPlaying) throw new InvalidOperationException("Play mode required");
                var shift=UnityEngine.Object.FindFirstObjectByType<ShopShift>();var motor=UnityEngine.Object.FindFirstObjectByType<ThirdPersonMotor>();var player=motor.GetComponent<PlayerInteractor>();var camera=motor.GetComponentInChildren<ThirdPersonCamera>();
                motor.enabled=false;player.enabled=false;camera.enabled=false;
                var stations=UnityEngine.Object.FindObjectsByType<ShopStation>(FindObjectsSortMode.None);
                Func<ShopStation.Kind,ShopStation> station=k=>stations.First(s=>s.kind==k);
                Action<Interactable,Vector3> approach=(s,offset)=>{motor.Teleport(new Vector3(s.InteractionPoint.x+offset.x,.05f,s.InteractionPoint.z+offset.z));Physics.SyncTransforms();player.FindNearby();check("accessible:"+s.name,player.Current==s,"Detected="+(player.Current?player.Current.name:"none")+" position="+motor.transform.position);};
                Func<ShopStation,bool> use=s=>{var reach=player.GetComponent<ShopRestockReach>();if(reach)reach.Advance(1f);approach(s,new Vector3(0,0,-1.1f));return player.TryInteract();};
                check("initial_inventory",ShopShift.Products.All(p=>shift.Stock.Total(p.sku)==6),"6 units per SKU; 18 total.");
                check("early_close_rejected",!shift.Close(player),"Cannot skip shift tasks.");
                check("early_checkout_atomic",!shift.Checkout(player)&&shift.Transactions.Receipts.Count==0,"No receipt or sale on incomplete basket.");
                check("receive",use(station(ShopStation.Kind.Delivery)),"Delivery station used through production interaction.");
                check("duplicate_receive_rejected",!shift.Receive(player),"Delivery cannot be counted twice.");
                var crate=UnityEngine.Object.FindFirstObjectByType<StockCrate>();approach(crate,new Vector3(0,0,-.8f));check("carry",player.TryInteract()&&shift.Carried==crate,"Crate attached to unchanged Sunny carry anchor.");
                var keyboard=InputSystem.AddDevice<Keyboard>("Phase4ValidationKeyboard");
                try{InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Q));InputSystem.Update();shift.SendMessage("Update");check("Q_return_action",!shift.Carried,"Synthetic Q read through production return-crate action.");InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();}
                finally{InputSystem.RemoveDevice(keyboard);}
                check("return_conservation",ShopShift.Products.All(p=>shift.Stock.Count("dock",p.sku)==6&&shift.Stock.Count("carry",p.sku)==0),"Return restores dock stock.");
                approach(crate,new Vector3(0,0,-.8f));check("carry_again",player.TryInteract(),"Returned crate can be collected again.");
                foreach(var shelf in stations.Where(s=>s.kind==ShopStation.Kind.Shelf)) check("stock:"+shelf.sku,use(shelf)&&shift.Stock.Count("shelf",shelf.sku)==6,"Stocked through E interaction.");
                check("crate_auto_return",!shift.Carried,"Empty crate returns automatically.");
                player.GetComponent<ShopRestockReach>()?.Advance(1f);
                foreach(var display in UnityEngine.Object.FindObjectsByType<ShopStockDisplay>(FindObjectsSortMode.None)){display.SendMessage("LateUpdate");check("stock_visual:"+display.sku,display.units.Count(o=>o.activeSelf)==6,"Counted stock row displays six units after stocking.");}
                check("wrong_sku_rejected",!shift.UseShelf("tea",player)&&shift.Stock.Count("basket","tea")==0,"First order does not request tea.");
                for(int i=0;i<3;i++) {
                    var order=shift.CurrentOrder;
                    foreach(var line in order.lines) {var shelf=stations.First(s=>s.kind==ShopStation.Kind.Shelf&&s.sku==line.sku);for(int n=0;n<line.quantity;n++)check("pick:"+order.id+":"+line.sku+":"+n,use(shelf),"Inventory transferred shelf to basket.");check("overpick_rejected:"+order.id+line.sku,!shift.UseShelf(line.sku,player),"Requested quantity enforced.");}
                    check("sale:"+order.id,use(station(ShopStation.Kind.Checkout)),"Basket sold and receipt printed.");
                    check("duplicate_sale:"+order.id,!shift.Transactions.TrySell(order,shift.Stock,ShopShift.Products,out _),"Order identity prevents duplicate sale.");
                    check("conservation:"+order.id,ShopShift.Products.All(p=>shift.Stock.Total(p.sku)==6),"All accounts sum to original stock.");
                }
                check("receipts",shift.Transactions.Receipts.Count==3&&shift.Transactions.RevenueCents==1650,"Expected revenue $16.50; three unique receipts.");
                check("close",use(station(ShopStation.Kind.Close))&&shift.Closed&&shift.Tasks.Complete&&shift.StabilityContribution==3,"All tasks complete; stability contribution +3.");
                check("closed_mutation_rejected",!shift.UseShelf("cola",player)&&!shift.Checkout(player)&&!shift.Receive(player)&&!shift.Close(player),"Closed shift rejects repeat actions.");
                var ledger=new InventoryLedger();ledger.Seed("a","x",2);ledger.Seed("a","y",1);
                check("atomic_bundle",!ledger.TransferBundle("a","b",new[]{new StockLine("x",1),new StockLine("y",2)},"test")&&ledger.Count("a","x")==2&&ledger.Journal.Count==0,"Insufficient bundle leaves all accounts untouched.");
                check("negative_transfer",!ledger.Transfer("a","b","x",-1,"test"),"Negative quantities rejected.");
                camera.yaw=0;camera.PositionCamera();motor.Teleport(new Vector3(-4,.06f,3));for(int n=0;n<60;n++)motor.Step(Vector2.up,false,false,1f/60);
                check("walkable_west_aisle",motor.transform.position.z>4.7f,"Production controller walks clear aisle: "+motor.transform.position);
                motor.Teleport(new Vector3(-2,.06f,4));for(int n=0;n<120;n++)motor.Step(Vector2.up,false,false,1f/60);
                check("shelf_collision",motor.transform.position.z<5.5f,"Shelf collision blocks controller: "+motor.transform.position);
                motor.Teleport(new Vector3(0,.05f,3.2f));camera.pitch=70;camera.PositionCamera();check("camera_ceiling_clearance",camera.transform.position.y<3.1f,"Unchanged camera's spherecast remains under the solid ceiling: "+camera.transform.position);camera.pitch=20;camera.PositionCamera();
                foreach(var s in stations){if(s.kind==ShopStation.Kind.Exit)approach(s,new Vector3(0,0,1));else approach(s,new Vector3(0,0,-1.1f));}
                check("navigation_build_settings",EditorBuildSettings.scenes.Any(s=>s.path==ShopBuilder.ScenePath&&s.enabled)&&EditorBuildSettings.scenes.Any(s=>s.path=="Assets/Prototype/Scenes/TownPrototype.unity"&&s.enabled),"Both scenes enabled.");
                var visited=new HashSet<Vector2Int>();var queue=new Queue<Vector2Int>();var start=new Vector2Int(0,6);queue.Enqueue(start);visited.Add(start);
                var steps=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
                while(queue.Count>0){var cell=queue.Dequeue();foreach(var step in steps){var next=cell+step;if(next.x<-10||next.x>10||next.y<0||next.y>24||visited.Contains(next))continue;var p=new Vector3(next.x*.5f,.3f,next.y*.5f);if(Physics.CheckCapsule(p,p+Vector3.up*.66f,.24f,~(1<<2),QueryTriggerInteraction.Ignore))continue;visited.Add(next);queue.Enqueue(next);}}
                foreach(var s in stations){var goal=new Vector2Int(Mathf.RoundToInt(s.InteractionPoint.x*2),Mathf.RoundToInt((s.InteractionPoint.z+(s.kind==ShopStation.Kind.Exit?1:-1.1f))*2));check("collision_route:"+s.name,visited.Contains(goal),"Connected clear 0.5m-grid route from spawn for a player-sized capsule to "+goal);}
                r.summary=new ShopShift.ShiftSummary{receipts=shift.Transactions.Receipts.ToArray(),movements=shift.Stock.Journal.ToArray(),tasks=shift.Tasks.Tasks.ToArray(),revenueCents=shift.Transactions.RevenueCents,stabilityContribution=shift.StabilityContribution,timestampUtc=r.timestampUtc};
                foreach(var display in UnityEngine.Object.FindObjectsByType<ShopStockDisplay>(FindObjectsSortMode.None)){display.SendMessage("LateUpdate");check("remaining_stock_visual:"+display.sku,display.units.Count(o=>o.activeSelf)==3,"Visual stock reflects three remaining units.");}
                foreach(var till in UnityEngine.Object.FindObjectsByType<ShopTillDisplay>(FindObjectsSortMode.None))till.SendMessage("LateUpdate");
                CaptureAll();
            } catch(Exception e){check("unexpected_exception",false,e.ToString());Debug.LogException(e);}
            finally {FinishOrNavigate(r);}
        }
        static void FinishOrNavigate(Report report)
        {
            Action finish=()=>{File.WriteAllText(Path.Combine(Output,"playmode-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("Phase4 validation "+report.checks.Count(c=>c.passed)+"/"+report.checks.Count);EditorApplication.isPlaying=false;};
            if(report.checks.Any(c=>!c.passed)){finish();return;}
            var exit=UnityEngine.Object.FindObjectsByType<ShopStation>(FindObjectsSortMode.None).First(s=>s.kind==ShopStation.Kind.Exit);
            var motor=UnityEngine.Object.FindFirstObjectByType<ThirdPersonMotor>();var player=motor.GetComponent<PlayerInteractor>();
            motor.Teleport(exit.InteractionPoint+new Vector3(0,-.65f,1));Physics.SyncTransforms();player.FindNearby();
            bool completed=false;double deadline=EditorApplication.timeSinceStartup+15;
            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,UnityEngine.SceneManagement.LoadSceneMode> loaded=null;
            EditorApplication.CallbackFunction timeout=null;
            loaded=(scene,mode)=>{
                if(completed)return;completed=true;UnityEngine.SceneManagement.SceneManager.sceneLoaded-=loaded;EditorApplication.update-=timeout;
                var townPlayer=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<ThirdPersonMotor>()).FirstOrDefault();
                report.checks.Add(new Check{name="actual_exit_navigation",passed=scene.path=="Assets/Prototype/Scenes/TownPrototype.unity"&&townPlayer!=null,details="Production E interaction loaded "+scene.path+" with original player."});
                EditorApplication.delayCall+=()=>finish();
            };
            timeout=()=>{if(completed||EditorApplication.timeSinceStartup<deadline)return;completed=true;UnityEngine.SceneManagement.SceneManager.sceneLoaded-=loaded;EditorApplication.update-=timeout;report.checks.Add(new Check{name="actual_exit_navigation",passed=false,details="Scene load timed out."});finish();};
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=loaded;EditorApplication.update+=timeout;
            if(player.Current!=exit || !player.TryInteract()){completed=true;UnityEngine.SceneManagement.SceneManager.sceneLoaded-=loaded;EditorApplication.update-=timeout;report.checks.Add(new Check{name="actual_exit_navigation",passed=false,details="Exit interaction rejected."});finish();}
        }
        [MenuItem("Business as Usual/Phase 4/Capture Shop Views")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(Output);
            var go=new GameObject("Shop capture camera");var cam=go.AddComponent<Camera>();cam.fieldOfView=62;cam.nearClipPlane=.05f;cam.farClipPlane=60;cam.cullingMask=~(1<<2);cam.backgroundColor=new Color(.34f,.27f,.18f);cam.clearFlags=CameraClearFlags.SolidColor;
            try {
                cam.transform.position=new Vector3(-.7f,1.8f,.3f);cam.transform.LookAt(new Vector3(0,1.1f,7));CaptureView(cam,Path.Combine(Output,"cashier-store.png"));
                cam.cullingMask=~((1<<2)|(1<<30));cam.transform.position=new Vector3(0,16,6);cam.transform.rotation=Quaternion.Euler(90,0,0);cam.orthographic=true;cam.orthographicSize=7;CaptureView(cam,Path.Combine(Output,"overhead-layout.png"));
                cam.cullingMask=~(1<<2);cam.orthographic=false;cam.transform.position=new Vector3(-3.7f,1.3f,7.3f);cam.transform.LookAt(new Vector3(-3.6f,1.1f,10.9f));CaptureView(cam,Path.Combine(Output,"product-detail.png"));
                cam.transform.position=new Vector3(-3.8f,1.35f,10.2f);cam.transform.LookAt(new Vector3(-3.8f,1.2f,11.7f));CaptureView(cam,Path.Combine(Output,"refrigerator-detail.png"));
                cam.transform.position=new Vector3(-1,2.25f,.35f);cam.transform.LookAt(new Vector3(-1,1.1f,1.8f));CaptureView(cam,Path.Combine(Output,"checkout-detail.png"));
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        static void CaptureView(Camera camera,string path)
        {
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var rt=RenderTexture.GetTemporary(1920,1080,24);var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(texture);}
        }
    }
}
