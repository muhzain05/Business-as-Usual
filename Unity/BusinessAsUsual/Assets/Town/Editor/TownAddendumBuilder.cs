using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BusinessAsUsual.Town.Editor
{
    // Additive art pass over the accepted town. Gameplay transforms and collision remain untouched.
    public static class TownAddendumBuilder
    {
        const string Root="Assets/Town/";
        const string Kit="Assets/Town/Prefabs/Environment/Addendum/";
        [MenuItem("Business as Usual/Phase 6 Addendum/Prepare Building Kit")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root+"Art/Processed/Addendum");Directory.CreateDirectory(Kit);
            foreach(var path in Directory.GetFiles(Root+"Art/Addendum","*.fbx")){
                string name=Path.GetFileNameWithoutExtension(path);var source=AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\','/'));if(!source)throw new Exception("Missing import: "+path);
                var instance=UnityEngine.Object.Instantiate(source);var filter=instance.GetComponentInChildren<MeshFilter>();var renderer=filter.GetComponent<MeshRenderer>();var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name=name;
                var matrix=filter.transform.localToWorldMatrix;var vertices=mesh.vertices;var normals=mesh.normals;
                for(int i=0;i<vertices.Length;i++){var v=matrix.MultiplyPoint3x4(vertices[i]);v.z=-v.z;vertices[i]=v;var n=matrix.MultiplyVector(normals[i]).normalized;n.z=-n.z;normals[i]=n;}mesh.vertices=vertices;mesh.normals=normals;
                for(int sub=0;sub<mesh.subMeshCount;sub++){var triangles=mesh.GetTriangles(sub);for(int i=0;i<triangles.Length;i+=3){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}mesh.SetTriangles(triangles,sub);}mesh.RecalculateBounds();
                string meshPath=Root+"Art/Processed/Addendum/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing){EditorUtility.CopySerialized(mesh,existing);existing.vertices=mesh.vertices;existing.normals=mesh.normals;existing.uv=mesh.uv;for(int sub=0;sub<mesh.subMeshCount;sub++)existing.SetTriangles(mesh.GetTriangles(sub),sub);existing.RecalculateBounds();UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,meshPath);
                var prefab=new GameObject(name);prefab.AddComponent<MeshFilter>().sharedMesh=mesh;prefab.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials.Select(m=>AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/"+m.name.Replace("Town_","").Split('.')[0]+".mat")).ToArray();
                if(prefab.GetComponent<Renderer>().sharedMaterials.Any(m=>!m))throw new Exception("Palette missing: "+name);
                PrefabUtility.SaveAsPrefabAsset(prefab,Kit+name+".prefab");UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(instance);
            }
            AssetDatabase.SaveAssets();Debug.Log("Five original reusable kit models prepared with shared existing materials.");
        }
        static GameObject Prop(string name,Vector3 position,Transform root,float yaw=0,Vector3? scale=null,bool addendum=true)
        {
            string path=(addendum?Kit:Root+"Prefabs/Environment/")+name+".prefab";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!prefab)throw new Exception("Missing model: "+path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(root,true);go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));go.transform.localScale=scale??Vector3.one;go.isStatic=true;return go;
        }
        static string[] CollisionSignature()=>UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(c=>c.name+"|"+c.enabled+"|"+c.transform.position.ToString("F4")+"|"+c.transform.rotation.ToString("F4")+"|"+c.transform.lossyScale.ToString("F4")).OrderBy(s=>s).ToArray();
        [MenuItem("Business as Usual/Phase 6 Addendum/Apply Town Correction")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=TownBuilder.ScenePath)throw new Exception("Open the stopped Phase 6 town.");
            if(GameObject.Find("Daylight Modeled Neighborhood"))throw new Exception("Addendum already assembled; retain inspected art edits.");
            var before=CollisionSignature();var root=new GameObject("Daylight Modeled Neighborhood").transform;var old=GameObject.Find("Original Daylight Neighborhood").transform;var graybox=GameObject.Find("Graybox Town").transform;
            foreach(var renderer in old.GetComponentsInChildren<Renderer>()){
                string name=renderer.name;var p=renderer.transform.position;
                if(name=="Shopfront"||name=="GarageFront"||name=="Roof"||name=="RoofDistant"||name=="Plastered gable end"||name=="Neighbor plaster"||name=="North neighbor plaster"||name=="Distant side house"||(name=="Window"&&(Mathf.Abs(p.x)>12||Mathf.Abs(p.z)>13)))renderer.enabled=false;
            }
            foreach(var panel in graybox.GetComponentsInChildren<Renderer>().Where(r=>r.name=="WallPanel"&&(r.transform.parent.name.StartsWith("Shop ")||r.transform.parent.name.StartsWith("Garage "))))panel.enabled=false;
            Prop("MarketShell",new Vector3(-8,0,6.5f),root);Prop("WorkshopShell",new Vector3(8,0,6.5f),root);
            for(int i=0;i<6;i++){float x=-20+i*8;Prop(i%2==0?"TownHouseA":"TownHouseB",new Vector3(x,0,-17),root,180);Prop(i%2==0?"TownHouseB":"TownHouseA",new Vector3(x,0,18),root);}
            foreach(float side in new[]{-1f,1f})for(int i=0;i<5;i++)Prop(i%2==0?"TownHouseA":"TownHouseB",new Vector3(side*21,0,-12+i*6),root,side<0?270:90,new Vector3(.92f,.92f,.92f));
            Prop("ParcelCart",new Vector3(-12.7f,0,3.2f),root,90);Prop("ParcelCart",new Vector3(-11.25f,0,10.45f),root,24);
            foreach(var label in old.GetComponentsInChildren<TextMesh>()){
                if(label.text=="DAYLIGHT MARKET"||label.text=="MIKAN MOTOR"){var p=label.transform.position;p.z=3.1f;label.transform.position=p;}
                if(label.text.StartsWith("SV-001")||label.text.StartsWith("SV-002")){label.text=label.text.StartsWith("SV-001")?"BAY 1 / SV-001":"BAY 2 / SV-002";label.characterSize=.014f;}
            }
            // Public wayfinding at the shared square. Signs have no collision and do not supply private instructions.
            var post=Prop("Drainpipe",new Vector3(-1.55f,0,5.1f),root,0,new Vector3(.8f,.92f,.8f),false);post.name="Square municipal signpost";
            string[] signs={"MARKET   <","GARAGE   >","YARD   v"};for(int i=0;i<3;i++){
                var panel=Prop("WallPanel",new Vector3(-1.55f,2.30f-i*.36f,5.1f),root,0,new Vector3(1.75f,.29f,.13f),false);panel.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Teal.mat");
                TownArtBuilder.Sign(signs[i],new Vector3(-1.55f,2.30f-i*.36f,5.015f),root,1.7f,.012f);
            }
            TownArtBuilder.Sign("DAYLIGHT SQUARE",new Vector3(2.45f,2.29f,7.76f),root,1.8f,.012f);
            // Soft daytime, with a restrained practical workshop fill for the covered work areas.
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.77f,.83f,.83f);RenderSettings.ambientEquatorColor=new Color(.73f,.71f,.59f);RenderSettings.ambientGroundColor=new Color(.47f,.50f,.41f);
            RenderSettings.fogColor=new Color(.76f,.83f,.82f);RenderSettings.fogStartDistance=42;RenderSettings.fogEndDistance=95;
            var sun=GameObject.Find("Sun").GetComponent<Light>();sun.color=new Color(1,.95f,.83f);sun.intensity=1.3f;sun.transform.rotation=Quaternion.Euler(47,-28,0);sun.shadowStrength=.72f;
            var fill=new GameObject("Workshop daylight fill");fill.transform.SetParent(root);fill.transform.position=new Vector3(8,2.7f,6.2f);var light=fill.AddComponent<Light>();light.type=LightType.Point;light.range=5.8f;light.intensity=.7f;light.color=new Color(1,.96f,.90f);light.shadows=LightShadows.None;
            if(!before.SequenceEqual(CollisionSignature()))throw new Exception("Collision changed during decorative assembly.");
            TownArtBuilder.DepthLabels();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            File.WriteAllText("../../Docs/Diagnostics/Phase6Addendum/scene-assembly.json",JsonUtility.ToJson(new AssemblyRecord {colliders=before.Length,collisionSignatureUnchanged=true,models=5,daytimeOnly=true},true));Debug.Log("Modeled neighborhood assembled; all "+before.Length+" collider transforms preserved.");
        }
        [Serializable] class AssemblyRecord {public int colliders,models;public bool collisionSignatureUnchanged,daytimeOnly;}
    }
}
