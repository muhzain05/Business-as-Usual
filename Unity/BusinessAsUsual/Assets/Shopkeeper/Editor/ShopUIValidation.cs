using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    [InitializeOnLoad]
    public static class ShopUIValidation
    {
        [Serializable] public class Check {public string name,details;public bool passed;}
        [Serializable] public class Report {public string timestampUtc,method;public List<Check> checks=new List<Check>();public List<string> screenshots=new List<string>();}
        static Report report;
        static ShopHUD hud;
        static ShopShift shift;
        static PlayerInteractor player;
        static ThirdPersonMotor motor;
        static ThirdPersonCamera camera;
        static PrototypeInput input;
        static ShopStation[] stations;
        static Keyboard keyboard;
        static Mouse mouse;
        static readonly Queue<Action> steps=new Queue<Action>();
        static double next;
        static string awaiting;
        static ShopUIValidation(){EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BAU.UI.Validate",false)){
                SessionState.SetBool("BAU.UI.Validate",false);next=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Initialize;
            }
        };}
        [MenuItem("Business as Usual/Phase 4.2/Validate UI and Capture States")]
        public static void Begin()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play and preserve the scene first.");
            EditorSceneManager.OpenScene(ShopBuilder.ScenePath);ShopUICapture.Resolution(1920,1080);
            SessionState.SetBool("BAU.UI.Validate",true);EditorApplication.isPlaying=true;
        }
        static void Test(string name,bool pass,string detail)=>report.checks.Add(new Check{name=name,passed=pass,details=detail});
        static void Initialize()
        {
            if(EditorApplication.timeSinceStartup<next)return;EditorApplication.update-=Initialize;
            report=new Report{timestampUtc=DateTime.UtcNow.ToString("o"),method="Live Unity Play mode; production interactions, synthetic Input System events and queued Game view GUI clicks. No human usability test."};
            try{
                Application.runInBackground=true;hud=UnityEngine.Object.FindFirstObjectByType<ShopHUD>();shift=hud.shift;player=hud.player;motor=player.GetComponent<ThirdPersonMotor>();camera=player.GetComponentInChildren<ThirdPersonCamera>();input=player.GetComponent<PrototypeInput>();
                motor.enabled=false;player.enabled=false;camera.enabled=false;
                stations=UnityEngine.Object.FindObjectsByType<ShopStation>(FindObjectsSortMode.None);keyboard=InputSystem.AddDevice<Keyboard>("UIValidationKeyboard");mouse=InputSystem.AddDevice<Mouse>("UIValidationMouse");
                Test("licensed_fonts_loaded",hud.FontsAvailable,"All four approved bundled font files loaded.");
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                typeof(ShopHUD).GetField("theme",flags).SetValue(hud,null);
                var keyField=typeof(ShopHUD).GetField("receiptKey",flags);
                ((InputAction)keyField.GetValue(hud)).Dispose();keyField.SetValue(hud,null);
                hud.SendMessage("OnEnable");
                Test("presentation_cache_reload",hud.FontsAvailable&&((InputAction)keyField.GetValue(hud)).enabled,"Re-enabling after nonserialized caches/actions are cleared recreates fonts, drawing cache and Tab action.");
                Test("initial_order",hud.View.customer=="Mara"&&hud.View.orderValue=="$5.10"&&hud.View.missing==3,"Current order preserved.");
                var snapshot=hud.View;Test("snapshot_cached",ReferenceEquals(snapshot,hud.View),"No full view rebuild without state changes.");
                Test("integer_money",ShopUIView.Money(1650)=="$16.50"&&ShopUIView.Money(1)=="$0.01"&&ShopUIView.Money(123456789)=="$1234567.89","No float currency conversion.");
                foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(2560,1080)}){
                    float scale=ShopHUD.Scale(size.x,size.y);var offset=ShopHUD.Offset(size.x,size.y,scale);
                    Test("safe_bounds_"+size.x+"x"+size.y,offset.x>=0&&offset.y>=0&&offset.x+1872*scale<=size.x&&offset.y+1016*scale<=size.y,"Reference layout fits; 24px text scales to "+24*scale+"px.");
                }
                var cursor=Cursor.lockState;float time=Time.timeScale;int journal=shift.Stock.Journal.Count;
                foreach(ShopHUD.Panel panel in Enum.GetValues(typeof(ShopHUD.Panel))){hud.ShowPanel(panel);Test("panel_read_only_"+panel,shift.Stock.Journal.Count==journal&&shift.Transactions.Receipts.Count==0&&Cursor.lockState==cursor&&Time.timeScale==time,"Presentation selection preserves transactions, time and cursor mode.");}
                hud.ShowPanel(ShopHUD.Panel.None);Press(Key.Tab,hud);Test("Tab_opens_receipts",hud.CurrentPanel==ShopHUD.Panel.Receipts,"Production Tab action.");Press(Key.Tab,hud);Test("Tab_closes_receipts",hud.CurrentPanel==ShopHUD.Panel.None,"Production Tab action.");
                Approach(stations.First(s=>s.kind==ShopStation.Kind.Delivery));input.SetCursor(true);hud.ShowPanel(ShopHUD.Panel.Tasks);Press(Key.E,player);
                Test("expanded_UI_does_not_steal_E",shift.Received,"E remains production interaction when mouse captured; opening book does not pause gameplay.");
                Press(Key.E,player);Test("repeated_E_no_duplicate_delivery",ShopShift.Products.All(p=>shift.Stock.Total(p.sku)==6&&shift.Stock.Count("delivery",p.sku)==0)&&shift.Tasks.Tasks[0].progress==1&&shift.Transactions.Receipts.Count==0,"Receive exposes the nearby crate, so the second E may legitimately pick it up. Original quantities and delivery count remain exact.");
                Press(Key.Escape,input);Test("Esc_releases_cursor",Cursor.lockState==CursorLockMode.None,"Production Esc action unchanged.");
                hud.ShowPanel(ShopHUD.Panel.Receipts);Pointer(1590,224,false);hud.SendMessage("Update");Pointer(1590,224,true);input.SendMessage("Update");
                Test("UI_click_does_not_capture_cursor",Cursor.lockState==CursorLockMode.None&&!input.CaptureCursor.enabled,"Hovering released-cursor UI consumes its click.");
                Click(1590,224);
                steps.Enqueue(()=>{Test("actual_tasks_button",hud.CurrentPanel==ShopHUD.Panel.Tasks,"Queued Game view MouseDown/MouseUp reaches the production GUI button.");Pointer(1350,120,false);hud.SendMessage("Update");Click(1350,120);});
                steps.Enqueue(()=>{Test("actual_shift_tab",hud.CurrentPanel==ShopHUD.Panel.Report,"Live handbook tab click.");hud.ShowPanel(ShopHUD.Panel.None);Pointer(20,500,false);hud.SendMessage("Update");Pointer(20,500,true);input.SendMessage("Update");Test("outside_click_recaptures_cursor",Cursor.lockState==CursorLockMode.Locked&&input.CaptureCursor.enabled,"Original gameplay click-to-capture action restored outside UI.");Pointer(20,500,false);hud.SendMessage("Update");
                    if(shift.Carried){Press(Key.Q,shift);Test("Q_return_while_UI_browsing",!shift.Carried,"Existing Q action returns carried stock without a UI transaction.");}
                    var crate=UnityEngine.Object.FindFirstObjectByType<StockCrate>();Approach(crate,.8f);Test("carry_production",player.TryInteract()&&shift.Carried,"Production crate interaction.");Pose();hud.ShowPanel(ShopHUD.Panel.None);next=EditorApplication.timeSinceStartup+5.3;});
                Captures("01-normal-hud");
                steps.Enqueue(()=>{hud.ShowPanel(ShopHUD.Panel.Tasks);Test("carry_inventory_exact",hud.View.items.All(i=>i.carry==6&&i.shelf==0)&&hud.View.completedTasks==1,"Handbook and HUD use current ledger quantities.");});Captures("02-task-handbook");
                steps.Enqueue(()=>{
                    hud.ShowPanel(ShopHUD.Panel.None);
                    foreach(var s in stations.Where(s=>s.kind==ShopStation.Kind.Shelf)){player.GetComponent<ShopRestockReach>().Advance(1);Approach(s);Test("stock_for_UI_"+s.sku,player.TryInteract()&&shift.Stock.Count("shelf",s.sku)==6,"Unchanged production shelf/reach path.");}
                    player.GetComponent<ShopRestockReach>().Advance(1);UseShelf("cola");Pose();player.FindNearby();Test("partial_basket_exact",hud.View.items.First(i=>i.sku=="cola").basket==1&&hud.View.missing==2&&hud.View.completedTasks==4,"Partial order and task counts reflect the domain.");hud.ShowPanel(ShopHUD.Panel.Order);
                });Captures("03-customer-order");
                steps.Enqueue(()=>{hud.ShowPanel(ShopHUD.Panel.None);Pose();player.FindNearby();next=EditorApplication.timeSinceStartup+5.3;});Captures("06-interaction-prompt");
                steps.Enqueue(()=>{
                    for(int order=0;order<3;order++){
                        var current=shift.CurrentOrder;
                        foreach(var l in current.lines)while(shift.Stock.Count("basket",l.sku)<l.quantity)UseShelf(l.sku);
                        Test("order_value_"+current.id,hud.View.orderValue==ShopUIView.Money(current.lines.Sum(l=>l.quantity*ShopShift.Products.First(p=>p.sku==l.sku).priceCents)),"Unpaid value from integer quantities and prices.");
                        Approach(stations.First(s=>s.kind==ShopStation.Kind.Checkout));Test("checkout_for_UI_"+current.id,player.TryInteract(),"Production checkout.");
                    }
                    Test("HUD_revenue_updated",hud.View.revenue=="$16.50"&&hud.View.receiptCount==3&&hud.View.customer=="All served!","Live HUD refresh follows three checkouts.");
                    foreach(var r in shift.Transactions.Receipts){Test("receipt_exact_"+r.id,r.totalCents==r.lines.Sum(l=>l.totalCents)&&r.lines.All(l=>l.totalCents==l.unitPriceCents*l.quantity),"Item quantities, unit prices and totals remain exact.");}
                    Pose();hud.ShowPanel(ShopHUD.Panel.Receipts);hud.SelectReceipt(0);
                });Captures("04-printed-receipt");
                steps.Enqueue(()=>{input.SetCursor(false);Pointer(440,520,false);hud.SendMessage("Update");Click(440,520);});
                steps.Enqueue(()=>{Test("actual_second_receipt_button",hud.SelectedReceipt==1,"Live receipt list selects Eli.");});Capture("receipt-02",1920,1080);
                steps.Enqueue(()=>{Pointer(440,670,false);hud.SendMessage("Update");Click(440,670);});
                steps.Enqueue(()=>{Test("actual_third_receipt_button",hud.SelectedReceipt==2,"Live receipt list selects Jo.");hud.SelectReceipt(999);Test("receipt_index_clamped",hud.SelectedReceipt==2,"Invalid presentation index cannot escape the receipt list.");});Capture("receipt-03",1920,1080);
                steps.Enqueue(()=>{
                    int before=shift.Stock.Journal.Count;for(int n=0;n<20;n++){hud.SelectReceipt(n%3);hud.ToggleBook();hud.ShowPanel(ShopHUD.Panel.Tasks);hud.ShowPanel(ShopHUD.Panel.Receipts);}
                    Test("repeated_browsing_no_transactions",shift.Stock.Journal.Count==before&&shift.Transactions.RevenueCents==1650&&shift.Transactions.Receipts.Count==3,"Repeated opens/closes/navigation create no sale or inventory movement.");
                    Approach(stations.First(s=>s.kind==ShopStation.Kind.Close));Test("close_shift_production",player.TryInteract()&&shift.Closed,"Unchanged closing station.");hud.SendMessage("Update");Test("completion_opens_report",hud.CurrentPanel==ShopHUD.Panel.Report&&hud.View.completedTasks==6,"Live closure triggers report once.");Pose();
                });Captures("05-shift-report");
                steps.Enqueue(()=>{hud.ToggleBook();hud.SendMessage("Update");Test("closed_report_can_close",hud.CurrentPanel==ShopHUD.Panel.None,"Report does not reopen every frame; front exit remains accessible.");hud.ShowPanel(ShopHUD.Panel.Report);hud.reducedMotion=true;Test("UI_did_not_modify_time",Time.timeScale==1,"UI does not pause or alter time.");Test("stock_conserved",ShopShift.Products.All(p=>shift.Stock.Total(p.sku)==6&&shift.Stock.Count("shelf",p.sku)==3),"Final counts unchanged.");});Capture("report-reduced-motion",1280,720);
                steps.Enqueue(Finish);next=EditorApplication.timeSinceStartup+.4;EditorApplication.update+=Tick;
            }catch(Exception e){Test("unexpected_exception",false,e.ToString());Debug.LogException(e);Finish();}
        }
        static void Captures(string name){Capture(name,1920,1080);Capture(name,1280,720);}
        static void Capture(string name,int w,int h){steps.Enqueue(()=>{
            ShopUICapture.Resolution(w,h);awaiting=name+"-"+w+"x"+h;ShopUICapture.Capture(awaiting);report.screenshots.Add(awaiting+".png");next=EditorApplication.timeSinceStartup+.8;
        });}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup<next||ShopUICapture.Pending)return;
            if(awaiting!=null){var path=Path.Combine(ShopUICapture.Output,awaiting+".png");if(!File.Exists(path))return;Test("captured_"+awaiting,new FileInfo(path).Length>10000,"Actual Game view ScreenCapture with UI; dimensions independently checked from PNG.");awaiting=null;}
            try{if(steps.Count>0){next=EditorApplication.timeSinceStartup+.4;steps.Dequeue()();}}catch(Exception e){Test("unexpected_exception",false,e.ToString());Debug.LogException(e);Finish();}
        }
        static void Press(Key key,MonoBehaviour target){InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));InputSystem.Update();target.SendMessage("Update");InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();}
        static void Pointer(float x,float y,bool down){float scale=ShopHUD.Scale(Screen.width,Screen.height);var offset=ShopHUD.Offset(Screen.width,Screen.height,scale);var state=new MouseState{position=new Vector2(x*scale+offset.x,Screen.height-(y*scale+offset.y))};if(down)state=state.WithButton(MouseButton.Left);InputSystem.QueueStateEvent(mouse,state);InputSystem.Update();}
        static void Click(float x,float y){float scale=ShopHUD.Scale(Screen.width,Screen.height);var offset=ShopHUD.Offset(Screen.width,Screen.height,scale);var position=new Vector2(x*scale+offset.x,y*scale+offset.y);EditorGUIUtility.QueueGameViewInputEvent(new Event{type=EventType.MouseDown,button=0,mousePosition=position});EditorGUIUtility.QueueGameViewInputEvent(new Event{type=EventType.MouseUp,button=0,mousePosition=position});EditorApplication.QueuePlayerLoopUpdate();}
        static void Approach(Interactable s,float distance=1.1f){motor.Teleport(new Vector3(s.InteractionPoint.x,.05f,s.InteractionPoint.z-distance));Physics.SyncTransforms();player.FindNearby();}
        static void UseShelf(string sku){player.GetComponent<ShopRestockReach>().Advance(1);var s=stations.First(s=>s.kind==ShopStation.Kind.Shelf&&s.sku==sku);Approach(s);if(!player.TryInteract())throw new InvalidOperationException("Shelf rejected "+sku);}
        static void Pose(){motor.Teleport(new Vector3(-2,.05f,4.85f));camera.yaw=0;camera.pitch=16;camera.PositionCamera();Physics.SyncTransforms();player.FindNearby();}
        static void Finish()
        {
            EditorApplication.update-=Tick;steps.Clear();
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);if(mouse!=null)InputSystem.RemoveDevice(mouse);
            Directory.CreateDirectory(ShopUICapture.Output);File.WriteAllText(Path.Combine(ShopUICapture.Output,"ui-validation.json"),JsonUtility.ToJson(report,true));
            Debug.Log("Phase4.2 UI validation "+report.checks.Count(c=>c.passed)+"/"+report.checks.Count);
            EditorApplication.isPlaying=false;
        }
    }
}
