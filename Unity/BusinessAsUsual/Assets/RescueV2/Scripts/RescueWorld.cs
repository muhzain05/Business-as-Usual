using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BusinessAsUsual.Town;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using TMPro;
namespace BusinessAsUsual.RescueV2
{
    // Presentation follows accepted custody. No stock is awarded by animation or local UI.
    public sealed class RescueWorld:MonoBehaviour
    {
        LivingWorkday work;NetworkShop shop;Transform root,wagon,animatedModel;
        AnimationClip hoodClip;float hoodTime;Material teal,ink,paper,mustard,outlineMaterial,goldenPrint;
        readonly Dictionary<string,Transform> items=new Dictionary<string,Transform>();
        readonly Dictionary<string,Renderer[]> affordances=new Dictionary<string,Renderer[]>();
        readonly List<GameObject> outlines=new List<GameObject>();
        string highlighted;
        UnityEngine.Mesh goldenMesh;
        public RescueCart Cart {get;private set;}
        public Transform ItemTransform(string id)=>items.TryGetValue(id,out var t)?t:null;
        public void BindWorkPoint(string name,Transform visual)=>Bind(name,visual);
        public static Vector3 Stockroom(int index)=>new Vector3(-35.65f,.025f,8.45f+index*.50f);
        public static Vector3 Shelf(int index)=>index==0?new Vector3(-42,.635f,5.77f):index==1?new Vector3(-40,.635f,9.47f):new Vector3(-38,.635f,5.77f);
        public Vector3 BatterySeat=>new Vector3(6.635f,.528f,5.435f);
        public void Build(LivingWorkday w,NetworkShop adapter,Transform parent){
            work=w;shop=adapter;root=parent;teal=Material("Town teal",new Color(.12f,.28f,.26f));ink=Material("Town graphite",new Color(.08f,.13f,.13f));paper=Material("Work paper",new Color(.95f,.88f,.70f));mustard=Material("Marked handle",new Color(.80f,.60f,.18f));
            goldenPrint=Resources.Load<Material>("RescueV2/Models/GoldenPrint");goldenMesh=Resources.Load<UnityEngine.Mesh>("RescueV2/Models/GoldenPrintMesh");foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))CorrectPackaging(renderer);
            foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)){
                // Imported meshes repeat their wrapper name. Resize/reposition the fixture once.
                if(t.parent&&t.parent.name==t.name)continue;
                if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&t.name=="WasteCart")t.gameObject.SetActive(false);
                var oldLabel=t.GetComponent<TextMesh>();if(oldLabel&&(oldLabel.text.StartsWith("BAY",StringComparison.OrdinalIgnoreCase)||oldLabel.text.StartsWith("EXIT TO TOWN")))oldLabel.gameObject.SetActive(false);
                if(t.name=="sign backing"&&Vector3.Distance(t.position,new Vector3(-40,2.4f,-.25f))<.12f)t.gameObject.SetActive(false);
                if(t.name=="ServiceVan"&&t.position.y>1)t.gameObject.SetActive(false);
                if(t.name=="Service Vehicle 0"||t.name=="Service Vehicle 1"){foreach(Transform child in t)child.gameObject.SetActive(false);foreach(var c in t.GetComponents<Collider>())c.enabled=false;}
                if(t.name=="Service Desk"||t.name=="Collection Route Desk"){
                    foreach(var renderer in t.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                    foreach(var collider in t.GetComponentsInChildren<Collider>())collider.enabled=false;
                    Desk(t.position.x,t.position.z);
                }
                if(t.name=="Delivery Desk"){t.localScale=new Vector3(1.2f,.72f,.70f);t.position=new Vector3(t.position.x,.36f,t.position.z);}
                if(t.name=="Checkout"){t.localScale=new Vector3(t.localScale.x,t.localScale.y*.75f/1.1f,t.localScale.z);t.position=new Vector3(t.position.x,t.position.y*.75f/1.1f,t.position.z);}
                if(t.name=="Counter"&&t.position.x<-25)t.localScale=new Vector3(t.localScale.x,t.localScale.y*.68f,t.localScale.z);
                if(new[]{"Register","Scanner","Basket"}.Contains(t.name)&&Vector3.Distance(t.position,new Vector3(-41,1,1.8f))<3)t.position=new Vector3(t.position.x,.75f,t.position.z);
                if(t.name.StartsWith("Product_")&&Mathf.Abs(t.position.y-1.07f)<.03f&&Mathf.Abs(t.position.z-2.08f)<.08f)t.position-=Vector3.up*.32f;
                if(t.name=="Scanner cable"){var line=t.GetComponent<LineRenderer>();if(line){var points=new Vector3[line.positionCount];line.GetPositions(points);for(int i=0;i<points.Length;i++)points[i].y-=.32f;line.SetPositions(points);}}
            }
            wagon=Instantiate(Resources.Load<GameObject>("RescueV2/Models/WagonRig"),root).transform;wagon.position=new Vector3(6.4f,.005f,6.3f);animatedModel=wagon.GetChild(0);var anim=animatedModel.GetComponent<Animation>();if(anim)anim.enabled=false;hoodClip=Resources.Load<AnimationClip>("RescueV2/Models/HoodOpen");
            var engine=Instantiate(Resources.Load<GameObject>("RescueV2/Models/EngineRig"),root);engine.transform.position=new Vector3(6.20f,.46f,5.42f);
            Box("Cabin collision",new Vector3(6.4f,.75f,6.60f),new Vector3(1.04f,1.4f,1.58f),null);
            foreach(float x in new[]{5.95f,6.85f})Box("Front wing collision",new Vector3(x,.33f,5.40f),new Vector3(.20f,.55f,.62f),null);
            Box("Front bumper collision",new Vector3(6.4f,.23f,5.04f),new Vector3(1.04f,.25f,.15f),null);
            var second=Instantiate(Resources.Load<GameObject>("RescueV2/Models/WagonRig"),root);second.transform.position=new Vector3(9.6f,.005f,6.3f);foreach(var a in second.GetComponentsInChildren<Animation>())a.enabled=false;Box("Waiting wagon collision",new Vector3(9.6f,.73f,6.3f),new Vector3(1.15f,1.4f,2.5f),null);
            foreach(var item in w.Domain.State.items){string prefab=item.id=="OLD-8A-001"||item.id=="PART-8A-001"?"Battery":item.id=="BAG-8A-001"?"CollectionBag":"Carton";var v=Instantiate(Resources.Load<GameObject>("RescueV2/Models/"+prefab),root).transform;v.name=item.id;var c=v.gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,.12f,0);c.size=prefab=="Battery"?new Vector3(.25f,.25f,.28f):new Vector3(.42f,.34f,.36f);v.gameObject.layer=2;items[item.id]=v;
                if(item.material=="stock"){var container=new GameObject(item.id+" contents").transform;container.SetParent(v,false);container.name="Retail product";string brand=item.sku=="cola"?"Sunbeam":item.sku=="tea"?"Meadow":"Golden";Instantiate(Resources.Load<GameObject>("RescueV2/Models/Product_"+brand),container,false);container.gameObject.SetActive(false);var label=new GameObject("Product label").AddComponent<TextMeshPro>();label.transform.SetParent(v,false);label.transform.localPosition=new Vector3(0,.17f,-.183f);label.font=Resources.Load<RescueSkin>("RescueV2/WorkdaySkin").labels;label.fontSize=.55f;label.color=new Color(.12f,.25f,.24f);label.text=item.sku.ToUpperInvariant();label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(.30f,.08f);}
            }
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))CorrectPackaging(renderer);
            // Actual contact points on the latch, posts, clamp, receiving bench and scrap tray.
            Position("Hood",new Vector3(6.4f,.735f,5.07f),"Open hood");Position("Probes",BatterySeat+new Vector3(-.085f,.229f,0),"Check battery");Position("Terminal",BatterySeat+new Vector3(.10f,.17f,-.06f),"Loosen clamp");Position("Socket",BatterySeat+Vector3.up*.10f,"Install battery");
            work.Stations["Probes"].altLabel="Load test";
            Position("Request",new Vector3(8.2f,.86f,7.20f),"Town supply");Position("Bay tray",new Vector3(8.20f,.74f,7.20f),"Place parcel");
            Position("Metal pickup",new Vector3(5.30f,.18f,7.80f),"Collect old battery");Position("Paper pickup",new Vector3(5.30f,.12f,8.30f),"Collect packaging");
            Position("Shipment",new Vector3(-36,.73f,10.30f),"Open shipment");Position("Dispatch",new Vector3(-36,.73f,10.30f),"Pack garage order");Position("Parcel",new Vector3(-36,.90f,10.30f),"Carry garage parcel");Position("Counter",new Vector3(-41,.82f,1.80f),"Serve customer");
            Position("Manifest",new Vector3(10,.75f,-6.2f),"File collection ticket");Position("Shift report",new Vector3(0,.75f,4.7f),"File town report");
            Box("Town report desk",new Vector3(0,.70f,4.7f),new Vector3(.70f,.08f,.45f),teal);foreach(float x in new[]{-.27f,.27f})Box("Town report stand",new Vector3(x,.33f,4.7f),new Vector3(.06f,.66f,.34f),ink);
            var report=Box("Town report clipboard",new Vector3(0,.746f,4.7f),new Vector3(.24f,.012f,.31f),paper);Bind("Shift report",report);
            Add("Lift battery",BatterySeat+Vector3.up*.17f,"remove-battery",2,"Lift old battery");Add("Scrap tray",new Vector3(5.30f,.28f,7.80f),"scrap-battery",2,"Place old battery");Add("Replacement",new Vector3(8.2f,.88f,7.20f),"grab-battery",2,"Grab replacement");Add("Repair test",BatterySeat+new Vector3(.085f,.229f,0),"test-repair",2,"Test repair");Add("Service ticket",new Vector3(7.65f,.75f,7.20f),"service-ticket",2,"Sign service ticket");
            for(int i=0;i<3;i++){string sku=w.Domain.State.items[4+i].sku;var shelf=shop.Targets.Values.OfType<ShopStation>().First(t=>t.kind==ShopStation.Kind.Shelf&&t.sku==sku);var solid=shelf.GetComponent<Collider>();Vector3 point=Shelf(i)+Vector3.up*.12f;if(solid)point.z=solid.bounds.min.z-.045f;Add("Stock box "+i,Stockroom(i)+Vector3.up*.18f,"take-stock-"+i,1,"Lift "+sku+" box");Add("Stock shelf "+i,point,"stock-"+i,1,"Stock "+sku+" shelf");Add("Shelf product "+i,point,"take-stock-"+i,1,"Collect "+sku);Bind("Stock box "+i,items[w.Domain.State.items[4+i].id]);Bind("Stock shelf "+i,items[w.Domain.State.items[4+i].id]);Bind("Shelf product "+i,items[w.Domain.State.items[4+i].id]);}
            Add("Order counter",new Vector3(-41.7f,.80f,1.9f),"place-order",1,"Place customer order");Add("Order register",new Vector3(-40.3f,.91f,1.8f),"scan-order",1,"Scan customer order");work.Stations["Order register"].alternative="priority";work.Stations["Order register"].altLabel="Change priority";
            var receipt=Box("Receipt output",new Vector3(-40.1f,.754f,1.86f),new Vector3(.11f,.008f,.22f),paper);Bind("Order register",receipt);
            Box("Scrap tray base",new Vector3(5.30f,.08f,7.80f),new Vector3(.52f,.06f,.44f),ink);foreach(float x in new[]{5.05f,5.55f})Box("Scrap tray lip",new Vector3(x,.12f,7.8f),new Vector3(.025f,.08f,.44f),teal);
            var ticket=Box("Service clipboard",new Vector3(7.65f,.737f,7.20f),new Vector3(.24f,.012f,.31f),paper);Bind("Service ticket",ticket);
            Position("Garage reset",new Vector3(7.40f,.77f,7.55f),"Reset tool tray");var toolTray=Box("Tool return tray",new Vector3(7.40f,.746f,7.55f),new Vector3(.28f,.012f,.20f),ink);Bind("Garage reset",toolTray);
            var latch=Box("Hood handle cue",new Vector3(6.4f,.729f,5.075f),new Vector3(.10f,.025f,.04f),mustard);Bind("Hood",latch);
            var clamp=Box("Battery clamp",BatterySeat+new Vector3(.105f,.155f,-.075f),new Vector3(.025f,.04f,.12f),mustard);Bind("Terminal",clamp);
            foreach(var n in new[]{"Probes","Repair test","Lift battery","Socket"})Bind(n,items["OLD-8A-001"]);
            Bind("Scrap tray",items["OLD-8A-001"]);Bind("Replacement",items["PART-8A-001"]);Bind("Bay tray",items["PACK-8A-001"]);
            foreach(var n in new[]{"Shipment","Dispatch","Parcel"})Bind(n,items["PACK-8A-001"]);
            Bind("Square pickup",items["BAG-8A-001"]);Bind("Metal pickup",items["OLD-8A-001"]);Bind("Paper pickup",items["PACK-8A-001"]);
            Sign("Town supply",new Vector3(8.20f,1.0f,7.43f),"TOWN SUPPLY",.025f);Box("Supply sign post",new Vector3(8.2f,.86f,7.45f),new Vector3(.025f,.24f,.025f),ink);Sign("Scrap tray",new Vector3(5.30f,.13f,7.53f),"USED BATTERY",.015f);
            Sign("Service bay",new Vector3(6.4f,.012f,4.55f),"01 / REPAIR BAY",.018f,90);
            var cart=Instantiate(Resources.Load<GameObject>("RescueV2/Models/CartRig"),root);Cart=cart.AddComponent<RescueCart>();Cart.Setup(work,shop);Bind("Collection cart",cart.transform);
            foreach(float x in new[]{-40.79f,-39.21f})Box("Market door jamb",new Vector3(x,1.10f,-.12f),new Vector3(.10f,2.20f,.15f),teal);
            Box("Market door lintel",new Vector3(-40,2.23f,-.12f),new Vector3(1.70f,.12f,.15f),teal);
            Box("Market entrance transom",new Vector3(-40,2.71f,-.19f),new Vector3(3.62f,.94f,.08f),teal);
            foreach(float x in new[]{-41.29f,-38.71f})Box("Market entrance infill",new Vector3(x,1.10f,-.19f),new Vector3(.94f,2.20f,.08f),teal);
            Sign("Town exit",new Vector3(-40,2.37f,-.10f),"TOWN SQUARE",.028f);var exitLabel=root.Find("Town exit label");if(exitLabel)exitLabel.rotation=Quaternion.Euler(0,180,0);
            gameObject.AddComponent<RescueTownView>().Setup();
        }
        Material Material(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.2f);return m;}
        Transform Box(string name,Vector3 at,Vector3 size,Material material){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root);g.transform.position=at;g.transform.localScale=size;if(material){g.GetComponent<Renderer>().sharedMaterial=material;g.layer=2;}else g.GetComponent<Renderer>().enabled=false;return g.transform;}
        void Desk(float x,float z){Box("Human scale workbench",new Vector3(x,.70f,z),new Vector3(1.60f,.08f,.65f),teal);foreach(float dx in new[]{-.65f,.65f})foreach(float dz in new[]{-.22f,.22f})Box("Workbench leg",new Vector3(x+dx,.33f,z+dz),new Vector3(.06f,.66f,.06f),ink);}
        void Sign(string name,Vector3 at,string text,float size,float pitch=0){var g=new GameObject(name+" label");g.transform.SetParent(root);g.transform.position=at;g.transform.rotation=Quaternion.Euler(pitch,0,0);var t=g.AddComponent<TextMeshPro>();t.font=Resources.Load<RescueSkin>("RescueV2/WorkdaySkin").labels;t.text=text;t.fontSize=10;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.95f,.89f,.72f);t.rectTransform.sizeDelta=new Vector2(30,3);t.ForceMeshUpdate();float width=Mathf.Max(.01f,t.textBounds.size.x);g.transform.localScale=Vector3.one*(size*35/width);}
        void Position(string name,Vector3 at,string label){var s=work.Stations[name];s.transform.position=at;s.label=label;}
        void Add(string name,Vector3 at,string action,int job,string label)=>work.Make(name,at,action,null,job,root,label,null);
        void Bind(string name,Transform visual)=>affordances[name]=visual.GetComponentsInChildren<Renderer>();
        public void Tick(){
            var s=work.Domain.State;float next=s.hood?hoodClip.length:0;hoodTime=Mathf.MoveTowards(hoodTime,next,Time.deltaTime);hoodClip.SampleAnimation(animatedModel.gameObject,hoodTime);
            if(Cart)Cart.ApplyPose();
            var players=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Where(p=>p.IsSpawned).ToArray();
            foreach(var p in players){p.Interactor.originHeight=.95f;if(!p.GetComponent<RescueFootPlant>())p.gameObject.AddComponent<RescueFootPlant>();if(p.IsOwner&&!p.GetComponent<RescueHands>()){var hands=p.gameObject.AddComponent<RescueHands>();hands.Setup(p);}}
            foreach(var item in s.items){var v=items[item.id];bool visible=item.custody!="unobserved"&&item.custody!="recorded"&&item.custody!="sorted"&&item.custody!="customer";
                // The sealed parcel has one carton silhouette, with its battery inside.
                if(item.id=="PART-8A-001"&&new[]{"shipment","market","dispatch","parcel","garage","returns","yard-hold"}.Contains(item.custody))visible=false;
                if(item.material=="stock"){var product=v.Find("Retail product");if(product){foreach(Transform child in v)if(child!=product)child.gameObject.SetActive(!item.shelved);product.gameObject.SetActive(item.shelved);}}
                v.gameObject.SetActive(visible);if(!visible)continue;
                var holder=players.FirstOrDefault(p=>p.OwnerClientId==item.holder);Vector3 at;
                if(holder)at=holder.Interactor.carryAnchor.position-Vector3.up*.12f;
                else if(item.custody=="cart")at=Cart.Slot(Array.IndexOf(s.items,item));
                else if(item.custody=="returns")at=new Vector3(-2.92f,.28f,6.42f);
                else if(item.custody=="yard-hold")at=new Vector3(12.02f,.28f,-6.52f);
                else if(item.custody=="stockroom")at=Stockroom(Array.IndexOf(s.items,item)-4);
                else if(item.custody=="shelf")at=Shelf(Array.IndexOf(s.items,item)-4);
                else if(item.custody=="checkout")at=new Vector3(-41.7f,.75f,1.9f);
                else if(item.custody=="vehicle"||item.custody=="installed")at=BatterySeat;
                else if(item.custody=="garage")at=new Vector3(8.2f,.74f,7.20f);
                else if(item.custody=="shipment"||item.custody=="market"||item.custody=="dispatch")at=work.Stations["Shipment"].transform.position;
                else if(item.custody=="square")at=new Vector3(-1,.025f,7.8f);
                else if(item.custody=="counter")at=work.Stations["Counter"].transform.position+Vector3.right*(Array.IndexOf(s.items,item)-5)*.30f;
                else if(item.custody=="yard")at=new Vector3(10,.025f,-7);
                else at=item.id=="PACK-8A-001"?new Vector3(5.30f,.025f,8.30f):new Vector3(5.30f,.11f,7.80f);
                if(item.holder==WorkdayDomain.MunicipalActor)at=item.custody=="removed-carry"?new Vector3(5.30f,.11f,7.80f):item.custody=="component-carry"?new Vector3(8.2f,.74f,7.2f):new Vector3(10,.025f,-6.8f);
                v.position=at;v.rotation=holder?Quaternion.Euler(0,holder.LookYaw.Value,0):item.custody=="cart"?Cart.transform.rotation:Quaternion.identity;v.GetComponent<Collider>().enabled=!holder;
            }
            var local=players.FirstOrDefault(p=>p.IsOwner);if(local){var guide=Guidance(local);Highlight(guide.objectives.FirstOrDefault()?.target?.name);}
        }
        void Highlight(string name){if(highlighted==name)return;highlighted=name;foreach(var g in outlines)Destroy(g);outlines.Clear();if(name==null||!affordances.TryGetValue(name,out var renderers))return;
            if(!outlineMaterial){var shader=Shader.Find("BusinessAsUsual/Rescue Outline");if(!shader)return;outlineMaterial=new Material(shader);outlineMaterial.SetColor("_Color",new Color(.95f,.72f,.18f));}
            foreach(var r in renderers){var filter=r.GetComponent<MeshFilter>();if(!filter)continue;var g=new GameObject("Active work outline");g.transform.SetParent(r.transform,false);g.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;g.AddComponent<MeshRenderer>().sharedMaterial=outlineMaterial;outlines.Add(g);}
        }
        void CorrectPackaging(Renderer renderer){if(goldenPrint&&renderer.sharedMaterials.Any(m=>m&&m.name=="Shop_Golden")){renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m&&m.name=="Shop_Golden"?goldenPrint:m).ToArray();var filter=renderer.GetComponent<MeshFilter>();if(filter&&goldenMesh)filter.sharedMesh=goldenMesh;}}
        void OnDestroy(){foreach(var m in new[]{teal,ink,paper,mustard,outlineMaterial})if(m)Destroy(m);}
        public TownGuidance Guidance(NetworkPlayer player){
            var s=work.Domain.State;var job=shop.Town.Job(player.OwnerClientId);var held=work.Domain.Held(player.OwnerClientId);var g=new TownGuidance{job=job};string next=null;
            var pin=BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&player.IsOwner&&RescueInterface.Instance?RescueInterface.Instance.PinnedErrand:null;if(pin){g.note="Visit "+pin.name;g.detail="An ordinary shared staging point. Private pin stays on your copy.";g.objectives.Add(new TownObjective{id=NetworkShop.Id(pin),title=pin.name,icon=TownGuidance.RoleIcon(job),target=pin});return g;}
            void Step(string station,string note,string detail){next=station;g.note=note;g.detail=detail;}
            if(s.closed){g.note="Shift finished";g.detail="Review the work tickets before starting another day.";return g;}
            if(held?.custody=="parcel")Step("Bay tray","Deliver garage parcel","Place the sealed carton on the garage receiving bench.");
            else if(held?.custody=="stock-carry"){int index=Array.IndexOf(s.items,held)-4;Step(held.shelved?"Order counter":"Stock shelf "+index,held.shelved?"Place customer order":"Stock "+held.sku+" shelf",held.shelved?"Match the current order; E sets the product on the counter.":"Place this stock on its matching printed shelf label.");}
            else if(held?.custody=="removed-carry")Step("Scrap tray","Place old battery","Set the old battery in the low scrap tray.");
            else if(held?.custody=="component-carry")Step("Socket","Install replacement","Seat the battery in the empty engine tray.");
            else if(held?.custody=="waste-carry"){if(work.IsAssistant(player.OwnerClientId))Step("Manifest","Stage collected batch","Leave the batch at yard intake for the sanitation lead.");else Step(held.inspected?(held.material=="paper"?"Paper chute":held.material=="metal"?"Metal chute":"Mixed chute"):"Mixed chute",held.inspected?"Sort "+held.material:"Inspect collected waste",held.inspected?"Choose the marked chute for the observed material. You can load the cart instead.":"R inspects the carried batch at the yard. E at the cart loads it for a combined trip.");}
            else if(work.IsAssistant(player.OwnerClientId)&&job==Profession.Mechanic){if(s.hood&&!s.loadChecked&&!s.installed)Step("Probes","Help test battery","R records the load reading. The lead handles the clamp and replacement.");else Step("Garage reset","Reset tool tray","Return the tools to their tray while the lead completes the repair.");}
            else if(job==Profession.Mechanic){
                if(!s.hood&&!s.repairTested)Step("Hood","Open hood","Lift the marked latch at the front of bay 01.");
                else if(!s.diagnosed)Step("Probes","Check battery","E reads voltage. R checks under load and leaves a more detailed reading.");
                else if(!s.dispatched)Step("Request","Await town supply","Solo: the labelled cabinet receives the same parcel after 12 seconds. In multiplayer, ask the shopkeeper or use R self-service after 25 seconds.");
                else if(!s.released)Step("Terminal","Loosen battery clamp","Hold left mouse and drag right. R selects slower housing recovery for cleaner scrap.");
                else if(!s.removed)Step("Lift battery","Lift old battery","The clamp is released. Lift the old battery by its handle.");
                else if(!s.oldPlaced)Step("Scrap tray","Place old battery","Use the scrap tray, leaving your hands free.");
                else if(!s.installed)Step("Replacement","Grab replacement","Open the received carton on the bench. Its packaging becomes collection work.");
                else if(!s.repairTested)Step("Repair test","Test repair","Connect both posts. A successful repair reads 12.6 volts.");
                else if(s.hood)Step("Hood","Close hood","Lower the bonnet after the voltage check.");
                else if(!s.serviceSigned)Step("Service ticket","Sign service ticket","The clipboard records the fitted and tested part, not every earlier handler.");
                else Step("Shift report","Await collection",NetworkSession.Instance&&NetworkSession.Instance.Solo?"The unattended yard service is finishing the waste tickets.":"Your sanitation colleague files the waste tickets. You can help carry loads.");
            }else if(job==Profession.Shopkeeper){
                if(!s.received)Step("Shipment","Receive shipment","Open the shipping carton on the rear delivery desk.");
                else if(!s.prepared&&s.priority=="garage")Step("Dispatch","Pack garage order","R verifies the contents; E packs an unchecked dispatch. R at the register changes priority.");
                else if(work.Domain.Find("PART-8A-001").custody=="dispatch")Step("Parcel","Grab garage parcel","Carry it through the market doorway to the garage bench.");
                else if(s.customerOrders<3&&!work.IsAssistant(player.OwnerClientId)){var goods=s.items[4+s.customerOrders];int index=Array.IndexOf(s.items,goods)-4;if(goods.custody=="checkout")Step("Order register","Scan customer order","Scan the placed product; a partial receipt records the sale.");else if((goods.custody=="stockroom"||goods.custody=="unobserved"))Step("Stock box "+index,"Lift "+goods.sku+" stock","The printed box label matches a shelf and the current customer order.");else if(goods.custody=="shelf")Step("Shelf product "+index,"Collect "+goods.sku,"Lift the requested product from its stocked shelf.");else Step("Order counter","Order in progress","A colleague has this stock. Continue garage dispatch while it is handled.");}
                else if(!s.prepared)Step("Dispatch","Pack garage order","The garage needs its parcel. Stock verification is optional.");
                else Step("Shift report","Await town services","The unattended jobs continue in solo. In multiplayer, help with deliveries.");
            }
            else if(job==Profession.Sanitation){
                if(s.items.Any(i=>i.custody=="sorted"))Step("Manifest","File collection ticket","Stamp the batch at the yard desk.");
                else if(s.items.Any(i=>i.custody=="yard"))Step("Yard intake","Lift staged batch","A colleague has left a collection load for you.");
                else if(s.items.Count(i=>i.custody=="cart")>=3||s.items.Any(i=>i.custody=="cart")&&!s.items.Any(i=>i.custody=="square"||i.custody=="garage-waste")){
                    if(Vector3.Distance(Cart.transform.position,new Vector3(8,0,-7))>2.8f)Step("Manifest","Take cart to yard","E at the handle starts pushing. WASD moves; E or Q parks.");else{var batch=s.items.First(i=>i.custody=="cart");Step("Cart batch "+Array.IndexOf(s.items,batch),"Lift a cart batch","Aim at a visible batch inside the cart and lift it for sorting.");}}
                else if(work.Domain.Find("BAG-8A-001").custody=="square")Step("Square pickup","Collect square bag","Carry one load directly, or collect several using the cart.");
                else if(work.Domain.Find("PACK-8A-001").custody=="garage-waste")Step("Paper pickup","Collect packaging","The garage leaves its opened carton by the scrap tray.");
                else if(work.Domain.Find("OLD-8A-001").custody=="garage-waste")Step("Metal pickup","Collect used battery","Repair choices affect the battery's treatment.");
                else Step("Manifest","Await garage waste",NetworkSession.Instance&&NetworkSession.Instance.Solo?"The unattended garage service repairs the actual vehicle in solo mode.":"Your mechanic produces the packaging and old battery during the repair.");
            }
            else {g.note="Choose a profession";g.detail="J opens the staff desk.";}
            if(s.serviceSigned&&s.customerOrders>=1&&s.items.Count(i=>i.custody=="recorded")>=3)Step("Shift report","File town report","All three jobs are filed. Sign off at the square bench.");
            if(next!=null){Interactable target=work.Stations[next];bool inside=player.transform.position.x<-25,market=target.transform.position.x<-25;if(market&&!inside)target=shop.Targets.Values.OfType<TownStation>().FirstOrDefault(t=>t.kind==TownStation.Kind.Portal);else if(!market&&inside)target=shop.Targets.Values.OfType<ShopStation>().FirstOrDefault(t=>t.kind==ShopStation.Kind.Exit);if(market&&!inside){var door=shop.Targets.Values.OfType<SimpleDoor>().FirstOrDefault(d=>Vector3.Distance(d.transform.position,new Vector3(-8.2f,1,3.4f))<2);if(door&&!door.IsOpen)target=door;}if(target)g.objectives.Add(new TownObjective{id=NetworkShop.Id(target),title=target is WorkdayStation a?a.label:market?"Enter market":"Exit market",icon=TownGuidance.RoleIcon(job),target=target});}
            return g;
        }
    }
}

