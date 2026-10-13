using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace BusinessAsUsual.Town.Editor
{
    public static class TownEnvironmentReview
    {
        static readonly string[] Names={"street","square","garage","yard","overhead","shopfront","alley"};
        static readonly Vector3[] Positions={new Vector3(0,1.7f,-7),new Vector3(-1.5f,1.7f,3.5f),new Vector3(8,1.7f,2.1f),new Vector3(7,1.7f,-3.5f),new Vector3(0,32,-1),new Vector3(-8,1.7f,-.8f),new Vector3(-4,1.7f,11.7f)};
        static readonly Vector3[] Aims={new Vector3(0,1.5f,5),new Vector3(1,1.4f,7.5f),new Vector3(8,1.3f,8),new Vector3(8,1.2f,-8.5f),Vector3.zero,new Vector3(-8,1.6f,4),new Vector3(-9,1.2f,10.5f)};
        [MenuItem("Business as Usual/Phase 6/Capture Town Comparison")]
        public static void CaptureComparison()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop before capture.");
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save changes before capture.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try{EditorSceneManager.OpenScene("Assets/Prototype/Scenes/TownPrototype.unity");Capture("Before");EditorSceneManager.OpenScene(TownBuilder.ScenePath);Capture("After");Budget();}
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        static void Capture(string folder)
        {
            string path="../../Docs/Diagnostics/Phase6/"+folder;Directory.CreateDirectory(path);
            var go=new GameObject("Temporary review camera");var camera=go.AddComponent<Camera>();camera.fieldOfView=60;camera.farClipPlane=100;camera.backgroundColor=new Color(.64f,.76f,.79f);camera.clearFlags=CameraClearFlags.Skybox;camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            try{for(int i=0;i<Names.Length;i++){
                camera.transform.position=Positions[i];camera.transform.LookAt(Aims[i]);var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path+"/"+Names[i]+".png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
            }}finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Serializable] class BudgetReport {public int renderers,materialAssets,uniqueMeshes;public long instancedTriangles,uniqueTriangles;public string notes;public View[] viewpoints;}
        [Serializable] class View {public string name;public Vector3 position,aim;}
        static void Budget()
        {
            var renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.transform.root.name!="Daylight Market Interior").ToArray();var filters=renderers.Select(r=>r.GetComponent<MeshFilter>()).Where(f=>f&&f.sharedMesh).ToArray();var meshes=filters.Select(f=>f.sharedMesh).Distinct().ToArray();
            var report=new BudgetReport{renderers=renderers.Length,materialAssets=renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count(),uniqueMeshes=meshes.Length,instancedTriangles=filters.Sum(f=>(long)f.sharedMesh.triangles.Length/3),uniqueTriangles=meshes.Sum(m=>(long)m.triangles.Length/3),notes="Active town environment geometry including workplace fixtures, excluding the connected approved shop and runtime Sunny. Meshes and instanced material assets are shared. Draw calls and shipping hardware performance require profiler measurement.",viewpoints=Names.Select((name,i)=>new View{name=name,position=Positions[i],aim=Aims[i]}).ToArray()};
            File.WriteAllText("../../Docs/Diagnostics/Phase6/environment-budget.json",JsonUtility.ToJson(report,true));
        }
    }
}
