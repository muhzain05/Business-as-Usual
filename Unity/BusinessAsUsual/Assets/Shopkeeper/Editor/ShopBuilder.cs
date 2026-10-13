using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    public static class ShopBuilder
    {
        public const string ScenePath="Assets/Shopkeeper/Scenes/ShopkeeperShift.unity";
        public const string Root="Assets/Shopkeeper/";
        [MenuItem("Business as Usual/Phase 4/Create Shopkeeper Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Stop Play and save current scene first.");
            if(File.Exists(ScenePath)) throw new InvalidOperationException("Shop scene already exists. Open it or use visual improvement command.");
            foreach(var f in new[]{"Scenes","Materials","Prefabs","Art","Textures"}) Directory.CreateDirectory(Root+f);
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cream=Mat("Cream",new Color(.88f,.82f,.65f)); var teal=Mat("Teal",new Color(.12f,.37f,.36f)); var oak=Mat("Oak",new Color(.42f,.24f,.12f));
            var fixtures=new GameObject("Functional Fixtures").transform;
            Box("Floor",new Vector3(0,-.15f,6),new Vector3(10.8f,.3f,12.8f),oak,fixtures);
            Box("West wall",new Vector3(-5.4f,1.6f,6),new Vector3(.2f,3.2f,12.8f),cream,fixtures);
            Box("East wall",new Vector3(5.4f,1.6f,6),new Vector3(.2f,3.2f,12.8f),cream,fixtures);
            Box("Back wall",new Vector3(0,1.6f,12.4f),new Vector3(10.8f,3.2f,.2f),cream,fixtures);
            Box("Front west",new Vector3(-3.45f,1.6f,-.4f),new Vector3(3.9f,3.2f,.2f),cream,fixtures);
            Box("Front east",new Vector3(3.45f,1.6f,-.4f),new Vector3(3.9f,3.2f,.2f),cream,fixtures);
            var shift=new GameObject("Shop Shift").AddComponent<ShopShift>();
            Station("Delivery Desk",new Vector3(4,.65f,10.3f),ShopStation.Kind.Delivery,shift,teal,fixtures,new Vector3(1.2f,1.3f,.7f));
            Station("Sunbeam Cola Shelf",new Vector3(-2,.65f,6),ShopStation.Kind.Shelf,shift,cream,fixtures,new Vector3(1.8f,1.3f,.8f),"cola");
            Station("Meadow Tea Shelf",new Vector3(0,.65f,9.7f),ShopStation.Kind.Shelf,shift,cream,fixtures,new Vector3(1.8f,1.3f,.8f),"tea");
            Station("Golden Crackers Shelf",new Vector3(2,.65f,6),ShopStation.Kind.Shelf,shift,cream,fixtures,new Vector3(1.8f,1.3f,.8f),"crackers");
            Station("Checkout",new Vector3(-1,.55f,1.8f),ShopStation.Kind.Checkout,shift,teal,fixtures,new Vector3(3.4f,1.1f,.85f));
            Station("Closing Terminal",new Vector3(3.8f,.55f,1.8f),ShopStation.Kind.Close,shift,teal,fixtures,new Vector3(1.2f,1.1f,.85f));
            var exit=Station("Town Exit",new Vector3(0,.65f,-.1f),ShopStation.Kind.Exit,shift,cream,fixtures,new Vector3(1.2f,1.3f,.1f)); exit.GetComponent<BoxCollider>().isTrigger=true; exit.GetComponent<Renderer>().enabled=false;
            var crate=Box("Stock Crate",new Vector3(4,.3f,8.2f),new Vector3(.55f,.6f,.45f),oak,fixtures).AddComponent<StockCrate>();crate.shift=shift;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Prefabs/SunnyPlayer.prefab");
            var player=(GameObject)PrefabUtility.InstantiatePrefab(prefab);player.transform.position=new Vector3(0,.05f,3.2f);
            var motor=player.GetComponent<ThirdPersonMotor>(); motor.respawnPosition=player.transform.position;
            player.GetComponent<PrototypeHUD>().enabled=false;
            var hud=player.AddComponent<ShopHUD>();hud.shift=shift;hud.player=player.GetComponent<PlayerInteractor>();
            player.AddComponent<ShopRestockReach>();
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=new Color(.65f,.58f,.45f);
            var sun=new GameObject("Warm daylight").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.89f,.7f);sun.intensity=1.1f;sun.transform.rotation=Quaternion.Euler(55,-30,0);sun.shadows=LightShadows.Soft;
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(ScenePath,true)}).GroupBy(s=>s.path).Select(g=>g.First()).ToArray();
            AssetDatabase.SaveAssets();Debug.Log("Shopkeeper scene created.");
        }
        public static Material Mat(string name,Color color)
        {
            string path=Root+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){ m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path); }
            m.color=color;m.SetFloat("_Smoothness",.25f);return m;
        }
        public static GameObject Box(string name,Vector3 p,Vector3 size,Material mat,Transform parent)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent);o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;return o;
        }
        static GameObject Station(string name,Vector3 p,ShopStation.Kind kind,ShopShift shift,Material mat,Transform parent,Vector3 size,string sku=null)
        {
            var o=Box(name,p,size,mat,parent);var station=o.AddComponent<ShopStation>();station.kind=kind;station.shift=shift;station.sku=sku;
            Label(name,new Vector3(p.x,p.y+size.y*.5f+.18f,p.z-size.z*.5f-.02f),o.transform,.08f);
            return o;
        }
        public static GameObject Label(string text,Vector3 pos,Transform parent,float size=.12f)
        {
            var o=new GameObject(text);o.transform.SetParent(parent,true);o.transform.position=pos;o.transform.rotation=Quaternion.Euler(0,180,0);var t=o.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.12f,.22f,.22f);return o;
        }
    }
}
