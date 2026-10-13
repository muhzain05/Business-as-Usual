using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    public static class ShopArtBuilder
    {
        static readonly Dictionary<string,Material> Materials=new Dictionary<string,Material>();
        static readonly string[] Brands={"Sunbeam","Meadow","Golden","Cloud","Orchard","Tide","LittleFox","Garden"};
        static Transform art;
        [MenuItem("Business as Usual/Phase 4/Improve Shop Environment")]
        public static void Improve()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first.");
            if(SceneManager.GetActiveScene().path!=ShopBuilder.ScenePath)throw new InvalidOperationException("Open ShopkeeperShift first.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SetupMaterials(); BuildPrefabs();
            var old=GameObject.Find("Original Retail Environment");if(old)UnityEngine.Object.DestroyImmediate(old);
            art=new GameObject("Original Retail Environment").transform;
            var stations=UnityEngine.Object.FindObjectsByType<ShopStation>(FindObjectsSortMode.None);
            foreach(var st in stations)
            {
                st.GetComponent<Renderer>().enabled=false;
                foreach(var t in st.GetComponentsInChildren<TextMesh>())UnityEngine.Object.DestroyImmediate(t.gameObject);
                if(st.kind==ShopStation.Kind.Shelf)
                {
                    var shelfCollision=st.GetComponent<BoxCollider>();shelfCollision.size=new Vector3(1,1.3077f,1);shelfCollision.center=new Vector3(0,.15385f,0);
                    Instance("Shelf",new Vector3(st.transform.position.x,0,st.transform.position.z));
                    Populate(new Vector3(st.transform.position.x,0,st.transform.position.z),false,Array.IndexOf(ShopShift.Products,ShopShift.Products.First(p=>p.sku==st.sku)),true);
                    if(st.sku!="tea")
                    {
                        Instance("Shelf",new Vector3(st.transform.position.x,0,st.transform.position.z+.65f),180);
                        Populate(new Vector3(st.transform.position.x,0,st.transform.position.z+.65f),false,st.sku=="cola"?4:6,false,180);
                        var c=st.GetComponent<BoxCollider>();c.size=new Vector3(1,1.3077f,1.8125f);c.center=new Vector3(0,.15385f,.40625f);
                    }
                    Sign(ShopShift.Products.First(p=>p.sku==st.sku).name.ToUpper(),new Vector3(st.transform.position.x,1.65f,st.transform.position.z+.205f),new Vector2(1.55f,.15f),Materials["Teal"],Color.white,.036f);
                    var display=st.gameObject.GetComponent<ShopStockDisplay>() ?? st.gameObject.AddComponent<ShopStockDisplay>();display.shift=st.shift;display.sku=st.sku;
                    display.reach=UnityEngine.Object.FindFirstObjectByType<ShopRestockReach>();
                    display.units=Enumerable.Range(0,6).Select(i=>Instance("Product_"+Brands[st.sku=="cola"?0:st.sku=="tea"?1:2],new Vector3(st.transform.position.x-.55f+i*.22f,.635f,st.transform.position.z-.2f))).ToArray();
                    Sign("SHIFT STOCK • "+(st.sku=="cola"?"$1.80":st.sku=="tea"?"$2.20":"$1.50"),new Vector3(st.transform.position.x,.62f,st.transform.position.z-.357f),new Vector2(1.5f,.065f),Materials["Paper"],new Color(.1f,.25f,.22f),.018f);
                }
                else if(st.kind==ShopStation.Kind.Checkout)
                {
                    Instance("Counter",new Vector3(-1,0,1.8f));
                    Instance("Register",new Vector3(-.3f,1.07f,1.8f),180);
                    Instance("Scanner",new Vector3(-1.05f,1.07f,1.65f));
                    Instance("Basket",new Vector3(-2,1.07f,1.8f));
                    Sign("DAYLIGHT MARKET",new Vector3(-1,.64f,1.36f),new Vector2(2.5f,.23f),Materials["Teal"],Color.white,.068f);
                    Sign("CHECKOUT / E",new Vector3(-1,1.08f,1.3f),new Vector2(.8f,.08f),Materials["Paper"],Color.black,.022f);
                    Sign("$0.00",new Vector3(-.43f,1.55f,1.845f),new Vector2(.22f,.06f),Materials["Screen"],new Color(.8f,1,.5f),.017f);
                    var till=st.GetComponent<ShopTillDisplay>()??st.gameObject.AddComponent<ShopTillDisplay>();till.shift=st.shift;till.display=art.GetComponentsInChildren<TextMesh>().Last();
                    var cable=new GameObject("Scanner cable");cable.transform.SetParent(art);var line=cable.AddComponent<LineRenderer>();line.sharedMaterial=Materials["Dark"];line.startWidth=.012f;line.endWidth=.012f;line.positionCount=24;
                    for(int i=0;i<24;i++){float t=i/23f;line.SetPosition(i,new Vector3(-1.05f+t*.7f,1.09f,1.85f+Mathf.Sin(t*3.14f)*.17f));}
                }
                else if(st.kind==ShopStation.Kind.Close)
                {
                    var counter=Instance("Counter",new Vector3(3.8f,0,1.8f));counter.transform.localScale=new Vector3(.35f,1,1);
                    Instance("Register",new Vector3(3.8f,1.07f,1.8f),180);
                    Sign("SHIFT REVIEW / E",new Vector3(3.8f,.65f,1.36f),new Vector2(1.04f,.2f),Materials["Teal"],Color.white,.027f);
                }
                else if(st.kind==ShopStation.Kind.Delivery)
                {
                    var counter=Instance("Counter",new Vector3(4,0,10.3f));counter.transform.localScale=new Vector3(.35f,1,1);
                    Sign("RECEIVING / E",new Vector3(4,1.4f,10.32f),new Vector2(1.1f,.2f),Materials["Teal"],Color.white,.03f);
                    for(int i=0;i<3;i++)Instance("Crate",new Vector3(4.8f,0,11.2f-i*.52f));
                }
                else Sign("EXIT TO TOWN / E AFTER SHIFT",new Vector3(0,2.4f,-.25f),new Vector2(2.6f,.22f),Materials["Teal"],Color.white,.04f,180);
            }
            var crate=UnityEngine.Object.FindFirstObjectByType<StockCrate>();crate.GetComponent<Renderer>().enabled=false;
            foreach(Transform child in crate.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            var crateArt=Instance("Crate",new Vector3(4,.07f,8.2f));crateArt.transform.SetParent(crate.transform,true);
            // Side wall assortment and back cold wall; generous working aisles remain clear.
            foreach(float z in new[]{4.5f,7,9.5f}) {Instance("Shelf",new Vector3(-4.9f,0,z),-90);Populate(new Vector3(-4.9f,0,z),false,(int)z%8,false,-90);FixtureCollider(new Vector3(-4.9f,.8f,z),new Vector3(.65f,1.6f,1.8f));}
            foreach(float x in new[]{-3.8f,-1.9f}) {Instance("Fridge",new Vector3(x,0,11.7f));Populate(new Vector3(x,0,11.7f),true,x< -3?0:3,false);FixtureCollider(new Vector3(x,1,11.7f),new Vector3(1.8f,2,.7f));Sign(x< -3?"CHILLED DRINKS":"MILK & PANTRY",new Vector3(x,2.05f,11.275f),new Vector2(1.56f,.13f),Materials["Teal"],Color.white,.033f);PointLight(new Vector3(x,1.85f,11.3f),new Color(1,.94f,.78f),.3f,1.6f);}
            // Distinct sale endcap and impulse products at the cashier.
            Instance("Shelf",new Vector3(4.6f,0,5.2f),90);Populate(new Vector3(4.6f,0,5.2f),false,4,false,90);FixtureCollider(new Vector3(4.6f,.8f,5.2f),new Vector3(.65f,1.6f,1.8f));
            Sign("WEEKEND PICKS",new Vector3(2.25f,2.15f,12.25f),new Vector2(2.3f,.3f),Materials["Terracotta"],Color.white,.065f);
            Sign("DAYLIGHT\nYOUR NEIGHBORHOOD MARKET",new Vector3(0,2.85f,12.25f),new Vector2(3.4f,.55f),Materials["Teal"],Color.white,.075f);
            for(int i=0;i<5;i++)Instance("Product_Golden",new Vector3(-1.65f+i*.18f,1.07f,2.08f));
            Instance("Basket",new Vector3(4.65f,0,2.1f));Instance("Basket",new Vector3(4.65f,.06f,2.1f));
            // Architecture trim, windows and practical ceiling fixtures.
            var floor=GameObject.Find("Floor").GetComponent<Renderer>();var oak=ShopBuilder.Mat("OakFloor",Color.white);oak.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(ShopBuilder.Root+"Textures/OakFloor.png");oak.mainTextureScale=new Vector2(8,10);oak.SetFloat("_Smoothness",.18f);floor.sharedMaterial=oak;
            foreach(float x in new[]{-5.27f,5.27f}) {DetailBox("terracotta wall stripe",new Vector3(x,2.67f,6),new Vector3(.025f,.14f,12.4f),"Terracotta");DetailBox("teal pinstripe",new Vector3(x,2.52f,6),new Vector3(.025f,.055f,12.4f),"Teal");DetailBox("skirting",new Vector3(x,.12f,6),new Vector3(.04f,.24f,12.4f),"Teal");}
            foreach(float x in new[]{-3.4f,3.4f}) {
                DetailBox("window dark frame",new Vector3(x,1.75f,-.23f),new Vector3(3.2f,1.5f,.06f),"Teal");DetailBox("evening glass",new Vector3(x,1.75f,-.19f),new Vector3(3.04f,1.34f,.01f),"Glass");
                for(int i=0;i<3;i++)DetailBox("window mullion",new Vector3(x-1+i,1.75f,-.16f),new Vector3(.025f,1.4f,.03f),"Cream");
            }
            foreach(float x in new[]{-2.5f,2.5f})foreach(float z in new[]{3.5f,7.5f,11f}) {DetailBox("ceiling light housing",new Vector3(x,3.04f,z),new Vector3(1.6f,.08f,.25f),"Teal");DetailBox("warm diffuser",new Vector3(x,2.99f,z),new Vector3(1.5f,.035f,.2f),"Paper");PointLight(new Vector3(x,2.65f,z),new Color(1,.83f,.6f),2.2f,5);}
            var ceiling=DetailBox("Warm plaster ceiling",new Vector3(0,3.18f,6),new Vector3(10.8f,.08f,12.8f),"Cream");ceiling.layer=30;ceiling.AddComponent<BoxCollider>();
            for(int i=0;i<6;i++){var seam=DetailBox("Ceiling panel seam",new Vector3(0,3.132f,i*2.1f-.3f),new Vector3(10.6f,.006f,.012f),"Metal");seam.layer=30;}
            // Plants use purpose-made leaf meshes rather than cube foliage.
            Plant(new Vector3(-4.5f,0,1));Plant(new Vector3(4.6f,0,11.8f));
            RenderSettings.ambientLight=new Color(.51f,.47f,.39f);
            var sun=GameObject.Find("Warm daylight").GetComponent<Light>();sun.intensity=.6f;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("Original retail environment assembled; gameplay station components preserved.");
        }
        static void SetupMaterials()
        {
            var colors=new Dictionary<string,Color>{{"Cream",new Color(.89f,.86f,.73f)},{"Terracotta",new Color(.7f,.26f,.14f)},{"Teal",new Color(.12f,.38f,.34f)},{"Metal",new Color(.5f,.55f,.54f)},{"Dark",new Color(.08f,.1f,.09f)},{"Screen",new Color(.05f,.26f,.19f)},{"Paper",new Color(.98f,.93f,.77f)},{"Blue",new Color(.22f,.38f,.62f)},{"Glass",new Color(.39f,.57f,.54f)},{"Cardboard",new Color(.58f,.39f,.22f)}};
            foreach(var pair in colors)Materials[pair.Key]=ShopBuilder.Mat("Shop_"+pair.Key,pair.Value);
            foreach(var brand in Brands){var m=ShopBuilder.Mat("Shop_"+brand,Color.white);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(ShopBuilder.Root+"Textures/"+brand+".png");Materials[brand]=m;}
            Materials["Metal"].SetFloat("_Metallic",.55f);Materials["Metal"].SetFloat("_Smoothness",.5f);
            Materials["Screen"].EnableKeyword("_EMISSION");Materials["Screen"].SetColor("_EmissionColor",new Color(.04f,.4f,.2f));
            Materials["Paper"].EnableKeyword("_EMISSION");Materials["Paper"].SetColor("_EmissionColor",new Color(.13f,.11f,.07f));
            foreach(var m in Materials.Values)m.enableInstancing=true;
        }
        static void BuildPrefabs()
        {
            foreach(var file in Directory.GetFiles(ShopBuilder.Root+"Art","*.fbx"))
            {
                string name=Path.GetFileNameWithoutExtension(file);var model=AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace('\\','/'));if(!model)throw new InvalidOperationException("Model unavailable: "+file);
                var wrapper=new GameObject(name);var obj=(GameObject)PrefabUtility.InstantiatePrefab(model);obj.transform.SetParent(wrapper.transform,false);
                if(name=="Shelf" || name=="Fridge" || name=="Counter")obj.transform.localRotation=Quaternion.Euler(0,180,0)*obj.transform.localRotation;
                foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Materials[m.name.Replace("Shop_","").Replace(" (Instance)","")]).ToArray();
                PrefabUtility.SaveAsPrefabAsset(wrapper,ShopBuilder.Root+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(wrapper);
            }
        }
        static GameObject Instance(string name,Vector3 pos,float yaw=0)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ShopBuilder.Root+"Prefabs/"+name+".prefab");var o=(GameObject)PrefabUtility.InstantiatePrefab(prefab);o.transform.SetParent(art);o.transform.position=pos;o.transform.rotation=Quaternion.Euler(0,yaw,0);return o;
        }
        static void Populate(Vector3 origin,bool fridge,int variant,bool reserved,float yaw=0)
        {
            var rotation=Quaternion.Euler(0,yaw,0);
            for(int level=0;level<4;level++)for(int row=0;row<2;row++)for(int column=0;column<10;column++)
            {
                if(reserved&&level==1&&row==0)continue;
                int brand=(variant+level+(column/5))%8;
                var p=new Vector3(-.72f+column*.16f,(fridge?.337f:.287f)+level*(fridge?.4f:.35f),-.2f+row*.23f);
                Instance("Product_"+Brands[brand],origin+rotation*p,yaw+180);
            }
            for(int level=0;level<4;level++)for(int column=0;column<4;column++)
            {
                var p=origin+rotation*new Vector3(-.62f+column*.415f,(fridge?.34f:.28f)+level*(fridge?.4f:.35f),-.366f);
                Sign("$"+(1.5f+((variant+level)%4)*.2f).ToString("F2"),p,new Vector2(.15f,.045f),Materials["Paper"],Color.black,.012f,yaw);
            }
        }
        static void Sign(string text,Vector3 pos,Vector2 size,Material material,Color ink,float fontSize,float yaw=0)
        {
            var board=DetailBox("sign backing",pos,new Vector3(size.x,size.y,.015f),material);board.transform.rotation=Quaternion.Euler(0,yaw,0);
            var go=new GameObject(text);go.transform.SetParent(art);go.transform.position=pos+Quaternion.Euler(0,yaw,0)*new Vector3(0,0,-.012f);go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=.1f;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=ink;
            var labelMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Shopkeeper/Materials/ShelfText.mat");
            if(labelMaterial)go.GetComponent<Renderer>().sharedMaterial=labelMaterial;
            var bounds=go.GetComponent<Renderer>().bounds;float fit=Mathf.Min(size.x*.92f/Mathf.Max(.001f,bounds.size.x),size.y*.8f/Mathf.Max(.001f,bounds.size.y));go.transform.localScale=Vector3.one*fit;
        }
        static GameObject DetailBox(string name,Vector3 pos,Vector3 size,string material)=>DetailBox(name,pos,size,Materials[material]);
        static GameObject DetailBox(string name,Vector3 pos,Vector3 size,Material material)
        {
            var o=ShopBuilder.Box(name,pos,size,material,art);UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;
        }
        static void FixtureCollider(Vector3 pos,Vector3 size){var o=new GameObject("Fixture collision");o.transform.SetParent(art);o.transform.position=pos;var c=o.AddComponent<BoxCollider>();c.size=size;}
        static void PointLight(Vector3 pos,Color color,float intensity,float range){var o=new GameObject("Warm retail light");o.transform.SetParent(art);o.transform.position=pos;var l=o.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;}
        static void Plant(Vector3 pos)
        {
            var pot=GameObject.CreatePrimitive(PrimitiveType.Cylinder);pot.name="Ceramic planter";pot.transform.SetParent(art);pot.transform.position=pos+Vector3.up*.18f;pot.transform.localScale=new Vector3(.35f,.18f,.35f);pot.GetComponent<Renderer>().sharedMaterial=Materials["Terracotta"];UnityEngine.Object.DestroyImmediate(pot.GetComponent<Collider>());
            for(int i=0;i<9;i++) {float a=i*Mathf.PI*2/9;var mesh=new Mesh();mesh.vertices=new[]{new Vector3(0,.25f,0),new Vector3(-.08f,.65f,.1f),new Vector3(0,.85f,.25f),new Vector3(.08f,.65f,.1f)};mesh.triangles=new[]{0,1,2,0,2,3,2,1,0,3,2,0};mesh.RecalculateNormals();string path=ShopBuilder.Root+"Art/PlantLeaf"+i+".asset";if(!AssetDatabase.LoadAssetAtPath<Mesh>(path))AssetDatabase.CreateAsset(mesh,path);else{UnityEngine.Object.DestroyImmediate(mesh);mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);}var leaf=new GameObject("Planter leaf");leaf.transform.SetParent(art);leaf.transform.position=pos;leaf.transform.rotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);leaf.AddComponent<MeshFilter>().sharedMesh=mesh;leaf.AddComponent<MeshRenderer>().sharedMaterial=Materials["Teal"];}
        }
    }
}
