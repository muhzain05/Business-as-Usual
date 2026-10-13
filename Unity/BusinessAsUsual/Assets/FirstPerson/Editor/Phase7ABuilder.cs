using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using BusinessAsUsual.FirstPerson;

namespace BusinessAsUsual.FirstPersonEditor
{
    // Repeatable Phase 7A scene/prefab changes. Each step is idempotent and logs what it touched.
    public static class Phase7ABuilder
    {
        public const string Scene = "Assets/Town/Scenes/DaylightTown.unity";
        public const string PlayerPrefab = "Assets/Town/Prefabs/SunnyTownPlayer.prefab";
        public const string SunnyModel = "Assets/Prototype/Art/Sunny/Sunny_v004.fbx";
        const string ArtFolder = "Assets/FirstPerson/Art";
        const string MapFolder = "Assets/Town/Resources/Town/Map";

        [MenuItem("Business as Usual/Phase 7A/Apply All (prefab, collision, vehicles, minimap)")]
        public static void ApplyAll()
        {
            var log = new StringBuilder();
            ConfigurePlayer(log);
            var scene = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            FixCollision(log);
            GroundVehicles(log);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            BakeMinimap(log);
            Debug.Log("[Phase7A] " + log);
        }

        [MenuItem("Business as Usual/Phase 7A/Configure First-person Player Prefab")]
        public static void ConfigurePlayerMenu() { var log = new StringBuilder(); ConfigurePlayer(log); Debug.Log("[Phase7A] " + log); }

        public static void ConfigurePlayer(StringBuilder log)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefab);
            try
            {
                T Ensure<T>() where T : Component { var c = root.GetComponent<T>(); if (!c) { c = root.AddComponent<T>(); log.Append("added " + typeof(T).Name + "; "); } return c; }
                Ensure<FirstPersonView>();
                // Re-added each run so the prefab always carries the current tuned code defaults.
                var oldHands = root.GetComponent<FirstPersonHands>(); if (oldHands) Object.DestroyImmediate(oldHands, true);
                var hands = root.AddComponent<FirstPersonHands>();
                hands.armsSource = AssetDatabase.LoadAssetAtPath<GameObject>(SunnyModel);
                hands.toolMetal = ToolMaterial("Tool_Metal", new Color(.62f, .66f, .66f), .55f, .55f);
                hands.toolHandle = ToolMaterial("Tool_Handle", new Color(.937f, .784f, .392f), 0, .35f); // handbook mustard
                root.GetComponent<FirstPersonView>().carryOffset = new Vector3(0, .5f, .5f);
                Ensure<PauseMenu>();
                Ensure<AvatarPresentation>();
                var camera = root.GetComponentInChildren<Camera>(true);
                camera.nearClipPlane = .03f;
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefab);
                log.Append("player prefab saved; ");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static GameObject Find(string path) => GameObject.Find(path);

        static Material ToolMaterial(string name, Color color, float metallic, float smoothness)
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Metallic", metallic); mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void SetBox(string path, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, StringBuilder log)
        {
            var go = Find(path); if (!go) { log.Append("MISSING " + path + "; "); return; }
            var box = go.GetComponent<BoxCollider>();
            Undo.RecordObject(box, "Phase 7A collider"); Undo.RecordObject(go.transform, "Phase 7A collider");
            // Graybox walls are unrotated cubes with box size 1; express the fitted world box through the transform.
            go.transform.position = new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, (zMin + zMax) / 2);
            go.transform.localScale = new Vector3(xMax - xMin, yMax - yMin, zMax - zMin);
            box.center = Vector3.zero; box.size = Vector3.one;
            log.Append("fit " + go.name + "; ");
        }

        public static void FixCollision(StringBuilder log)
        {
            // 1. Market doorway: the modeled MarketShell frame opening spans x -8.91..-7.49 (centre -8.20),
            //    but the functional door, its collision gap and entry trigger were centred at -7.80.
            //    Players could walk into the visible wall east of the door. Move gameplay to the art.
            var door = Find("Graybox Town/Shop/Shop Door/Hinge");
            if (door && Mathf.Abs(door.transform.position.x + 8.5f) < .01f)
            {
                Undo.RecordObject(door.transform, "Phase 7A door"); door.transform.position += Vector3.left * .4f; log.Append("door moved to frame; ");
                var portal = Find("Town Workplaces/Enter Daylight Market");
                if (portal) { Undo.RecordObject(portal.transform, "Phase 7A portal"); portal.transform.position += Vector3.left * .4f; }
            }
            // The door's interaction point (Handle) kept an offset authored for the old scaled graybox
            // panel (1.4 x 2.2 x 0.12). After the panel scale became 1 it floated 1.25 m off the door,
            // so an open door's point sat in the vestibule and captured first-person aim/E.
            var handle = Find("Graybox Town/Shop/Shop Door/Hinge/Door Panel/Handle");
            if (handle && handle.transform.localPosition.z < -.5f)
            {
                Undo.RecordObject(handle.transform, "Phase 7A door handle");
                handle.transform.localPosition = new Vector3(.45f, -.15f, -.12f); handle.transform.localScale = Vector3.one;
                log.Append("door handle point moved onto the leaf; ");
            }
            // The door art was mirrored: its handle sat on the hinge side. Flip the visual only
            // (the Door Panel collider is untouched) so the handle is on the swinging edge.
            var doorArt = Find("Graybox Town/Shop/Shop Door/Hinge/Door Panel/Door");
            if (doorArt && doorArt.transform.localScale.x > 0)
            {
                Undo.RecordObject(doorArt.transform, "Phase 7A door art");
                var scale = doorArt.transform.localScale; scale.x = -scale.x; doorArt.transform.localScale = scale;
                log.Append("door art mirrored so the handle is on the free edge; ");
            }
            // Front walls fitted to the modeled facade: outer face z 3.20 (pilasters 3.375), inner 3.65.
            SetBox("Graybox Town/Shop/Shop Front Left", -11.65f, -8.91f, 0, 3, 3.2f, 3.65f, log);
            SetBox("Graybox Town/Shop/Shop Front Right", -7.49f, -4.35f, 0, 3, 3.2f, 3.65f, log);
            SetBox("Graybox Town/Shop/Shop Door Lintel", -8.91f, -7.49f, 2.27f, 3, 3.2f, 3.65f, log);
            // Garage walls matched to the measured WorkshopShell inner surfaces (4.61 / 11.39 / 9.38).
            SetBox("Graybox Town/Garage/Garage West", 4.35f, 4.62f, 0, 3, 3.5f, 9.5f, log);
            SetBox("Graybox Town/Garage/Garage East", 11.38f, 11.65f, 0, 3, 3.5f, 9.5f, log);
            SetBox("Graybox Town/Garage/Garage Rear", 4.5f, 11.5f, 0, 3, 9.37f, 9.65f, log);
            SetBox("Graybox Town/Garage/Garage Front Left", 4.5f, 5.95f, 0, 3, 3.33f, 3.67f, log);
            SetBox("Graybox Town/Garage/Garage Front Right", 10.05f, 11.5f, 0, 3, 3.33f, 3.67f, log);
            SetBox("Graybox Town/Garage/Garage Lintel", 5.95f, 10.05f, 2.2f, 3, 3.33f, 3.67f, log);

            // 2. Phase 6 modeled props that never received collision volumes.
            string[] props = { "WasteCart", "Crate", "Workbench", "WasteBin", "ToolRack", "Yard canopy post", "ParcelCart", "Square municipal signpost" };
            var interactionPoints = Object.FindObjectsByType<BusinessAsUsual.Prototype.Interactable>(FindObjectsSortMode.None).Select(i => i.InteractionPoint).ToArray();
            int added = 0;
            foreach (var parent in new[] { "Original Daylight Neighborhood", "Daylight Modeled Neighborhood" })
            {
                var root = Find(parent); if (!root) continue;
                foreach (Transform t in root.transform)
                {
                    if (!props.Contains(t.name) || t.GetComponent<Collider>()) continue;
                    var r = t.GetComponent<Renderer>(); if (!r || !r.enabled) continue;
                    var box = Undo.AddComponent<BoxCollider>(t.gameObject);
                    // Fit in local space from the mesh, shrunk slightly so contact reads as touching.
                    var mf = t.GetComponent<MeshFilter>(); var b = mf && mf.sharedMesh ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
                    box.center = b.center; box.size = b.size * .98f;
                    if (t.name == "Square municipal signpost") { box.center = new Vector3(b.center.x, b.center.y, b.center.z); box.size = new Vector3(Mathf.Min(b.size.x, .3f), b.size.y, Mathf.Min(b.size.z, .3f)); }
                    // Props that host an interaction point must not block its line of sight: keep them
                    // solid for movement but out of interaction/camera rays (Ignore Raycast layer).
                    var world = box.bounds; world.Expand(.3f);
                    if (interactionPoints.Any(p => world.Contains(p))) { t.gameObject.layer = 2; log.Append(t.name + "@" + t.position.ToString("F1") + " ignore-raycast; "); }
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, GameObjectUtility.GetStaticEditorFlags(t.gameObject));
                    added++;
                }
            }
            log.Append("prop colliders added: " + added + "; ");
        }

        public static void GroundVehicles(StringBuilder log)
        {
            var shadow = ContactShadowMaterial();
            for (int i = 0; i < 2; i++)
            {
                var root = Find("Town Workplaces/Service Vehicle " + i); if (!root) continue;
                var van = root.transform.Find("ServiceVan"); if (!van) continue;
                const float floor = .05f; // Workshop concrete floor top.
                Undo.RecordObject(root.transform, "Phase 7A vehicle"); Undo.RecordObject(van, "Phase 7A vehicle");
                var world = van.position; // keep the van's horizontal placement
                root.transform.position = new Vector3(world.x, floor, world.z);
                var mesh = van.GetComponent<MeshFilter>().sharedMesh;
                // Uniform scale and tyres resting 4 mm into the floor so they read as loaded.
                float s = .85f; van.localScale = Vector3.one * s;
                van.localPosition = new Vector3(0, -mesh.bounds.min.y * s - .004f, 0);
                van.localRotation = Quaternion.identity;
                var box = root.GetComponent<BoxCollider>(); Undo.RecordObject(box, "Phase 7A vehicle");
                root.transform.localScale = Vector3.one;
                var b = mesh.bounds;
                box.center = new Vector3(b.center.x * s, (b.center.y - b.min.y) * s, b.center.z * s);
                box.size = new Vector3(b.size.x * s * .97f, b.size.y * s, b.size.z * s * .98f);
                var cube = root.GetComponent<MeshRenderer>(); if (cube) cube.enabled = false;
                var blob = root.transform.Find("Contact shadow");
                if (!blob)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Quad); go.name = "Contact shadow";
                    Object.DestroyImmediate(go.GetComponent<Collider>());
                    Undo.RegisterCreatedObjectUndo(go, "Phase 7A contact shadow");
                    go.transform.SetParent(root.transform, false); blob = go.transform;
                }
                blob.localPosition = new Vector3(b.center.x * s, .004f, b.center.z * s);
                blob.localRotation = Quaternion.Euler(90, 0, 0);
                blob.localScale = new Vector3(b.size.x * s * 1.18f, b.size.z * s * 1.1f, 1);
                var mr = blob.GetComponent<MeshRenderer>(); mr.sharedMaterial = shadow; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                log.Append("vehicle " + i + " grounded (tyre bottom y=" + (floor + van.localPosition.y + mesh.bounds.min.y * s).ToString("F3") + ", collider top=" + (floor + box.center.y + box.size.y / 2).ToString("F2") + "); ");
            }
        }

        static Material ContactShadowMaterial()
        {
            Directory.CreateDirectory(ArtFolder);
            string texPath = ArtFolder + "/ContactShadow.png", matPath = ArtFolder + "/ContactShadow.mat";
            if (!File.Exists(texPath))
            {
                const int size = 128; var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    // Rounded-rectangle falloff: darkest under the chassis, soft toward the wheels.
                    float u = Mathf.Abs(x / (size - 1f) * 2 - 1), v = Mathf.Abs(y / (size - 1f) * 2 - 1);
                    float d = Mathf.Pow(Mathf.Pow(u, 4) + Mathf.Pow(v, 4), .25f);
                    float a = Mathf.Clamp01(1 - Mathf.SmoothStep(.55f, 1f, d)) * .62f;
                    tex.SetPixel(x, y, new Color(.07f, .12f, .12f, a));
                }
                File.WriteAllBytes(texPath, tex.EncodeToPNG()); Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(texPath); importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false; importer.SaveAndReimport();
            }
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.SetFloat("_Surface", 1); mat.SetFloat("_Blend", 0); mat.SetFloat("_ZWrite", 0);
                mat.SetOverrideTag("RenderType", "Transparent"); mat.renderQueue = (int)RenderQueue.Transparent - 10;
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath)); mat.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Separate output folders keep the accepted Phase 5/6 builds and evidence untouched.
        [MenuItem("Business as Usual/Phase 7A/Build Windows Town (Builds/Phase7A)")]
        public static void BuildTown() => Build("Builds/Phase7A/BusinessAsUsual.exe", Scene, "Assets/Shopkeeper/Scenes/ShopkeeperShift.unity", "Assets/Prototype/Scenes/TownPrototype.unity");
        [MenuItem("Business as Usual/Phase 7A/Build Phase 5 Shop Regression (Builds/Phase7A-Phase5Regression)")]
        public static void BuildPhase5Regression() => Build("Builds/Phase7A-Phase5Regression/BusinessAsUsual.exe", "Assets/Multiplayer/Scenes/ShopkeeperMultiplayer.unity", "Assets/Shopkeeper/Scenes/ShopkeeperShift.unity", "Assets/Prototype/Scenes/TownPrototype.unity");
        static void Build(string relative, params string[] scenes)
        {
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../" + relative)); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            Debug.Log("[Phase7A] build " + relative + ": " + report.summary.result + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " size=" + report.summary.totalSize);
        }

        [MenuItem("Business as Usual/Phase 7A/Bake Minimap")]
        public static void BakeMinimapMenu() { var log = new StringBuilder(); BakeMinimap(log); Debug.Log("[Phase7A] " + log); }

        // One-time editor bake into stylized paper images. Runtime never re-renders them.
        public static void BakeMinimap(StringBuilder log)
        {
            if (EditorSceneManager.GetActiveScene().path != Scene) EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            Directory.CreateDirectory(MapFolder);
            Bake(TownMinimap.TownWorld, 40, 24, MapFolder + "/DaylightTownMap.png", log);
            // Below the ceiling (3.14 m) so the market reads as a floor plan.
            Bake(TownMinimap.MarketWorld, 2.9f, 40, MapFolder + "/DaylightMarketMap.png", log);
        }

        static void Bake(Rect world, float height, int pixelsPerMetre, string path, StringBuilder log)
        {
            int w = Mathf.RoundToInt(world.width * pixelsPerMetre), h = Mathf.RoundToInt(world.height * pixelsPerMetre);
            var go = new GameObject("Phase 7A map camera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = world.height / 2; cam.aspect = world.width / world.height;
            cam.transform.SetPositionAndRotation(new Vector3(world.center.x, height, world.center.y), Quaternion.Euler(90, 0, 0));
            cam.nearClipPlane = .01f; cam.farClipPlane = height + 2;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.83f, .78f, .66f);
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            // Hide runtime-only and text clutter (world signs) so the map stays legible.
            var hidden = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.enabled && (r.name.Contains("/ sign") || r.name.StartsWith("notice-") || r is SkinnedMeshRenderer)).ToArray();
            foreach (var r in hidden) r.enabled = false;
            cam.Render();
            foreach (var r in hidden) r.enabled = true;
            RenderTexture.active = rt;
            var src = new Texture2D(w, h, TextureFormat.RGB24, false); src.ReadPixels(new Rect(0, 0, w, h), 0, 0); src.Apply();
            RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
            var output = Stylize(src);
            File.WriteAllBytes(path, output.EncodeToPNG());
            Object.DestroyImmediate(src); Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default; importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.sRGBTexture = true;
            importer.npotScale = TextureImporterNPOTScale.None; // keep exact world-to-pixel mapping
            importer.SaveAndReimport();
            log.Append("baked " + Path.GetFileName(path) + " " + w + "x" + h + "; ");
        }

        // Paper look: soften colour toward the handbook palette, then ink the luminance edges.
        static Texture2D Stylize(Texture2D src)
        {
            int w = src.width, h = src.height; var px = src.GetPixels(); var lum = new float[px.Length];
            for (int i = 0; i < px.Length; i++) lum[i] = px[i].r * .3f + px[i].g * .59f + px[i].b * .11f;
            Color paper = new Color(.965f, .914f, .788f), ink = new Color(.141f, .251f, .278f);
            var outPx = new Color[px.Length];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                float gx = 0, gy = 0;
                if (x > 0 && x < w - 1 && y > 0 && y < h - 1)
                {
                    gx = -lum[i - w - 1] - 2 * lum[i - 1] - lum[i + w - 1] + lum[i - w + 1] + 2 * lum[i + 1] + lum[i + w + 1];
                    gy = -lum[i - w - 1] - 2 * lum[i - w] - lum[i - w + 1] + lum[i + w - 1] + 2 * lum[i + w] + lum[i + w + 1];
                }
                float edge = Mathf.Clamp01((Mathf.Sqrt(gx * gx + gy * gy) - .12f) * 2.2f);
                Color c = px[i];
                float g = lum[i];
                // Posterize into four tones, tinted from the source hue.
                float tone = Mathf.Round(g * 4) / 4f;
                Color tinted = Color.Lerp(new Color(tone, tone, tone), c, .45f);
                Color baseColor = Color.Lerp(paper, tinted * new Color(1.02f, .98f, .9f), .55f);
                outPx[i] = Color.Lerp(baseColor, ink, edge * .85f);
            }
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.SetPixels(outPx); tex.Apply();
            return tex;
        }
    }
}
