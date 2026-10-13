using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.FirstPersonEditor;

namespace BusinessAsUsual.Town.Editor
{
    public static class Phase7BBuilder
    {
        const string Root = "Assets/Town/Phase7B/";
        static T Save<T>(T value,string path) where T:UnityEngine.Object
        {
            value.name=Path.GetFileNameWithoutExtension(path);
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}
            AssetDatabase.CreateAsset(value,path);return value;
        }
        static Material Material(string name)
        {
            string path=Root+"Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+name+"/base_color.png"));
            mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+name+"/normal.png"));
            mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.4f);
            mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",.12f);mat.enableInstancing=true;
            mat.color=name=="Tree"?new Color(.64f,.86f,.58f):new Color(.90f,.88f,.83f);
            EditorUtility.SetDirty(mat);return mat;
        }
        static Mesh Model(string name)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/"+name+".fbx");
            if(!source)throw new Exception("Missing Meshy FBX: "+name);
            var go=UnityEngine.Object.Instantiate(source);
            var combine=go.GetComponentsInChildren<MeshFilter>().Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray();
            var mesh=new Mesh{name=name};mesh.CombineMeshes(combine,true,true);mesh.RecalculateBounds();
            UnityEngine.Object.DestroyImmediate(go);return Save(mesh,Root+"Meshes/"+name+".asset");
        }
        struct Vertex { public Vector3 p,n;public Vector2 uv;public static Vertex Lerp(Vertex a,Vertex b,float t)=>new Vertex{p=Vector3.Lerp(a.p,b.p,t),n=Vector3.Lerp(a.n,b.n,t).normalized,uv=Vector2.Lerp(a.uv,b.uv,t)}; }
        static Mesh Clip(Mesh mesh,float height,bool above)
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var uvs=new List<Vector2>();var submeshes=new List<int[]>();
            var p=mesh.vertices;var n=mesh.normals;var uv=mesh.uv;
            for(int sub=0;sub<mesh.subMeshCount;sub++)
            {
                var indices=new List<int>();var triangles=mesh.GetTriangles(sub);
                for(int i=0;i<triangles.Length;i+=3)
                {
                    var poly=new List<Vertex>();for(int j=0;j<3;j++){int k=triangles[i+j];poly.Add(new Vertex{p=p[k],n=n.Length>k?n[k]:Vector3.up,uv=uv.Length>k?uv[k]:Vector2.zero});}
                    var clipped=new List<Vertex>();
                    for(int j=0;j<poly.Count;j++)
                    {
                        var a=poly[j];var b=poly[(j+1)%poly.Count];bool ai=above?a.p.y>=height:a.p.y<=height,bi=above?b.p.y>=height:b.p.y<=height;
                        if(ai)clipped.Add(a);if(ai!=bi)clipped.Add(Vertex.Lerp(a,b,(height-a.p.y)/(b.p.y-a.p.y)));
                    }
                    int first=vertices.Count;foreach(var v in clipped){vertices.Add(v.p);normals.Add(v.n);uvs.Add(v.uv);}
                    for(int j=1;j+1<clipped.Count;j++){indices.Add(first);indices.Add(first+j);indices.Add(first+j+1);}
                }
                submeshes.Add(indices.ToArray());
            }
            var result=new Mesh{indexFormat=IndexFormat.UInt32};result.SetVertices(vertices);result.SetNormals(normals);result.SetUVs(0,uvs);result.subMeshCount=submeshes.Count;
            for(int s=0;s<submeshes.Count;s++)result.SetTriangles(submeshes[s],s);result.RecalculateBounds();result.RecalculateTangents();return result;
        }
        static void Hybrid(string original,string generated)
        {
            var go=GameObject.Find("Daylight Modeled Neighborhood/"+original);
            var originalPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Town/Prefabs/Environment/Addendum/"+original+".prefab");
            var lower=Clip(AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Town/Art/Processed/Addendum/"+original+".asset"),3.02f,false);
            var upper=Clip(Model(generated),3.02f,true);
            var parts=new List<CombineInstance>();for(int s=0;s<lower.subMeshCount;s++)parts.Add(new CombineInstance{mesh=lower,subMeshIndex=s,transform=Matrix4x4.identity});
            parts.Add(new CombineInstance{mesh=upper,transform=Matrix4x4.identity});var mesh=new Mesh{name=original+"_MeshyHybrid",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(parts.ToArray(),false,true);
            go.GetComponent<MeshFilter>().sharedMesh=Save(mesh,Root+"Meshes/"+original+"Hybrid.asset");
            go.GetComponent<Renderer>().sharedMaterials=originalPrefab.GetComponent<Renderer>().sharedMaterials.Concat(new[]{Material(generated)}).ToArray();
            UnityEngine.Object.DestroyImmediate(lower);UnityEngine.Object.DestroyImmediate(upper);
        }
        static GameObject Prop(string name,Vector3 position,Transform parent,Vector3? scale=null,float yaw=0)
        {
            var go=new GameObject("Meshy "+name);go.transform.SetParent(parent,false);go.transform.position=position;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=scale??Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Meshes/"+name+".asset");go.AddComponent<MeshRenderer>().sharedMaterial=Material(name);return go;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 size,Transform parent,Material material)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,true);go.transform.position=pos;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;return go;}
        [MenuItem("Business as Usual/Phase 7B/Apply Environment and Presentation")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop before applying Phase7B.");
            Directory.CreateDirectory(Root+"Materials");Directory.CreateDirectory(Root+"Meshes");
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles(Root+"Textures","*.png",SearchOption.AllDirectories))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=path.EndsWith("normal.png")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=path.EndsWith("base_color.png");importer.maxTextureSize=2048;importer.anisoLevel=4;importer.filterMode=FilterMode.Trilinear;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            }
            EditorSceneManager.OpenScene(Phase7ABuilder.Scene);
            var existing=GameObject.Find("Phase 7B Living Town");if(existing)UnityEngine.Object.DestroyImmediate(existing);
            var root=new GameObject("Phase 7B Living Town").transform;
            Hybrid("MarketShell","Market");Hybrid("WorkshopShell","Garage");
            // Generated roof undersides can have irregular eaves. Fit a clean ceiling and
            // upper wall band to the retained collision footprint, closing every seam.
            foreach(float x in new[]{-8f,8f})
            {
                var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/Town/Materials/"+(x<0?"Plaster":"Blue")+".mat");
                Box("Upper rear wall band",new Vector3(x,3.08f,9.47f),new Vector3(6.86f,.56f,.16f),root,wall);
                foreach(float side in new[]{-1f,1f})Box("Upper side wall band",new Vector3(x+side*3.47f,3.08f,6.5f),new Vector3(.16f,.56f,5.94f),root,wall);
                Box("Clean workplace ceiling",new Vector3(x,3.25f,6.5f),new Vector3(6.86f,.08f,5.94f),root,AssetDatabase.LoadAssetAtPath<Material>("Assets/Town/Materials/Plaster.mat"));
            }
            foreach(var n in new[]{"Cottage","Tree","Lamp"})Model(n);
            var neighborhood=GameObject.Find("Daylight Modeled Neighborhood").transform;
            foreach(var house in neighborhood.Cast<Transform>().Where(t=>t.name=="TownHouseA"||t.name=="TownHouseB"))
            {
                foreach(var r in house.GetComponentsInChildren<Renderer>())r.enabled=false;
                float yaw=house.position.z>14?180:house.position.z<-14?0:house.position.x>0?270:90;
                Prop("Cottage",house.position,root,new Vector3(1,.88f,1),yaw);
            }
            var old=GameObject.Find("Original Daylight Neighborhood").transform;
            var trees=new List<Transform>();var lights=new List<Light>();var windows=new List<Renderer>();
            foreach(var t in old.GetComponentsInChildren<Transform>().Where(t=>t.name=="Tree"||t.name=="Lamp").ToArray())
            {
                foreach(var r in t.GetComponentsInChildren<Renderer>())r.enabled=false;
                var prop=Prop(t.name,t.position,root, t.name=="Tree"?Vector3.one*.88f:Vector3.one);
                if(t.name=="Tree")trees.Add(prop.transform);
                else
                {
                    var l=new GameObject("Warm evening light");l.transform.SetParent(prop.transform,false);l.transform.localPosition=new Vector3(0,2.65f,0);var light=l.AddComponent<Light>();light.type=LightType.Point;light.range=6.5f;light.color=new Color(1,.67f,.30f);light.shadows=LightShadows.None;lights.Add(light);
                    var collision=prop.AddComponent<CapsuleCollider>();collision.radius=.13f;collision.height=2.7f;collision.center=Vector3.up*1.35f;
                }
            }
            // Dedicated window glows never reveal occupants or private objectives.
            var glow=new Material(Shader.Find("Universal Render Pipeline/Lit"));glow.color=new Color(.25f,.28f,.23f);glow.EnableKeyword("_EMISSION");glow.SetFloat("_Smoothness",.1f);glow=Save(glow,Root+"Materials/WindowGlow.mat");
            foreach(var house in root.Cast<Transform>().Where(t=>t.name=="Meshy Cottage").ToArray())
            {
                var fit=house.gameObject.AddComponent<MeshCollider>();fit.sharedMesh=house.GetComponent<MeshFilter>().sharedMesh;Physics.SyncTransforms();
                var ray=new Ray(house.TransformPoint(new Vector3(0,3.4f,4)), -house.forward);
                if(fit.Raycast(ray,out var hit,10))
                {
                    var pane=Box("Evening window",hit.point+house.forward*.012f,new Vector3(.5f,.65f,.012f),root,glow);pane.transform.rotation=house.rotation;
                    UnityEngine.Object.DestroyImmediate(pane.GetComponent<Collider>());var renderer=pane.GetComponent<Renderer>();renderer.enabled=false;windows.Add(renderer);
                }
                UnityEngine.Object.DestroyImmediate(fit);
            }
            var cloudMat=new Material(Shader.Find("Business as Usual/Soft Town Cloud"));cloudMat=Save(cloudMat,Root+"Materials/Cloud.mat");var clouds=new List<Transform>();
            for(int i=0;i<8;i++)
            {
                var cluster=new GameObject("Drifting cloud "+i).transform;cluster.SetParent(root,false);cluster.localPosition=new Vector3(i*13-35,22+i%3*3, i%2==0?22:-24);clouds.Add(cluster);
                for(int j=0;j<4;j++){var puff=GameObject.CreatePrimitive(PrimitiveType.Sphere);puff.name="Soft cloud lobe";puff.transform.SetParent(cluster,false);puff.transform.localPosition=new Vector3(j*2.4f-3.6f,j==1||j==2?.6f:0,0);puff.transform.localScale=new Vector3(5,2.4f+(j%2)*.6f,3.5f);UnityEngine.Object.DestroyImmediate(puff.GetComponent<Collider>());puff.GetComponent<Renderer>().sharedMaterial=cloudMat;puff.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
            }
            var atmosphere=root.gameObject.AddComponent<TownAtmosphere>();atmosphere.sun=GameObject.Find("Sun").GetComponent<Light>();atmosphere.lamps=lights.ToArray();atmosphere.litWindows=windows.ToArray();atmosphere.clouds=clouds.ToArray();atmosphere.trees=trees.ToArray();
            var sky=new Material(Shader.Find("Business as Usual/Town Sky"));RenderSettings.skybox=Save(sky,Root+"Materials/Sky.mat");RenderSettings.sun=atmosphere.sun;
            atmosphere.sun.shadowBias=.04f;atmosphere.sun.shadowNormalBias=.2f;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.fog=true;RenderSettings.fogStartDistance=40;RenderSettings.fogEndDistance=90;atmosphere.Apply(0);
            // Loading footprints are visual markings, preserving functional stations and routes.
            var paint=AssetDatabase.LoadAssetAtPath<Material>("Assets/Town/Materials/Mustard.mat");
            foreach(var p in new[]{new Vector3(-10.8f,.053f,10.7f),new Vector3(9.5f,.055f,-5.2f)})
                foreach(float s in new[]{-1f,1f}){var line=Box("Delivery staging line",p+Vector3.right*s*.65f,new Vector3(.05f,.008f,1.2f),root,paint);UnityEngine.Object.DestroyImmediate(line.GetComponent<Collider>());}
            ConfigurePlayer();
            // Include Performance on Windows; otherwise Unity strips it and renumbers High to zero.
            var quality=new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings"));
            quality.FindProperty("m_QualitySettings").GetArrayElementAtIndex(0).FindPropertyRelative("excludedTargetPlatforms").ClearArray();quality.ApplyModifiedProperties();
            // The Performance preset inherited an 80% internal scale. Both presets now
            // render native pixels; their shadow/light budgets still distinguish them.
            foreach(var path in new[]{"Assets/Settings/PC_RPAsset.asset","Assets/Settings/Mobile_RPAsset.asset"})
            {var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);pipeline.renderScale=1;EditorUtility.SetDirty(pipeline);}
            foreach(var path in new[]{"Assets/Settings/PC_Renderer.asset","Assets/Settings/Mobile_Renderer.asset"})
            {
                var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                if(!renderer.rendererFeatures.OfType<NativeRenderProbe>().Any())
                {var probe=ScriptableObject.CreateInstance<NativeRenderProbe>();probe.name="Native resolution evidence";AssetDatabase.AddObjectToAsset(probe,renderer);renderer.rendererFeatures.Add(probe);EditorUtility.SetDirty(renderer);}
            }
            foreach(var mat in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Town/Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))))
                if(mat.shader.name=="Universal Render Pipeline/Lit"){mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",mat.name=="Glass"?.22f:.10f);EditorUtility.SetDirty(mat);}
            foreach(var path in new[]{"Assets/Town/Materials/Asphalt.mat","Assets/Town/Materials/Paving.mat"}){var mat=AssetDatabase.LoadAssetAtPath<Material>(path);mat.color=path.Contains("Asphalt")?new Color(.68f,.70f,.67f):new Color(.83f,.81f,.73f);EditorUtility.SetDirty(mat);}
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Phase7ABuilder.BakeMinimapMenu();
            Debug.Log("[Phase7B] Meshy hybrids, houses, vegetation, evening lights, clouds and staging integrated.");
        }
        static void ConfigurePlayer()
        {
            // Bake proportioned hands in the Editor. Runtime never reads a non-readable
            // imported mesh; the original Sunny export and its importer stay unchanged.
            var arms=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art/Sunny/Sunny_v004.fbx"));
            arms.name="Sunny first-person arms";
            foreach(var r in arms.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.enabled=r.name.StartsWith("ForearmHand");if(!r.enabled)continue;
                var mesh=FirstPersonHands.RefinedMesh(r,.65f);mesh.hideFlags=HideFlags.None;r.sharedMesh=Save(mesh,Root+"Meshes/"+r.name+"Viewmodel.asset");
            }
            var preparedArms=PrefabUtility.SaveAsPrefabAsset(arms,Root+"SunnyFirstPersonArms.prefab");UnityEngine.Object.DestroyImmediate(arms);
            var avatar=AssetDatabase.LoadAllAssetsAtPath("Assets/Prototype/Art/Sunny/Sunny_v004.fbx").OfType<Avatar>().First();
            foreach(var clip in new[]{"Walk","Run"})
            {
                string path="Assets/FirstPerson/Art/Animation/Sunny_"+clip+"_Phase7B.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CopyFromOther;importer.sourceAvatar=avatar;importer.animationCompression=ModelImporterAnimationCompression.Off;
                var settings=importer.defaultClipAnimations;foreach(var c in settings){c.name=clip+" Phase7B";c.loopTime=true;c.keepOriginalPositionY=true;c.keepOriginalPositionXZ=true;c.keepOriginalOrientation=true;}importer.clipAnimations=settings;importer.SaveAndReimport();
            }
            var source=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Prototype/Animation/SunnyLocomotion.controller");
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(Root+"SunnyTownLocomotion.overrideController");if(!controller){controller=new AnimatorOverrideController(source);AssetDatabase.CreateAsset(controller,Root+"SunnyTownLocomotion.overrideController");}
            controller.runtimeAnimatorController=source;var overrides=new List<KeyValuePair<AnimationClip,AnimationClip>>();controller.GetOverrides(overrides);
            Directory.CreateDirectory(Root+"Animation");
            for(int i=0;i<overrides.Count;i++)
            {
                var key=overrides[i].Key;string word=key.name.Contains("Walk")?"Walk":key.name.Contains("Run")?"Run":null;if(word==null)continue;
                var imported=AssetDatabase.LoadAllAssetsAtPath("Assets/FirstPerson/Art/Animation/Sunny_"+word+"_Phase7B.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
                var value=UnityEngine.Object.Instantiate(imported);
                // Exported armature root tracks bind to Sunny Visual. Even constant tracks
                // enter the controller's write-default set and reset idle network facing.
                foreach(var binding in AnimationUtility.GetCurveBindings(value))
                {
                    if(binding.path==""&&binding.type==typeof(Transform)){AnimationUtility.SetEditorCurve(value,binding,null);continue;}
                    // An armature-only FBX roots at SunnyRig; the actual player Animator is
                    // on its parent Sunny Visual. Bind the retained bones to that hierarchy.
                    if(binding.path.StartsWith("mixamorig:"))
                    {
                        var curve=AnimationUtility.GetEditorCurve(value,binding);AnimationUtility.SetEditorCurve(value,binding,null);
                        var fitted=binding;fitted.path="SunnyRig/"+binding.path;AnimationUtility.SetEditorCurve(value,fitted,curve);
                    }
                }
                value=Save(value,Root+"Animation/Sunny"+word+".anim");overrides[i]=new KeyValuePair<AnimationClip,AnimationClip>(key,value);
            }
            controller.ApplyOverrides(overrides);EditorUtility.SetDirty(controller);
            var prefab=PrefabUtility.LoadPrefabContents(Phase7ABuilder.PlayerPrefab);
            try
            {
                var hands=prefab.GetComponent<FirstPersonHands>();hands.armsSource=preparedArms;hands.proportionsPrepared=true;hands.rigScale=1f;hands.handProportion=.65f;hands.shoulderOffset=new Vector3(0,-.30f,.30f);hands.restRight=new Vector3(.24f,-.25f,.46f);hands.restLeft=new Vector3(-.24f,-.25f,.46f);
                var presentation=prefab.GetComponent<AvatarPresentation>();presentation.walkCadence=2.2f/(.26f/(.6f*(20f/60)));presentation.runCadence=4.2f/(.32f/(.4f*(16f/60)));
                var animator=prefab.GetComponentInChildren<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
                foreach(var camera in prefab.GetComponentsInChildren<Camera>(true)){camera.allowDynamicResolution=false;camera.allowMSAA=true;camera.clearFlags=CameraClearFlags.Skybox;}
                PrefabUtility.SaveAsPrefabAsset(prefab,Phase7ABuilder.PlayerPrefab);
            }finally{PrefabUtility.UnloadPrefabContents(prefab);}
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Business as Usual/Phase 7B/Build Windows Town")]
        public static void Build()
        {
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/Phase7B/BusinessAsUsual.exe"));Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Phase7ABuilder.Scene,"Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("[Phase7B] Build "+report.summary.result+" errors="+report.summary.totalErrors);
        }
        [MenuItem("Business as Usual/Phase 7B/Build Phase 5 Regression")]
        public static void BuildPhase5Regression()
        {
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/Phase7B-Phase5Regression/BusinessAsUsual.exe"));Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Multiplayer/Scenes/ShopkeeperMultiplayer.unity","Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("[Phase7B] Shop regression build "+report.summary.result+" errors="+report.summary.totalErrors);
        }
    }
}
