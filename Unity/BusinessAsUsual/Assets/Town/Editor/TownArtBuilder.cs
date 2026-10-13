using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Town.Editor
{
    public static class TownArtBuilder
    {
        const string Root="Assets/Town/";
        static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        static Material Mat(string name,Color color,string texture=null)
        {
            if(materials.TryGetValue(name,out var cached))return cached;
            string path=Root+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.color=color;mat.SetFloat("_Smoothness",name=="Glass"?.30f:.16f);mat.enableInstancing=true;
            if(texture!=null){mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+texture+".png"));mat.color=Color.white;}
            materials[name]=mat;return mat;
        }
        static void Palette()
        {
            string[] names={"Plaster","Sage","Teal","Coral","Mustard","Wood","Glass","Steel","Rubber","Leaf","LeafLight","Paper","Blue","Bag","Light"};
            Color[] colors={new Color(.83f,.75f,.60f),new Color(.28f,.48f,.37f),new Color(.075f,.19f,.21f),new Color(.64f,.28f,.18f),new Color(.88f,.64f,.22f),new Color(.42f,.25f,.12f),new Color(.10f,.23f,.26f),new Color(.39f,.46f,.44f),new Color(.09f,.12f,.12f),new Color(.37f,.56f,.27f),new Color(.53f,.65f,.34f),new Color(.96f,.90f,.72f),new Color(.29f,.48f,.52f),new Color(.19f,.30f,.27f),new Color(1,.79f,.38f)};
            for(int i=0;i<names.Length;i++)Mat(names[i],colors[i],names[i]=="Plaster"?"plaster":names[i]=="Wood"?"wood":null);
            Mat("Asphalt",Color.white,"asphalt");Mat("Paving",Color.white,"paving");
        }
        [MenuItem("Business as Usual/Phase 6/Prepare Original Town Art")]
        public static void PrepareArt()
        {
            Directory.CreateDirectory(Root+"Art/Processed");Directory.CreateDirectory(Root+"Prefabs/Environment");Palette();
            foreach(string path in Directory.GetFiles(Root+"Art","*.fbx")){
                string name=Path.GetFileNameWithoutExtension(path);var source=AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\','/'));if(!source)throw new Exception("Import missing: "+path);
                var instance=UnityEngine.Object.Instantiate(source);var filter=instance.GetComponentInChildren<MeshFilter>();var renderer=filter.GetComponent<MeshRenderer>();var sourceMesh=filter.sharedMesh;
                var mesh=UnityEngine.Object.Instantiate(sourceMesh);mesh.name=name;var matrix=filter.transform.localToWorldMatrix;var vertices=mesh.vertices;var normals=mesh.normals;
                for(int i=0;i<vertices.Length;i++){var v=matrix.MultiplyPoint3x4(vertices[i]);v.z=-v.z;vertices[i]=v;var n=matrix.MultiplyVector(normals[i]).normalized;n.z=-n.z;normals[i]=n;}
                mesh.vertices=vertices;mesh.normals=normals;
                for(int sub=0;sub<mesh.subMeshCount;sub++){var triangles=mesh.GetTriangles(sub);for(int i=0;i<triangles.Length;i+=3){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}mesh.SetTriangles(triangles,sub);}
                mesh.RecalculateBounds();string meshPath=Root+"Art/Processed/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,meshPath);
                var prefab=new GameObject(name);prefab.AddComponent<MeshFilter>().sharedMesh=mesh;prefab.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials.Select(m=>materials[m.name.Replace("Town_","")]).ToArray();
                PrefabUtility.SaveAsPrefabAsset(prefab,Root+"Prefabs/Environment/"+name+".prefab");UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(instance);
            }
            AssetDatabase.SaveAssets();Debug.Log("Original town meshes normalized, material palette shared, reusable prefabs ready.");
        }
        static GameObject Prop(string asset,Vector3 position,Transform parent,float yaw=0,Vector3? scale=null)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Environment/"+asset+".prefab");if(!prefab)throw new Exception("Missing prepared prefab: "+asset);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent,true);go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));go.transform.localScale=scale??Vector3.one;go.isStatic=true;return go;
        }
        static void Hide(Renderer renderer){renderer.enabled=false;}
        static GameObject Panel(string name,Vector3 position,Vector3 size,Material material,Transform parent)
        {var go=Prop("WallPanel",position,parent,0,size);go.name=name;go.GetComponent<Renderer>().sharedMaterial=material;return go;}
        static void Collision(GameObject go,Vector3 size,Vector3 center)
        {var box=go.AddComponent<BoxCollider>();box.size=size;box.center=center;}
        static void NormalizeFixture(GameObject go)
        {var size=go.transform.localScale;foreach(var box in go.GetComponents<BoxCollider>()){box.size=Vector3.Scale(box.size,size);box.center=Vector3.Scale(box.center,size);}go.transform.localScale=Vector3.one;foreach(var renderer in go.GetComponents<Renderer>())Hide(renderer);}
        static void Fixture(GameObject go,string model,float floorOffset)
        {NormalizeFixture(go);Prop(model,go.transform.position+Vector3.down*floorOffset,go.transform);}
        public static void Sign(string text,Vector3 position,Transform parent,float width=3,float size=.028f)
        {
            var go=new GameObject(text+" / sign");go.transform.SetParent(parent);go.transform.position=position;var label=go.AddComponent<TextMesh>();label.text=text;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=Shopkeeper.ShopUITheme.Paper;label.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Shopkeeper/UI/Resources/Handbook/Fonts/DelaGothicOne-Regular.ttf");go.GetComponent<MeshRenderer>().sharedMaterial=label.font.material;go.isStatic=true;
        }
        public static void DepthLabels()
        {
            foreach(var label in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.transform.position.x>-25)){
                if(!label.font)continue;
                string path=Root+"Materials/Text-"+label.font.name+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!mat){mat=new Material(label.font.material);AssetDatabase.CreateAsset(mat,path);}
                mat.shader=Shader.Find("Business as Usual/Shelf Text");mat.mainTexture=label.font.material.mainTexture;EditorUtility.SetDirty(mat);
                label.GetComponent<Renderer>().sharedMaterial=mat;
                if(!label.GetComponent<TownWorldText>())label.gameObject.AddComponent<TownWorldText>();
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void Poster(string name,Vector3 position,Transform parent,float yaw=0,Vector3? size=null)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=name;go.transform.SetParent(parent);go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));go.transform.localScale=size??new Vector3(.52f,.65f,1);UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=Mat(name,Color.white,name);go.isStatic=true;
        }
        [MenuItem("Business as Usual/Phase 6/Improve Town Atmosphere")]
        public static void Improve()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Stop and save before art assembly.");
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=TownBuilder.ScenePath)throw new Exception("Open the Phase 6 town first.");
            if(GameObject.Find("Original Daylight Neighborhood"))throw new Exception("Town art already assembled; preserve custom edits.");
            Palette();var root=new GameObject("Original Daylight Neighborhood").transform;
            var town=GameObject.Find("Graybox Town").transform;
            foreach(var text in town.GetComponentsInChildren<TextMesh>())text.gameObject.SetActive(false);
            foreach(var renderer in town.GetComponentsInChildren<MeshRenderer>()){
                var go=renderer.gameObject;string name=go.name;
                if(name=="Ground")renderer.sharedMaterial=Mat("Ground",new Color(.42f,.55f,.36f));
                else if(name=="Main Street")renderer.sharedMaterial=materials["Asphalt"];
                else if(name.Contains("Walkway")||name=="Town Square"||name=="Sanitation Pad")renderer.sharedMaterial=materials["Paving"];
                else if(name=="Back Alley")renderer.sharedMaterial=materials["Asphalt"];
                else if(name.StartsWith("Shop Front")||name=="Shop Door Lintel"||name.StartsWith("Garage Front")||name=="Garage Lintel"||name.StartsWith("Street Lamp")||name.StartsWith("Lamp Head")||name.StartsWith("Bench ")||name=="Shop Counter"||name=="Square Monument"||name=="Door Panel"||name.StartsWith("Boundary")||name=="Yard Back Fence")Hide(renderer);
                else if(name.StartsWith("Shop ")||name.StartsWith("Garage ")){
                    Hide(renderer);var child=Prop("WallPanel",go.transform.position,go.transform);child.transform.localRotation=Quaternion.identity;child.transform.localScale=Vector3.one;child.GetComponent<Renderer>().sharedMaterial=name.StartsWith("Shop")?materials["Plaster"]:materials["Blue"];
                }
            }
            // Preserve collider/interaction wiring while replacing their graybox visible geometry.
            Prop("Shopfront",new Vector3(-8,0,3.5f),root);Prop("Awning",new Vector3(-8,2.85f,3.15f),root);Prop("Roof",new Vector3(-8,3.28f,6.5f),root,0,new Vector3(1,.9f,.93f));
            Prop("GarageFront",new Vector3(8,0,3.5f),root);Prop("Roof",new Vector3(8,3.35f,6.5f),root,0,new Vector3(1,.78f,.93f));
            Sign("DAYLIGHT MARKET",new Vector3(-8,3.08f,3.18f),root,6,.033f);Sign("MIKAN MOTOR",new Vector3(8,3.18f,3.25f),root,5,.039f);
            var door=UnityEngine.Object.FindFirstObjectByType<SimpleDoor>();if(door){var panel=door.hinge.GetComponentsInChildren<MeshRenderer>().First().gameObject;NormalizeFixture(panel);Prop("Door",panel.transform.position,panel.transform);}
            Fixture(GameObject.Find("Service Desk"),"Workbench",.45f);Fixture(GameObject.Find("Collection Route Desk"),"Workbench",.4f);
            for(int i=0;i<3;i++){
                var bin=GameObject.Find(new[]{"Paper Sorting Bin","Mixed Sorting Bin","Metal Sorting Bin"}[i]);Fixture(bin,"WasteBin",.55f);Sign(new[]{"PAPER","MIXED","METAL"}[i],bin.transform.position+new Vector3(0,.22f,-.45f),bin.transform,1,.015f);
            }
            for(int i=0;i<2;i++){
                var vehicle=GameObject.Find("Service Vehicle "+i);NormalizeFixture(vehicle);Prop("ServiceVan",new Vector3(vehicle.transform.position.x,0,vehicle.transform.position.z),vehicle.transform,0,new Vector3(.86f,.84f,.84f));
                Sign(i==0?"SV-001 / BATTERY":"SV-002 / FILTER",new Vector3(vehicle.transform.position.x,.12f,4.8f),root,1.4f,.014f);
            }
            foreach(var bag in UnityEngine.Object.FindObjectsByType<WasteBag>(FindObjectsSortMode.None)){
                foreach(var renderer in bag.GetComponents<Renderer>())Hide(renderer);bag.transform.localScale=Vector3.one;bag.GetComponent<SphereCollider>().radius=.27f;var visual=Prop("WasteBag",bag.transform.position,bag.transform);visual.isStatic=false;
            }
            Prop("ToolRack",new Vector3(8,1.65f,9.25f),root);Prop("ToolRack",new Vector3(10.2f,1.55f,9.25f),root,0,new Vector3(.55f,.8f,1));
            Poster("notice-garage",new Vector3(5.55f,1.6f,3.29f),root);Poster("notice-shop",new Vector3(-10.75f,1.56f,3.17f),root,0,new Vector3(.50f,.63f,1));
            for(int i=0;i<4;i++)Prop("ServiceVan",new Vector3(7.1f+i*.38f,1.04f,8.05f),root,0,new Vector3(.08f,.08f,.08f));
            // Square landmark / shared gathering area.
            var clock=Prop("Clock",new Vector3(0,0,6.5f),root);Collision(clock,new Vector3(.88f,.18f,.88f),new Vector3(0,.09f,0));var stem=clock.AddComponent<CapsuleCollider>();stem.radius=.12f;stem.height=2.7f;stem.center=new Vector3(0,1.5f,0);
            var board=Prop("Noticeboard",new Vector3(2.45f,0,7.95f),root);Collision(board,new Vector3(1.85f,2.2f,.36f),new Vector3(0,1.1f,0));Poster("notice-town",new Vector3(2.05f,1.46f,7.83f),root,0,new Vector3(.38f,.48f,1));
            for(int i=0;i<2;i++){var bench=Prop("Bench",new Vector3(i==0?-2.5f:2.5f,0,7.8f),root);if(i==1)bench.transform.position=new Vector3(2.5f,0,5.5f);Collision(bench,new Vector3(1.8f,.55f,.75f),new Vector3(0,.5f,.08f));}
            foreach(var old in town.GetComponentsInChildren<Collider>().Where(c=>c.name.StartsWith("Bench ")||c.name=="Square Monument"||c.name=="Shop Counter"))old.enabled=false;
            foreach(var p in new[]{new Vector3(-3.3f,0,4.3f),new Vector3(3.3f,0,4.3f),new Vector3(-3.1f,0,8.6f),new Vector3(3.2f,0,8.9f),new Vector3(-10.5f,0,2.1f),new Vector3(-5.5f,0,2.2f),new Vector3(11.8f,0,2.6f)}){var pot=Prop("Planter",p,root);Collision(pot,new Vector3(.72f,.75f,.72f),new Vector3(0,.38f,0));}
            // Curbs/crossings/paint keep the central circulation routes unobstructed.
            for(int side=-1;side<=1;side+=2)for(int i=0;i<28;i++)Prop("Curb",new Vector3(-13.5f+i,.04f,side*2.5f),root);
            for(int i=0;i<7;i++)Panel("Pedestrian crossing",new Vector3(-2.3f+i*.76f,.05f,0),new Vector3(.36f,.013f,4.2f),materials["Paper"],root);
            for(int i=0;i<8;i++)Panel("Road dash",new Vector3(-12.4f+i*3.5f,.045f,0),new Vector3(1.1f,.01f,.08f),materials["Mustard"],root);
            for(int i=0;i<4;i++)Prop("Lamp",new Vector3(-12+i*8,0,-2.8f),root);
            // Municipal yard: covered sorting area, storage, fencing and carts.
            for(int i=0;i<4;i++)Prop("Fence",new Vector3(5.2f+i*1.85f,0,-10),root);
            Panel("Yard canopy",new Vector3(8,2.65f,-8),new Vector3(7.4f,.18f,4.2f),materials["Sage"],root);
            foreach(float x in new[]{4.5f,11.5f}){Panel("Yard canopy post",new Vector3(x,1.3f,-9.6f),new Vector3(.12f,2.6f,.12f),materials["Teal"],root);}
            Panel("Municipal fascia",new Vector3(8,2.53f,-5.85f),new Vector3(5.6f,.45f,.14f),materials["Teal"],root);Sign("DAYLIGHT / COLLECTION YARD",new Vector3(8,2.57f,-5.94f),root,5,.026f);
            Poster("notice-yard",new Vector3(10.65f,1.7f,-9.83f),root);Prop("WasteCart",new Vector3(11.15f,0,-7.8f),root);for(int i=0;i<3;i++)Prop("Crate",new Vector3(4.5f,.05f+i*.5f,-9.15f),root);
            // Alley and shop pickup locker use original reusable props, no public map expansion.
            Prop("Workbench",new Vector3(-10.8f,0,2.95f),root,0,new Vector3(.45f,.8f,.75f));Sign("PARTS / MUNICIPAL RESERVE",new Vector3(-10.8f,1.12f,2.58f),root,2,.013f);
            for(int i=0;i<3;i++)Prop("Crate",new Vector3(-11.8f,.05f+i*.50f,10.65f),root);
            Prop("WasteBin",new Vector3(12.0f,0,10.65f),root);Prop("Aircon",new Vector3(-7.8f,2.4f,9.69f),root,180);Prop("Drainpipe",new Vector3(-11.25f,0,9.70f),root,180);Prop("Drainpipe",new Vector3(11.25f,0,9.7f),root,180);Poster("notice-alley",new Vector3(-5.3f,1.55f,9.7f),root,180);
            for(int i=0;i<3;i++)Prop("Window",new Vector3(6+i*2,1.8f,9.70f),root,180);
            // Neighborhood backdrop follows existing bounds; it is decorative, with no new traversal land.
            foreach(var p in new[]{new Vector3(-13.5f,0,-7),new Vector3(-12.7f,0,11.6f),new Vector3(13.6f,0,11.7f),new Vector3(-3.7f,0,-10.5f),new Vector3(1.5f,0,-11.4f)}){var tree=Prop("Tree",p,root,0,new Vector3(1.05f,1.05f,1.05f));var collider=tree.AddComponent<CapsuleCollider>();collider.radius=.13f;collider.height=2.0f;collider.center=Vector3.up;}
            for(int i=0;i<6;i++){float x=-20+i*8;Panel("Neighbor plaster",new Vector3(x,2,-17),new Vector3(6.7f,4,5.5f),i%2==0?materials["Plaster"]:materials["Coral"],root);Prop("Roof",new Vector3(x,4.1f,-17),root,0,new Vector3(.96f,.7f,.86f));for(int j=0;j<2;j++)Prop("Window",new Vector3(x-1.6f+j*3.2f,2.5f,-14.17f),root,180);}
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.68f,.76f,.78f);RenderSettings.ambientEquatorColor=new Color(.65f,.62f,.49f);RenderSettings.ambientGroundColor=new Color(.38f,.41f,.34f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=36;RenderSettings.fogEndDistance=85;RenderSettings.fogColor=new Color(.70f,.78f,.77f);
            var sun=GameObject.Find("Sun").GetComponent<Light>();sun.color=new Color(1,.91f,.76f);sun.intensity=1.15f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-32,0);
            var volume=root.gameObject.AddComponent<Volume>();volume.isGlobal=true;var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"Materials/TownAtmosphere.asset");volume.sharedProfile=profile;var grading=profile.Add<ColorAdjustments>(true);grading.postExposure.Override(0);grading.saturation.Override(-4);grading.contrast.Override(5);AssetDatabase.AddObjectToAsset(grading,profile);
            var camera=UnityEngine.Object.FindFirstObjectByType<Multiplayer.NetworkSession>().PreviewCamera;camera.backgroundColor=new Color(.64f,.76f,.79f);
            DepthLabels();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("Phase6 original environment assembled over preserved compact town gameplay.");
        }
        [MenuItem("Business as Usual/Phase 6/Refine Town Atmosphere")]
        public static void Refine()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=TownBuilder.ScenePath)throw new Exception("Open the stopped Phase 6 town.");
            var root=GameObject.Find("Original Daylight Neighborhood").transform;Palette();
            foreach(var roof in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Roof"&&Mathf.Abs(t.position.x)<12&&t.position.z>0)){var p=roof.position;p.y=3.02f;roof.position=p;}
            foreach(var text in root.GetComponentsInChildren<TextMesh>().Where(t=>t.text=="PAPER"||t.text=="MIXED"||t.text=="METAL"||t.text=="DAYLIGHT / COLLECTION YARD")){text.transform.rotation=Quaternion.Euler(0,180,0);var p=text.transform.position;p.z+=text.text.Length>10?.25f:.85f;text.transform.position=p;}
            Details();
            // Fill the gable ends with shared mesh geometry rather than open sky under the roof.
            Gables();
            foreach(var vehicle in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name.StartsWith("Service Vehicle"))){var box=vehicle.GetComponent<BoxCollider>();box.size=new Vector3(1.42f,1.63f,2.4f);box.center=new Vector3(0,.22f,0);}
            var skirt=Panel("Decorative ground skirt",new Vector3(0,-.2f,0),new Vector3(100,.12f,100),Mat("Ground",new Color(.42f,.55f,.36f)),root);
            for(int i=0;i<6;i++){float x=-20+i*8;Panel("North neighbor plaster",new Vector3(x,2,18),new Vector3(6.7f,4,5.5f),i%2==0?materials["Plaster"]:materials["Blue"],root);Prop("Roof",new Vector3(x,4.02f,18),root,0,new Vector3(.96f,.7f,.86f));for(int j=0;j<2;j++)Prop("Window",new Vector3(x-1.6f+j*3.2f,2.5f,15.17f),root);}
            foreach(float side in new[]{-1f,1f})for(int i=0;i<5;i++){float z=-12+i*6;Panel("Distant side house",new Vector3(side*21,1.9f,z),new Vector3(5.5f,3.8f,5.8f),materials["Plaster"],root);Prop("Roof",new Vector3(side*21,3.9f,z),root,0,new Vector3(.8f,.7f,.87f));Prop("Tree",new Vector3(side*17,0,z+2),root);}
            // Render grading for the preview and this phase's own player prefab only.
            LightingCameras();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        public static void Details()
        {
            Palette();var root=GameObject.Find("Original Daylight Neighborhood").transform;
            foreach(var pot in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Planter")){if((pot.position-new Vector3(-10.5f,0,2.1f)).sqrMagnitude<.01f)pot.position=new Vector3(-12.1f,0,1.8f);var box=pot.GetComponent<BoxCollider>();if(box){box.size=new Vector3(.88f,.95f,.88f);box.center=new Vector3(0,.47f,0);}}
            foreach(var bin in UnityEngine.Object.FindObjectsByType<TownStation>(FindObjectsSortMode.None).Where(t=>t.kind==TownStation.Kind.Sort)){bin.transform.rotation=Quaternion.identity;var visual=bin.transform.Find("WasteBin");visual.localRotation=Quaternion.Euler(0,180,0);var renderer=visual.GetComponent<MeshRenderer>();renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m.name=="Paper"?materials[bin.materialType=="Paper"?"Paper":bin.materialType=="Mixed"?"Mustard":"Blue"]:m).ToArray();var label=bin.GetComponentInChildren<TextMesh>();label.transform.rotation=Quaternion.Euler(0,180,0);label.color=Shopkeeper.ShopUITheme.Ink;label.characterSize=.023f;}
            if(!GameObject.Find("Workshop concrete floor")){
                Panel("Workshop concrete floor",new Vector3(8,.04f,6.5f),new Vector3(6.85f,.02f,5.85f),materials["Asphalt"],root);
                foreach(float x in new[]{6.4f,9.6f})foreach(float side in new[]{-1f,1f})Panel("Service bay paint",new Vector3(x+side*.86f,.061f,6.2f),new Vector3(.07f,.009f,2.6f),materials["Mustard"],root);
                Prop("Crate",new Vector3(10.3f,.05f,8.2f),root);Poster("notice-garage",new Vector3(10.3f,.6f,8.15f),root,0,new Vector3(.3f,.36f,1));
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        public static void Gables()
        {
            Palette();var root=GameObject.Find("Original Daylight Neighborhood").transform;
            foreach(var old in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Plastered gable end").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
            var mesh=new Mesh{name="Gable"};mesh.vertices=new[]{new Vector3(0,0,-3.25f),new Vector3(0,0,3.25f),new Vector3(0,1.07f,0),new Vector3(0,0,-3.25f),new Vector3(0,0,3.25f),new Vector3(0,1.07f,0)};mesh.triangles=new[]{0,2,1,3,4,5};mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Root+"Art/Processed/Gable.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
            foreach(float x in new[]{-11.65f,-4.35f,4.35f,11.65f}){var gable=new GameObject("Plastered gable end");gable.transform.SetParent(root);gable.transform.position=new Vector3(x,3.02f,6.5f);gable.AddComponent<MeshFilter>().sharedMesh=mesh;gable.AddComponent<MeshRenderer>().sharedMaterial=materials[x<0?"Plaster":"Blue"];gable.isStatic=true;}
        }
        public static void DistantRoofs()
        {
            var root=GameObject.Find("Original Daylight Neighborhood").transform;
            foreach(var roof in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Roof"&&(Mathf.Abs(t.position.x)>12||Mathf.Abs(t.position.z)>13)).ToArray()){
                Prop("RoofDistant",roof.position,root,roof.eulerAngles.y,roof.localScale);UnityEngine.Object.DestroyImmediate(roof.gameObject);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        public static void LightingCameras()
        {
            // Use the established camera rendering path; warmth comes from palette and scene lighting.
            var session=UnityEngine.Object.FindFirstObjectByType<Multiplayer.NetworkSession>();session.PreviewCamera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            var prefab=PrefabUtility.LoadPrefabContents(TownBuilder.PrefabPath);foreach(var camera in prefab.GetComponentsInChildren<Camera>(true)){camera.backgroundColor=new Color(.64f,.76f,.79f);camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;}PrefabUtility.SaveAsPrefabAsset(prefab,TownBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(prefab);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
    }
}
