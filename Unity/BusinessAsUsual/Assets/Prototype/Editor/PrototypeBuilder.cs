using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BusinessAsUsual.Prototype.Editor
{
    public static class PrototypeBuilder
    {
        public const string ScenePath = "Assets/Prototype/Scenes/TownPrototype.unity";
        public const string PrefabPath = "Assets/Prototype/Prefabs/SunnyPlayer.prefab";
        const string Art = "Assets/Prototype/Art/Sunny/";
        [Serializable] class MaterialEntry { public string name; public float[] color_linear; public float roughness; public float metallic; public Textures textures; }
        [Serializable] class Textures { public string @base; public string normal; public string data; }
        [Serializable] class MaterialList { public MaterialEntry[] items; }
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();

        [MenuItem("Business as Usual/Phase 3/Create First Playable (new scene only)")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before building.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save your open scene before building.");
            if (File.Exists(ScenePath) || File.Exists(PrefabPath)) throw new InvalidOperationException("Prototype exists; refusing to overwrite it.");
            foreach (string folder in new[] { "Scenes", "Prefabs", "Materials", "Animation" }) Directory.CreateDirectory("Assets/Prototype/" + folder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureSunny();
            AnimatorController controller = CreateAnimator();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "TownPrototype";
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.65f, 0.69f, 0.75f);
            RenderSettings.fog = false;
            var lightObj = new GameObject("Sun");
            var light = lightObj.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(45, -35, 0);
            Material ground = Flat("Ground", new Color(0.32f, 0.37f, 0.34f));
            Material paving = Flat("Paving", new Color(0.63f, 0.64f, 0.60f));
            Material street = Flat("Street", new Color(0.23f, 0.26f, 0.29f));
            Material shop = Flat("Shop", new Color(0.65f, 0.62f, 0.52f));
            Material garage = Flat("Garage", new Color(0.48f, 0.57f, 0.61f));
            Material sanitation = Flat("Sanitation", new Color(0.37f, 0.53f, 0.42f));
            Material dark = Flat("Dark", new Color(0.16f, 0.21f, 0.24f));
            Material wood = Flat("Box", new Color(0.64f, 0.42f, 0.22f));
            var town = new GameObject("Graybox Town").transform;
            Cube("Ground", new Vector3(0,-0.2f,0), new Vector3(32,0.4f,28), ground, town);
            Cube("Main Street", new Vector3(0,0.015f,0), new Vector3(28,0.03f,5), street, town, false);
            Cube("Town Square", new Vector3(0,0.025f,5.5f), new Vector3(9,0.05f,6), paving, town, false);
            Cube("North Walkway", new Vector3(0,0.035f,3), new Vector3(28,0.07f,1), paving, town, false);
            Cube("South Walkway", new Vector3(0,0.035f,-3), new Vector3(28,0.07f,1), paving, town, false);
            Cube("Back Alley", new Vector3(0,0.01f,11), new Vector3(29,0.02f,2.5f), dark, town, false);
            // Open interiors with wide, traversable entrances; roofs intentionally deferred.
            var shopRoot = new GameObject("Shop").transform; shopRoot.SetParent(town);
            Cube("Shop Rear", new Vector3(-8,1.5f,9.5f), new Vector3(7,3,0.3f), shop, shopRoot);
            Cube("Shop West", new Vector3(-11.5f,1.5f,6.5f), new Vector3(0.3f,3,6), shop, shopRoot);
            Cube("Shop East", new Vector3(-4.5f,1.5f,6.5f), new Vector3(0.3f,3,6), shop, shopRoot);
            Cube("Shop Front Left", new Vector3(-10,1.5f,3.5f), new Vector3(3,3,0.3f), shop, shopRoot);
            Cube("Shop Front Right", new Vector3(-5.8f,1.5f,3.5f), new Vector3(2.6f,3,0.3f), shop, shopRoot);
            Cube("Shop Door Lintel", new Vector3(-7.8f,2.65f,3.5f), new Vector3(1.6f,0.7f,0.3f), shop, shopRoot);
            Cube("Shop Counter", new Vector3(-9,0.45f,7.5f), new Vector3(3,0.9f,0.8f), dark, shopRoot);
            var doorObj = new GameObject("Shop Door"); doorObj.transform.SetParent(shopRoot);
            var hinge = new GameObject("Hinge").transform; hinge.SetParent(doorObj.transform); hinge.position = new Vector3(-8.5f,0,3.5f);
            var panel = Cube("Door Panel", new Vector3(-7.8f,1.1f,3.5f), new Vector3(1.4f,2.2f,0.12f), dark, hinge);
            var handle = new GameObject("Handle").transform; handle.SetParent(panel.transform); handle.position = new Vector3(-7.3f,0.8f,3.35f);
            var door = doorObj.AddComponent<SimpleDoor>(); door.hinge = hinge; door.handle = handle;
            var garageRoot = new GameObject("Garage").transform; garageRoot.SetParent(town);
            Cube("Garage Rear", new Vector3(8,1.5f,9.5f), new Vector3(7,3,0.3f), garage, garageRoot);
            Cube("Garage West", new Vector3(4.5f,1.5f,6.5f), new Vector3(0.3f,3,6), garage, garageRoot);
            Cube("Garage East", new Vector3(11.5f,1.5f,6.5f), new Vector3(0.3f,3,6), garage, garageRoot);
            Cube("Garage Front Left", new Vector3(5.2f,1.5f,3.5f), new Vector3(1.4f,3,0.3f), garage, garageRoot);
            Cube("Garage Front Right", new Vector3(10.8f,1.5f,3.5f), new Vector3(1.4f,3,0.3f), garage, garageRoot);
            Cube("Garage Lintel", new Vector3(8,2.6f,3.5f), new Vector3(4.2f,0.8f,0.3f), garage, garageRoot);
            var bench = Cube("Garage Workstation", new Vector3(8,0.45f,7.5f), new Vector3(2.5f,0.9f,0.8f), wood, garageRoot);
            bench.AddComponent<BasicWorkstation>();
            var yard = new GameObject("Sanitation Area").transform; yard.SetParent(town);
            Cube("Sanitation Pad", new Vector3(8,0.02f,-7), new Vector3(8,0.04f,6), paving, yard, false);
            Cube("Yard Back Fence", new Vector3(8,0.7f,-10), new Vector3(8,1.4f,0.15f), sanitation, yard);
            for (int i=0; i<3; i++) Cube("Waste Bin " + (i+1), new Vector3(6+i*1.5f,0.55f,-8.5f), new Vector3(1,1.1f,0.9f), sanitation, yard);
            var sorting = Cube("Sorting Workstation", new Vector3(10,0.4f,-6), new Vector3(2,0.8f,0.8f), wood, yard);
            sorting.AddComponent<BasicWorkstation>().stationName = "sanitation sorting table";
            var box = Cube("Delivery Box", new Vector3(-2,0.32f,1.7f), Vector3.one * 0.55f, wood, town);
            var body = box.AddComponent<Rigidbody>(); body.mass = 2; body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            box.AddComponent<CarryableBox>();
            Cube("Square Monument", new Vector3(0,0.4f,6.5f), new Vector3(1.5f,0.8f,1.5f), dark, town);
            for (int i=0; i<2; i++)
            {
                float x = i==0 ? -2.5f : 2.5f;
                Cube("Bench Seat " + i, new Vector3(x,0.4f,7.8f), new Vector3(1.8f,0.15f,0.55f), wood, town);
                Cube("Bench Back " + i, new Vector3(x,0.7f,8.02f), new Vector3(1.8f,0.6f,0.12f), wood, town);
                Cube("Bench Support " + i, new Vector3(x,0.18f,7.8f), new Vector3(1.2f,0.36f,0.35f), dark, town);
            }
            for (int i=0; i<4; i++)
            {
                float x = -12 + i*8;
                Cube("Street Lamp " + i, new Vector3(x,1.1f,-2.8f), new Vector3(0.1f,2.2f,0.1f), dark, town);
                Cube("Lamp Head " + i, new Vector3(x,2.2f,-2.8f), new Vector3(0.4f,0.18f,0.4f), shop, town);
            }
            Cube("Boundary North", new Vector3(0,0.7f,13.8f), new Vector3(32,1.4f,0.3f), ground, town);
            Cube("Boundary South", new Vector3(0,0.7f,-13.8f), new Vector3(32,1.4f,0.3f), ground, town);
            Cube("Boundary West", new Vector3(-15.8f,0.7f,0), new Vector3(0.3f,1.4f,28), ground, town);
            Cube("Boundary East", new Vector3(15.8f,0.7f,0), new Vector3(0.3f,1.4f,28), ground, town);
            Sign("TOWN SQUARE", new Vector3(0,1.9f,6.5f), town);
            Sign("SHOP  /  E: DOOR", new Vector3(-8,2.7f,3.25f), town);
            Sign("GARAGE  /  WORKBENCH", new Vector3(8,2.7f,3.25f), town);
            Sign("SANITATION", new Vector3(8,1.7f,-9.8f), town);
            Sign("BACK ALLEY", new Vector3(0,1.2f,12.7f), town);
            var player = new GameObject("Sunny Player");
            player.layer = 2; player.tag = "Player"; player.transform.position = new Vector3(0,0.08f,-0.8f);
            var cc = player.AddComponent<CharacterController>(); cc.height=1.2f; cc.radius=0.24f; cc.center=new Vector3(0,0.6f,0); cc.stepOffset=0.22f; cc.skinWidth=0.025f; cc.slopeLimit=45;
            var input = player.AddComponent<PrototypeInput>();
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Sunny_v004.fbx"));
            visual.name="Sunny Visual"; visual.transform.SetParent(player.transform,false);
            SetLayer(visual,2);
            var anim = visual.GetComponent<Animator>(); if (!anim) anim=visual.AddComponent<Animator>();
            anim.runtimeAnimatorController=controller; anim.applyRootMotion=false; anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.quality=SkinQuality.Auto;
            var motor=player.AddComponent<ThirdPersonMotor>(); motor.visual=visual.transform; motor.animator=anim; motor.respawnPosition=player.transform.position;
            var carry=new GameObject("Carry Anchor").transform; carry.SetParent(visual.transform,false); carry.localPosition=new Vector3(0,0.55f,0.65f);
            var interactor=player.AddComponent<PlayerInteractor>(); interactor.carryAnchor=carry;
            // Child camera references remain self-contained in the reusable prefab.
            var cameraObj=new GameObject("Third Person Camera"); cameraObj.tag="MainCamera"; cameraObj.transform.SetParent(player.transform,false);
            var camera=cameraObj.AddComponent<Camera>(); camera.nearClipPlane=0.05f; camera.farClipPlane=100; camera.fieldOfView=60; camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(0.52f,0.67f,0.77f);
            cameraObj.AddComponent<AudioListener>();
            var orbit=cameraObj.AddComponent<ThirdPersonCamera>(); orbit.target=player.transform; orbit.input=input; orbit.PositionCamera(); motor.cameraTransform=camera.transform;
            var hud=player.AddComponent<PrototypeHUD>(); hud.player=interactor; hud.input=input;
            PrefabUtility.SaveAsPrefabAssetAndConnect(player,PrefabPath,InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true) }.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject=player;
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0,0,3),Quaternion.Euler(55,0,0),20);
            Debug.Log("Phase 3 prototype created: " + ScenePath);
        }
        static void ConfigureSunny()
        {
            var entries=JsonUtility.FromJson<MaterialList>("{\"items\":"+File.ReadAllText(Art+"material-manifest.json")+"}").items;
            foreach(var entry in entries)
            {
                foreach(string file in new[]{entry.textures.@base,entry.textures.normal,entry.textures.data})
                {
                    if(string.IsNullOrEmpty(file)) continue;
                    var importer=(TextureImporter)AssetImporter.GetAtPath(Art+file);
                    importer.textureType=file==entry.textures.normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    importer.sRGBTexture=file==entry.textures.@base;
                    importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
                }
                var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.name=entry.name;
                Color col=new Color(entry.color_linear[0],entry.color_linear[1],entry.color_linear[2],entry.color_linear[3]);
                mat.SetColor("_BaseColor", string.IsNullOrEmpty(entry.textures.@base) ? col.gamma : Color.white);
                mat.SetFloat("_Smoothness",1-entry.roughness); mat.SetFloat("_Metallic",entry.metallic);
                if(!string.IsNullOrEmpty(entry.textures.@base)) mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+entry.textures.@base));
                if(!string.IsNullOrEmpty(entry.textures.normal)) { mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+entry.textures.normal)); mat.EnableKeyword("_NORMALMAP"); }
                if(!string.IsNullOrEmpty(entry.textures.data))
                {
                    string packed=Art+entry.textures.data.Replace(".png","_MetallicSmoothness.png");
                    var ti=(TextureImporter)AssetImporter.GetAtPath(packed); ti.sRGBTexture=false; ti.textureCompression=TextureImporterCompression.Uncompressed; ti.SaveAndReimport();
                    mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packed)); mat.SetFloat("_Smoothness",1); mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                AssetDatabase.CreateAsset(mat,"Assets/Prototype/Materials/"+entry.name+".mat"); mats[entry.name]=mat;
            }
            foreach(string name in new[]{"Sunny_v004","Sunny_Walk_v004","Sunny_Run_v004"})
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(Art+name+".fbx");
                importer.animationType=ModelImporterAnimationType.Generic; importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                importer.motionNodeName="SunnyRig"; importer.globalScale=1; importer.importAnimation=true; importer.importBlendShapes=true;
                importer.skinWeights=ModelImporterSkinWeights.Custom; importer.maxBonesPerVertex=32; importer.minBoneWeight=0.00001f;
                importer.animationCompression=ModelImporterAnimationCompression.Off;
                importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                foreach(var pair in mats) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),pair.Key),pair.Value);
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips) { clip.name=name=="Sunny_v004"?"Idle":name.Contains("Walk")?"Walk":"Run"; clip.loopTime=true; clip.loopPose=false; clip.lockRootPositionXZ=true; clip.lockRootHeightY=true; clip.lockRootRotation=true; }
                importer.clipAnimations=clips; importer.SaveAndReimport();
            }
        }
        static AnimatorController CreateAnimator()
        {
            var controller=AnimatorController.CreateAnimatorControllerAtPath("Assets/Prototype/Animation/SunnyLocomotion.controller");
            controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            var tree=new BlendTree { name="Locomotion",blendParameter="Speed",blendType=BlendTreeType.Simple1D,useAutomaticThresholds=false };
            AssetDatabase.AddObjectToAsset(tree,controller);
            foreach(var item in new[]{("Sunny_v004",0f),("Sunny_Walk_v004",2.2f),("Sunny_Run_v004",4.2f)})
            {
                var clip=AssetDatabase.LoadAllAssetsAtPath(Art+item.Item1+".fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
                tree.AddChild(clip,item.Item2);
            }
            var state=controller.layers[0].stateMachine.AddState("Locomotion"); state.motion=tree; controller.layers[0].stateMachine.defaultState=state;
            return controller;
        }
        static Material Flat(string name,Color color)
        {
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.name=name; mat.SetColor("_BaseColor",color); mat.SetFloat("_Smoothness",0.1f);
            AssetDatabase.CreateAsset(mat,"Assets/Prototype/Materials/Graybox_"+name+".mat"); return mat;
        }
        static GameObject Cube(string name,Vector3 position,Vector3 scale,Material material,Transform parent,bool collision=true)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(parent); obj.transform.position=position; obj.transform.localScale=scale; obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!collision) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }
        static void Sign(string label,Vector3 position,Transform parent)
        {
            var obj=new GameObject(label+" Sign"); obj.transform.SetParent(parent); obj.transform.position=position;
            var text=obj.AddComponent<TextMesh>(); text.text=label; text.fontSize=48; text.characterSize=0.035f; text.anchor=TextAnchor.MiddleCenter; text.color=new Color(0.95f,0.95f,0.85f);
        }
        static void SetLayer(GameObject obj,int layer) { obj.layer=layer; foreach(Transform child in obj.transform) SetLayer(child.gameObject,layer); }
    }
}
