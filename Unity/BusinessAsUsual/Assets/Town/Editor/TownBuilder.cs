using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Town.Editor
{
    public static class TownBuilder
    {
        public const string ScenePath="Assets/Town/Scenes/DaylightTown.unity",PrefabPath="Assets/Town/Prefabs/SunnyTownPlayer.prefab";
        [MenuItem("Business as Usual/Phase 6/Create Multi-Job Town")]
        public static void Create()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Require stopped, saved editor.");
            if(File.Exists(ScenePath))throw new InvalidOperationException("Phase 6 scene exists; refusing to regenerate over edits.");
            foreach(var folder in new[]{"Scenes","Prefabs","Materials","Art","Working"})Directory.CreateDirectory("Assets/Town/"+folder);
            AssetDatabase.Refresh();
            var town=EditorSceneManager.OpenScene("Assets/Prototype/Scenes/TownPrototype.unity");EditorSceneManager.SaveScene(town,ScenePath,true);town=EditorSceneManager.OpenScene(ScenePath);
            var original=UnityEngine.Object.FindFirstObjectByType<ThirdPersonMotor>().gameObject;original.SetActive(false);
            var delivery=UnityEngine.Object.FindFirstObjectByType<CarryableBox>();if(delivery)UnityEngine.Object.DestroyImmediate(delivery.gameObject);
            foreach(var workstation in UnityEngine.Object.FindObjectsByType<BasicWorkstation>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(workstation);
            var source=EditorSceneManager.OpenScene("Assets/Multiplayer/Scenes/ShopkeeperMultiplayer.unity",OpenSceneMode.Additive);
            var interior=new GameObject("Daylight Market Interior").transform;SceneManager.MoveGameObjectToScene(interior.gameObject,town);
            foreach(var root in source.GetRootGameObjects().Where(r=>r.name=="Functional Fixtures"||r.name=="Shop Shift"||r.name=="Original Retail Environment")){SceneManager.MoveGameObjectToScene(root,town);root.transform.SetParent(interior,true);}
            interior.position=new Vector3(-40,0,0);
            var sessionRoot=source.GetRootGameObjects().Single(r=>r.name=="LAN Network Session");SceneManager.MoveGameObjectToScene(sessionRoot,town);
            var session=sessionRoot.GetComponent<NetworkSession>();var shop=interior.GetComponentInChildren<NetworkShop>();shop.gameObject.AddComponent<TownJobs>();
            var prefab=UnityEngine.Object.Instantiate(session.Manager.NetworkConfig.PlayerPrefab);prefab.name="SunnyTownPlayer";prefab.AddComponent<TownHandbook>().enabled=false;PrefabUtility.SaveAsPrefabAsset(prefab,PrefabPath);UnityEngine.Object.DestroyImmediate(prefab);session.Manager.NetworkConfig.PlayerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            SetupNetworkPrefabs(session.Manager);
            var preview=source.GetRootGameObjects().Single(r=>r.name=="Connection Desk Preview Camera");SceneManager.MoveGameObjectToScene(preview,town);preview.transform.SetPositionAndRotation(new Vector3(0,2.2f,-6),Quaternion.Euler(12,0,0));session.PreviewCamera=preview.GetComponent<Camera>();
            EditorSceneManager.CloseScene(source,true);
            var gameplay=new GameObject("Town Workplaces").transform;
            Station("Enter Daylight Market",new Vector3(-7.8f,0,4.2f),TownStation.Kind.Portal,gameplay,"Enter Daylight Market").destination=new Vector3(-40,.08f,3.2f);
            Station("Service Desk",new Vector3(8,0,8.1f),TownStation.Kind.ServiceDesk,gameplay,"Receive tickets / file records / finish shift");
            Station("Parts Request Slip",new Vector3(10.3f,0,8.2f),TownStation.Kind.RequestParts,gameplay,"File replacement parts request");
            for(int i=0;i<2;i++){
                float x=i==0?6.4f:9.6f;
                var body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.name="Service Vehicle "+i;body.transform.position=new Vector3(x,.6f,6.3f);body.transform.localScale=new Vector3(1.4f,1.2f,2.4f);body.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Materials/Graybox_Garage.mat");body.transform.SetParent(gameplay);
                Station("Vehicle "+i+" Inspection",new Vector3(i==0?5.35f:10.65f,0,5.5f),TownStation.Kind.Inspect,gameplay,"Hold E / inspect vehicle "+(i+1),i);
                Station("Vehicle "+i+" Repair",new Vector3(x,0,4.75f),TownStation.Kind.Repair,gameplay,"Hold E / repair vehicle "+(i+1),i);
            }
            Station("Parts Packing Counter",new Vector3(-35.9f,0,4.8f),TownStation.Kind.PackParts,gameplay,"Pack mechanic replacement parts");
            Station("Parts Pickup Locker",new Vector3(-10.8f,0,2.75f),TownStation.Kind.CollectParts,gameplay,"Collect parts / municipal reserve");
            Station("Collection Route Desk",new Vector3(10,0,-6),TownStation.Kind.RouteDesk,gameplay,"Receive route / file manifest / finish shift");
            string[] types={"Paper","Mixed","Metal"};for(int i=0;i<3;i++){var bin=Station(types[i]+" Sorting Bin",new Vector3(6+i*1.5f,0,-8.5f),TownStation.Kind.Sort,gameplay,"Hold E / sort "+types[i]);bin.materialType=types[i];}
            Vector3[] bags={new Vector3(-1,.3f,7.8f),new Vector3(0,.3f,11),new Vector3(5,.3f,-6.8f),new Vector3(-10,.3f,2.9f),new Vector3(5.15f,.3f,8.55f),new Vector3(10.85f,.3f,8.55f)};
            for(int i=0;i<bags.Length;i++){var prop=GameObject.CreatePrimitive(PrimitiveType.Sphere);prop.name="Route Bag "+i;prop.transform.position=bags[i];prop.transform.localScale=new Vector3(.5f,.6f,.5f);prop.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Materials/Graybox_Dark.mat");prop.AddComponent<WasteBag>().index=i;prop.transform.SetParent(gameplay,true);}
            EditorSceneManager.SaveScene(town);AssetDatabase.SaveAssets();Debug.Log("Phase6 functional town created; source town and shop unchanged.");
        }
        public static TownStation Station(string name,Vector3 position,TownStation.Kind kind,Transform parent,string label,int index=0)
        {
            string existing=name=="Service Desk"?"Garage Workstation":name=="Collection Route Desk"?"Sorting Workstation":name=="Paper Sorting Bin"?"Waste Bin 1":name=="Mixed Sorting Bin"?"Waste Bin 2":name=="Metal Sorting Bin"?"Waste Bin 3":null;
            if(existing!=null){var fixture=GameObject.Find(existing);if(fixture){fixture.name=name;var bound=fixture.AddComponent<TownStation>();bound.kind=kind;bound.label=label;bound.index=index;return bound;}}
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;var trigger=go.AddComponent<BoxCollider>();trigger.center=Vector3.up*.65f;trigger.size=new Vector3(.5f,1.1f,.5f);trigger.isTrigger=true;
            var station=go.AddComponent<TownStation>();station.kind=kind;station.label=label;station.index=index;return station;
        }
        public static void SetupNetworkPrefabs(NetworkManager manager)
        {
            const string path="Assets/Town/Prefabs/TownNetworkPrefabs.asset";
            var list=AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(path);
            if(!list){list=ScriptableObject.CreateInstance<NetworkPrefabsList>();AssetDatabase.CreateAsset(list,path);}
            if(!list.Contains(manager.NetworkConfig.PlayerPrefab))list.Add(new NetworkPrefab{Prefab=manager.NetworkConfig.PlayerPrefab});
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists=new System.Collections.Generic.List<NetworkPrefabsList>{list};EditorUtility.SetDirty(list);EditorUtility.SetDirty(manager);
        }
        [MenuItem("Business as Usual/Phase 6/Build Windows Town")]
        public static void Build()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/Phase6/BusinessAsUsual.exe"));Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath,"Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});Debug.Log("Phase6 build: "+report.summary.result+" errors="+report.summary.totalErrors);
        }
    }
}
