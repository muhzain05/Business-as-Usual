using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Multiplayer.Editor
{
    public static class NetworkBuilder
    {
        public const string ScenePath="Assets/Multiplayer/Scenes/ShopkeeperMultiplayer.unity";
        public const string PrefabPath="Assets/Multiplayer/Prefabs/SunnyNetworkPlayer.prefab";
        [MenuItem("Business as Usual/Phase 5/Create Multiplayer Scene")]
        public static void Create()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Stop Play and preserve unsaved scene work first.");
            Directory.CreateDirectory("Assets/Multiplayer/Scenes"); Directory.CreateDirectory("Assets/Multiplayer/Prefabs");
            var source=EditorSceneManager.OpenScene("Assets/Shopkeeper/Scenes/ShopkeeperShift.unity");
            EditorSceneManager.SaveScene(source,ScenePath,true);
            EditorSceneManager.OpenScene(ScenePath);
            var original=Object.FindFirstObjectByType<ThirdPersonMotor>().gameObject;
            var template=Object.Instantiate(original); template.name="SunnyNetworkPlayer";
            var hud=template.GetComponent<ShopHUD>();hud.shift=null;hud.player=template.GetComponent<PlayerInteractor>();hud.enabled=false;
            template.GetComponent<PrototypeInput>().enabled=false;template.GetComponent<PlayerInteractor>().enabled=false;
            template.GetComponent<ThirdPersonMotor>().enabled=false;template.GetComponent<CharacterController>().enabled=false;
            template.GetComponent<ThirdPersonMotor>().cameraTransform=null;
            foreach(var c in template.GetComponentsInChildren<Camera>(true))c.enabled=false;
            foreach(var c in template.GetComponentsInChildren<AudioListener>(true))c.enabled=false;
            foreach(var c in template.GetComponentsInChildren<ThirdPersonCamera>(true)){c.target=template.transform;c.input=template.GetComponent<PrototypeInput>();c.enabled=false;}
            template.AddComponent<NetworkObject>(); template.AddComponent<NetworkPlayer>();
            var nt=template.AddComponent<NetworkTransform>();nt.Interpolate=true;nt.PositionThreshold=.015f;
            nt.SyncScaleX=nt.SyncScaleY=nt.SyncScaleZ=false;nt.SyncRotAngleX=nt.SyncRotAngleY=nt.SyncRotAngleZ=false;
            var prefab=PrefabUtility.SaveAsPrefabAsset(template,PrefabPath);Object.DestroyImmediate(template);original.SetActive(false);
            foreach(var display in Object.FindObjectsByType<ShopStockDisplay>(FindObjectsSortMode.None))display.reach=null;
            var shift=Object.FindFirstObjectByType<ShopShift>();var shop=shift.gameObject.AddComponent<NetworkShop>();shop.Shift=shift;shop.Crate=Object.FindFirstObjectByType<StockCrate>();shift.gameObject.AddComponent<NetworkObject>();
            var managerGo=new GameObject("LAN Network Session");var transport=managerGo.AddComponent<UnityTransport>();var manager=managerGo.AddComponent<NetworkManager>();
            manager.NetworkConfig.NetworkTransport=transport;manager.NetworkConfig.PlayerPrefab=prefab;manager.NetworkConfig.TickRate=30;
            var session=managerGo.AddComponent<NetworkSession>();session.Manager=manager;managerGo.AddComponent<NetworkTestDriver>();
            var preview=Object.Instantiate(original.GetComponentInChildren<Camera>(true).gameObject);
            preview.name="Connection Desk Preview Camera";
            preview.GetComponent<ThirdPersonCamera>().enabled=false;
            preview.GetComponent<AudioListener>().enabled=false;
            session.PreviewCamera=preview.GetComponent<Camera>();session.PreviewCamera.enabled=true;
            // Reuse Phase 3's existing door/workstation prefabs if present, without rebuilding art.
            var town=EditorSceneManager.OpenScene("Assets/Prototype/Scenes/TownPrototype.unity",OpenSceneMode.Additive);
            var destination=shift.gameObject.scene;
            var door=town.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SimpleDoor>(true)).FirstOrDefault();
            if(door){var copy=Object.Instantiate(door.gameObject);SceneManager.MoveGameObjectToScene(copy,destination);copy.name="Shared Door Test Fixture";copy.transform.position=new Vector3(4.1f,0,3);copy.SetActive(false);}
            var work=town.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<BasicWorkstation>(true)).FirstOrDefault();
            if(work){var copy=Object.Instantiate(work.gameObject);SceneManager.MoveGameObjectToScene(copy,destination);copy.name="Shared Workstation Test Fixture";copy.transform.position=new Vector3(-4.1f,0,3);copy.SetActive(false);}
            EditorSceneManager.CloseScene(town,true);
            EditorSceneManager.SaveScene(destination);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();Debug.Log("Phase5 scene created; original single-player scene unchanged.");
        }
        [MenuItem("Business as Usual/Phase 5/Build Windows Prototype")]
        public static void Build()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/Phase5/BusinessAsUsual.exe"));Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath,"Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            Debug.Log("Phase5 build: "+report.summary.result+" errors="+report.summary.totalErrors);
        }
    }
}
