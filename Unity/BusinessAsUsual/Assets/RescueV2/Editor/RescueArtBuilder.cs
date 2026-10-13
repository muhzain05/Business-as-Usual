using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace BusinessAsUsual.RescueV2.Editor
{
    public static class RescueArtBuilder
    {
        const string Base="Assets/RescueV2/Resources/RescueV2/Models/";
        public static void PrepareGoldenPrint(){
            var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Product_Golden.prefab"));
            try{var filter=model.GetComponentInChildren<MeshFilter>();var copy=UnityEngine.Object.Instantiate(filter.sharedMesh);var vertices=copy.vertices;var normals=copy.normals;var world=vertices.Select(filter.transform.TransformPoint).ToArray();float xmin=world.Min(p=>p.x),xmax=world.Max(p=>p.x),ymin=world.Min(p=>p.y),ymax=world.Max(p=>p.y),zmin=world.Min(p=>p.z),zmax=world.Max(p=>p.z);var uv=copy.uv;
                for(int i=0;i<world.Length;i++){var p=world[i];var n=filter.transform.TransformDirection(normals[i]);float u=Mathf.Abs(n.z)>Mathf.Abs(n.x)?(n.z>0?1-Mathf.InverseLerp(xmin,xmax,p.x):Mathf.InverseLerp(xmin,xmax,p.x)):(n.x>0?Mathf.InverseLerp(zmin,zmax,p.z):1-Mathf.InverseLerp(zmin,zmax,p.z));if(Mathf.Abs(n.y)<.75f)uv[i]=new Vector2(u,Mathf.InverseLerp(ymin,ymax,p.y));}copy.uv=uv;copy.name="GoldenPrintMesh";
                string path=Base+"GoldenPrintMesh.asset";var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path);if(old){EditorUtility.CopySerialized(copy,old);UnityEngine.Object.DestroyImmediate(copy);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(copy,path);
                var material=AssetDatabase.LoadAssetAtPath<Material>(Base+"GoldenPrint.mat");material.mainTextureScale=Vector2.one;material.mainTextureOffset=Vector2.zero;EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
            }finally{UnityEngine.Object.DestroyImmediate(model);}
        }
        [MenuItem("Business as Usual/Rescue V2/Prepare physical assets")]
        public static void Prepare(){
            Directory.CreateDirectory(Base);
            var path=Base+"UtilityWagon.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Legacy;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var wagon=new GameObject("WagonRig");var model=(GameObject)PrefabUtility.InstantiatePrefab(source);model.transform.SetParent(wagon.transform,false);model.transform.localRotation=Quaternion.Euler(0,180,0)*model.transform.localRotation;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(Convert).ToArray();
            var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__preview__")&&c.name.Contains("HoodCleanOpen"));
            if(!clip)throw new Exception("Authored bonnet animation missing");
            var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(Base+"HoodOpen.anim");if(existing)EditorUtility.CopySerialized(clip,existing);else AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(clip),Base+"HoodOpen.anim");
            var saved=AssetDatabase.LoadAssetAtPath<AnimationClip>(Base+"HoodOpen.anim");saved.name="HoodOpen";EditorUtility.SetDirty(saved);
            PrefabUtility.SaveAsPrefabAsset(wagon,Base+"WagonRig.prefab");UnityEngine.Object.DestroyImmediate(wagon);
            Equipment("EngineAssembly","EngineRig");Equipment("CollectionCart","CartRig");CartAnimation();Products();Gloves();Battery();Carton();Bag();AssetDatabase.SaveAssets();
        }
        static Material Mat(string name,Color color,float metal=0){var path=Base+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.24f);return m;}
        static Material Convert(Material old){
            if(!old)return Mat("Town graphite",new Color(.08f,.13f,.13f));string name=old.name.Replace("(Instance)","").Trim();var m=Mat("Wagon "+name,old.color);m.mainTexture=old.mainTexture;return m;
        }
        static void Equipment(string source,string name){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Base+source+".fbx");if(!asset)return;var g=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(asset);model.transform.SetParent(g.transform,false);foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>{var converted=Mat(name+" "+m.name,Color.white);converted.mainTexture=m.mainTexture;return converted;}).ToArray();Save(g,name);}
        static void CartAnimation(){string path=Base+"CollectionCart.fbx";var m=AssetImporter.GetAtPath(path) as ModelImporter;if(!m)return;m.importAnimation=true;m.animationType=ModelImporterAnimationType.Legacy;m.SaveAndReimport();var clip=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__preview__"));if(!clip)return;var output=Base+"CartRoll.anim";var old=AssetDatabase.LoadAssetAtPath<AnimationClip>(output);if(old)EditorUtility.CopySerialized(clip,old);else AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(clip),output);var saved=AssetDatabase.LoadAssetAtPath<AnimationClip>(output);saved.name="CartRoll";EditorUtility.SetDirty(saved);}
        static void Products(){foreach(string brand in new[]{"Sunbeam","Meadow","Golden"}){var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Shopkeeper/Prefabs/Product_"+brand+".prefab");var g=(GameObject)PrefabUtility.InstantiatePrefab(source);foreach(var c in g.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);Save(g,"Product_"+brand);}}
        static void Gloves(){
            string path=Base+"WorkGloves.fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(!importer)return;
            importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Legacy;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            string take=importer.defaultClipAnimations[0].takeName;string[] names={"Idle","Reach","Grip","Tool","Probe","Stamp","Sort"};int[] starts={1,22,43,64,85,106,127},ends={21,42,63,84,105,126,151};
            importer.clipAnimations=names.Select((n,i)=>new ModelImporterClipAnimation{name=n,takeName=take,firstFrame=starts[i],lastFrame=ends[i],loopTime=n=="Idle"||n=="Grip"}).ToArray();importer.SaveAndReimport();
            foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__"))){var output=Base+"Gloves_"+clip.name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(output);if(existing)EditorUtility.CopySerialized(clip,existing);else AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(clip),output);var saved=AssetDatabase.LoadAssetAtPath<AnimationClip>(output);saved.name="Gloves_"+clip.name;EditorUtility.SetDirty(saved);}
            var g=new GameObject("GloveRig");var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));model.transform.SetParent(g.transform,false);
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>{var converted=Mat("Gloves "+m.name,m.color);return converted;}).ToArray();Save(g,"GloveRig");
        }
        static Transform Part(Transform root,string name,Vector3 at,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube){var p=GameObject.CreatePrimitive(type);p.name=name;p.transform.SetParent(root,false);p.transform.localPosition=at;p.transform.localScale=size;p.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());return p.transform;}
        static void Save(GameObject root,string name){PrefabUtility.SaveAsPrefabAsset(root,Base+name+".prefab");UnityEngine.Object.DestroyImmediate(root);}
        static void Battery(){
            var root=new GameObject("Battery");var ink=Mat("Battery casing",new Color(.12f,.20f,.19f));var brass=Mat("Terminal brass",new Color(.69f,.51f,.20f),.5f);var paper=Mat("Battery label",new Color(.94f,.86f,.64f));
            Part(root.transform,"Casing",new Vector3(0,.10f,0),new Vector3(.24f,.20f,.27f),ink);Part(root.transform,"Lid",new Vector3(0,.205f,0),new Vector3(.25f,.02f,.28f),ink);
            Part(root.transform,"Positive post",new Vector3(.085f,.229f,0),new Vector3(.035f,.022f,.035f),brass,PrimitiveType.Cylinder);Part(root.transform,"Negative post",new Vector3(-.085f,.229f,0),new Vector3(.035f,.022f,.035f),brass,PrimitiveType.Cylinder);
            Part(root.transform,"Positive sleeve",new Vector3(.085f,.212f,0),new Vector3(.048f,.009f,.048f),Mat("Positive red",new Color(.68f,.25f,.17f)));
            Part(root.transform,"Printed label",new Vector3(0,.12f,-.136f),new Vector3(.16f,.10f,.002f),paper);
            for(int i=0;i<3;i++)Part(root.transform,"Label bar",new Vector3(-.045f+i*.045f,.12f,-.138f),new Vector3(.022f,.052f,.003f),ink);
            for(float x=-.07f;x<.08f;x+=.14f)Part(root.transform,"Handle foot",new Vector3(x,.226f,.085f),new Vector3(.015f,.04f,.018f),ink);
            Part(root.transform,"Carry handle",new Vector3(0,.25f,.085f),new Vector3(.15f,.016f,.018f),ink);Save(root,"Battery");
        }
        static void Carton(){if(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"ShippingCarton.fbx")){Equipment("ShippingCarton","Carton");return;}var root=new GameObject("Carton");var kraft=Mat("Kraft carton",new Color(.63f,.44f,.24f));var tape=Mat("Carton tape",new Color(.88f,.77f,.48f));Part(root.transform,"Box",new Vector3(0,.16f,0),new Vector3(.42f,.32f,.36f),kraft);Part(root.transform,"Tape",new Vector3(0,.321f,0),new Vector3(.07f,.003f,.36f),tape);Part(root.transform,"Shipping label",new Vector3(.08f,.19f,-.181f),new Vector3(.19f,.12f,.002f),Mat("Paper label",new Color(.96f,.90f,.72f)));Save(root,"Carton");}
        static void Bag(){var root=new GameObject("CollectionBag");var paper=Mat("Paper collection sack",new Color(.59f,.64f,.48f));Part(root.transform,"Sack",new Vector3(0,.22f,0),new Vector3(.36f,.43f,.31f),paper,PrimitiveType.Sphere);Part(root.transform,"Tied neck",new Vector3(0,.45f,0),new Vector3(.055f,.08f,.055f),paper,PrimitiveType.Cylinder);Part(root.transform,"Collection tag",new Vector3(.09f,.33f,-.14f),new Vector3(.11f,.085f,.012f),Mat("Paper label",new Color(.96f,.90f,.72f)));Save(root,"CollectionBag");}
    }
}

