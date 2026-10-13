using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.RescueV2.Editor;
namespace BusinessAsUsual.Phase8B.Editor {
    public static class Phase8BBuilder {
        const string Root="Assets/Phase8B/";
        [MenuItem("Business as Usual/Phase 8B/Build first suspicious day")]
        public static void Build(){
            if(EditorUtility.scriptCompilationFailed)throw new Exception("Compile the Phase 8B scripts before building.");Directory.CreateDirectory(Root+"Scenes");Directory.CreateDirectory(Root+"Resources/Phase8B");RescueBuilder.PrepareUI();PrepareModels();CreateMenu();
            var path=Path.GetFullPath("../../Builds/Phase8B/BusinessAsUsual.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"Scenes/Phase8BFrontEnd.unity","Assets/Town/Scenes/DaylightTown.unity","Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            string evidence=Path.GetFullPath("../../Docs/Diagnostics/Phase8B/build.json");Directory.CreateDirectory(Path.GetDirectoryName(evidence));File.WriteAllText(evidence,JsonUtility.ToJson(new Evidence{result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,path=path},true));if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build "+report.summary.result);
        }
        static void CreateMenu(){
            var town=EditorSceneManager.OpenScene("Assets/Town/Scenes/DaylightTown.unity",OpenSceneMode.Single);var original=town.GetRootGameObjects();var menu=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(menu);int count=0;
            foreach(var root in original){if(root.GetComponent<NetworkSession>()||root.GetComponent<NetworkShop>()||root.GetComponent<BusinessAsUsual.Town.TownJobs>()||root.GetComponent<Camera>())continue;var renderers=root.GetComponentsInChildren<Renderer>();if(renderers.Length==0)continue;var clone=UnityEngine.Object.Instantiate(root);clone.name="Menu / "+root.name;SceneManager.MoveGameObjectToScene(clone,menu);foreach(var component in clone.GetComponentsInChildren<MonoBehaviour>())UnityEngine.Object.DestroyImmediate(component);foreach(var listener in clone.GetComponentsInChildren<AudioListener>())UnityEngine.Object.DestroyImmediate(listener);foreach(var camera in clone.GetComponentsInChildren<Camera>())UnityEngine.Object.DestroyImmediate(camera);count+=renderers.Length;}
            var main=new GameObject("Neighborhood menu camera",typeof(Camera),typeof(AudioListener));main.transform.position=new Vector3(-1.2f,1.6f,-3.6f);main.transform.rotation=Quaternion.Euler(4,-22,0);main.GetComponent<Camera>().fieldOfView=64;main.GetComponent<Camera>().backgroundColor=new Color(.64f,.81f,.86f);
            var kiosk=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Resources/Phase8B/StaffKiosk.prefab");if(kiosk){var board=(GameObject)PrefabUtility.InstantiatePrefab(kiosk,menu);board.name="Menu staff kiosk";board.transform.position=new Vector3(1.4f,.025f,6.1f);}
            var sun=new GameObject("Warm afternoon sun",typeof(Light));var light=sun.GetComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.91f,.72f);light.intensity=1.1f;light.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-24,0);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.56f,.67f,.63f);
            EditorSceneManager.SaveScene(menu,Root+"Scenes/Phase8BFrontEnd.unity");EditorSceneManager.CloseScene(town,true);Debug.Log("Phase 8B menu uses real neighborhood geometry: "+count+" renderers, one camera/listener.");AssetDatabase.SaveAssets();
        }
        [Serializable]sealed class Evidence{public string result,path;public int errors,warnings;}
        static void PrepareModels(){foreach(string name in new[]{"StaffKiosk","ReturnsCabinet"}){
            string folder=Root+"Resources/Phase8B/Models/",path=folder+name+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(!importer)continue;importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            var wrapper=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.transform.SetParent(wrapper.transform,false);model.transform.localRotation=Quaternion.Euler(0,180,0)*model.transform.localRotation;
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>{
                string target=folder+m.name.Replace(" ","_")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(target);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,target);}mat.name=Path.GetFileNameWithoutExtension(target);
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+name+"_Albedo.png");if(m.mainTexture&&texture){mat.mainTexture=texture;mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",Color.white);}else mat.SetColor("_BaseColor",m.HasProperty("_BaseColor")?m.GetColor("_BaseColor"):m.color);mat.SetFloat("_Smoothness",.16f);mat.SetFloat("_Metallic",0);EditorUtility.SetDirty(mat);return mat;}).ToArray();
            wrapper.layer=2;foreach(var t in wrapper.GetComponentsInChildren<Transform>())t.gameObject.layer=2;var body=wrapper.AddComponent<BoxCollider>();body.center=name=="StaffKiosk"?new Vector3(0,.85f,0):new Vector3(0,.44f,0);body.size=name=="StaffKiosk"?new Vector3(.90f,1.70f,1.0f):new Vector3(1.01f,.88f,.47f);
            PrefabUtility.SaveAsPrefabAsset(wrapper,Root+"Resources/Phase8B/"+name+".prefab");UnityEngine.Object.DestroyImmediate(wrapper);
        }AssetDatabase.SaveAssets();}
    }
}
