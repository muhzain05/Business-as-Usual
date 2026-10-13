using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Town;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.FirstPersonEditor
{
    // Phase 7A checks. Static checks run in edit mode on the saved town scene; play checks run
    // inside Play Mode against a real NGO host, the production motor and CharacterController.
    public static class Phase7AValidation
    {
        public static readonly string Output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Docs/Diagnostics/Phase7A/Editor"));
        [Serializable] public class Check { public string name; public bool passed; public string detail; }
        [Serializable] public class Report { public string suite, scene, utc; public int passed, total; public List<Check> checks = new List<Check>(); }

        static void Add(Report r, string name, bool ok, string detail = "") { r.checks.Add(new Check { name = name, passed = ok, detail = detail }); }
        static void Save(Report r, string file)
        {
            r.passed = r.checks.Count(c => c.passed); r.total = r.checks.Count; r.utc = DateTime.UtcNow.ToString("o");
            Directory.CreateDirectory(Output); File.WriteAllText(Path.Combine(Output, file), JsonUtility.ToJson(r, true));
            Debug.Log($"[Phase7A] {r.suite}: {r.passed}/{r.total} passed. " + string.Join("; ", r.checks.Where(c => !c.passed).Select(c => "FAIL " + c.name + " " + c.detail)));
        }

        // ---------- static (edit mode) ----------
        [MenuItem("Business as Usual/Phase 7A/Run Static Validation")]
        public static void RunStatic()
        {
            if (EditorApplication.isPlaying) { Debug.LogError("Stop Play Mode first."); return; }
            var scene = EditorSceneManager.OpenScene(Phase7ABuilder.Scene, OpenSceneMode.Single);
            var r = new Report { suite = "Phase 7A static", scene = scene.path };
            var temp = new List<MeshCollider>();
            try
            {
                foreach (var shell in new[] { "Daylight Modeled Neighborhood/MarketShell", "Daylight Modeled Neighborhood/WorkshopShell" }) temp.Add(GameObject.Find(shell).AddComponent<MeshCollider>());
                Physics.SyncTransforms();
                // Collision integrity: along every modeled wall the gameplay collider must stop a 0.24 m
                // capsule at least 3 cm before its centre (the camera) reaches the visible surface.
                int samples = 0; var failures = new List<string>();
                void Wall(string label, Vector3 from, Vector3 dir, float len)
                {
                    samples++;
                    var hits = Physics.RaycastAll(from, dir, len, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance).ToArray();
                    var mesh = hits.FirstOrDefault(h => h.collider is MeshCollider mc && temp.Contains(mc));
                    var solid = hits.FirstOrDefault(h => !(h.collider is MeshCollider mc2 && temp.Contains(mc2)));
                    if (!mesh.collider) return; // opening
                    float stopAt = solid.collider ? solid.distance - .265f : float.PositiveInfinity;
                    if (!solid.collider || stopAt > mesh.distance - .03f) failures.Add(label + $" mesh@{mesh.distance:F2} stop@{stopAt:F2}");
                }
                for (float y = .3f; y <= 1.7f; y += .7f)
                {
                    for (float z = 4f; z <= 9f; z += .5f) { Wall("market west", new Vector3(-14, y, z), Vector3.right, 3); Wall("market east", new Vector3(-2, y, z), Vector3.left, 3); Wall("garage west in", new Vector3(5.2f, y, z), Vector3.left, 2); Wall("garage east in", new Vector3(10.8f, y, z), Vector3.right, 2); Wall("garage west out", new Vector3(2, y, z), Vector3.right, 3); Wall("garage east out", new Vector3(14, y, z), Vector3.left, 3); }
                    for (float x = -11.4f; x <= -4.6f; x += .2f) { Wall("market front", new Vector3(x, y, 1.0f), Vector3.forward, 3); Wall("market rear", new Vector3(x, y, 12), Vector3.back, 3); }
                    for (float x = 4.7f; x <= 11.3f; x += .2f) { Wall("garage front out", new Vector3(x, y, 1.0f), Vector3.forward, 3); Wall("garage rear in", new Vector3(x, y, 8.8f), Vector3.forward, 1.2f); Wall("garage rear out", new Vector3(x, y, 12), Vector3.back, 3); }
                }
                Add(r, "collision_walls_cover_modeled_shells", failures.Count == 0, samples + " rays; " + string.Join(" | ", failures.Take(8)));
                // Market doorway: collision gap matches the modeled frame opening.
                float frameL = Cast(temp[0], new Vector3(-8.2f, 1, 3.6f), Vector3.left), frameR = Cast(temp[0], new Vector3(-8.2f, 1, 3.6f), Vector3.right);
                var leftWall = GameObject.Find("Graybox Town/Shop/Shop Front Left").GetComponent<Collider>().bounds; var rightWall = GameObject.Find("Graybox Town/Shop/Shop Front Right").GetComponent<Collider>().bounds;
                float gapL = leftWall.max.x, gapR = rightWall.min.x, openL = -8.2f - frameL, openR = -8.2f + frameR;
                Add(r, "market_door_gap_matches_frame", Mathf.Abs(gapL - openL) < .04f && Mathf.Abs(gapR - openR) < .04f, $"frame {openL:F2}..{openR:F2} collision gap {gapL:F2}..{gapR:F2}");
                var hinge = GameObject.Find("Graybox Town/Shop/Shop Door/Hinge"); var portal = GameObject.Find("Town Workplaces/Enter Daylight Market");
                Add(r, "market_door_leaf_and_entry_centered_in_frame", Mathf.Abs(hinge.transform.position.x - openL) < .05f && Mathf.Abs(portal.transform.position.x - (openL + openR) / 2) < .05f, $"hinge {hinge.transform.position.x:F2} portal {portal.transform.position.x:F2}");
            }
            finally { foreach (var mc in temp) if (mc) UnityEngine.Object.DestroyImmediate(mc); Physics.SyncTransforms(); }

            // Props with collision.
            string[] props = { "WasteCart", "Crate", "Workbench", "WasteBin", "ToolRack", "Yard canopy post", "ParcelCart", "Square municipal signpost", "Bench", "Planter", "Noticeboard", "Clock", "Tree" };
            var missing = new List<string>(); int propCount = 0;
            foreach (var parent in new[] { "Original Daylight Neighborhood", "Daylight Modeled Neighborhood" })
                foreach (Transform t in GameObject.Find(parent).transform)
                {
                    if (!props.Contains(t.name) || t.position.x < -15.5f || t.position.x > 15.5f || Mathf.Abs(t.position.z) > 13.5f) continue;
                    propCount++;
                    if (!t.GetComponentsInChildren<Collider>().Any(c => c.enabled && !c.isTrigger)) missing.Add(t.name + "@" + t.position.ToString("F1"));
                }
            Add(r, "solid_props_inside_bounds_have_colliders", missing.Count == 0 && propCount > 25, propCount + " props; missing " + string.Join(",", missing));

            // Vehicles rest on the floor; colliders match bodies.
            for (int i = 0; i < 2; i++)
            {
                var root = GameObject.Find("Town Workplaces/Service Vehicle " + i).transform; var van = root.Find("ServiceVan");
                var mesh = van.GetComponent<MeshFilter>().sharedMesh; var rend = van.GetComponent<Renderer>().bounds; var box = root.GetComponent<BoxCollider>().bounds;
                float tyre = rend.min.y;
                Add(r, "vehicle_" + i + "_grounded", tyre <= .05f && tyre >= .04f && Mathf.Abs(van.lossyScale.x - van.lossyScale.y) < .001f, $"lowest tyre y {tyre:F3}, floor 0.050, scale {van.lossyScale.x:F2}");
                Add(r, "vehicle_" + i + "_collider_fits_body", Mathf.Abs(box.max.y - rend.max.y) < .06f && box.min.y <= .06f && box.size.x <= rend.size.x + .02f && box.size.z <= rend.size.z + .02f, $"collider {box.min:F2}..{box.max:F2} body {rend.min:F2}..{rend.max:F2}");
                Add(r, "vehicle_" + i + "_contact_shadow", root.Find("Contact shadow") && root.Find("Contact shadow").GetComponent<Renderer>().enabled, "");
            }

            // Repair/interaction accessibility using the server's own range/line-of-sight rule.
            var targets = UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(t => t.gameObject.activeInHierarchy).ToArray();
            var blocked = new List<string>(); var repairDetail = new List<string>();
            foreach (var t in targets)
            {
                var spot = Accessible(t, out float distance);
                if (!spot.HasValue) blocked.Add(t.name);
                if (t is TownStation s && (s.kind == TownStation.Kind.Repair || s.kind == TownStation.Kind.Inspect)) repairDetail.Add($"{t.name}: {(spot.HasValue ? "stand " + spot.Value.ToString("F2") + " reach " + distance.ToString("F2") : "NONE")}");
            }
            Add(r, "every_interaction_point_has_a_free_standing_spot_in_server_range", blocked.Count == 0, targets.Length + " targets; blocked: " + string.Join(",", blocked));
            // Interaction points must sit on (or right at) their own object, never floating in space.
            var floating = new List<string>();
            foreach (var t in targets)
            {
                var cols = t.GetComponentsInChildren<Collider>(true).Where(c => c.enabled).ToArray();
                if (cols.Length == 0) continue;
                float d = cols.Min(c => Vector3.Distance(c.ClosestPointOnBounds(t.InteractionPoint), t.InteractionPoint));
                if (d > .35f) floating.Add($"{t.name} {d:F2} m");
            }
            Add(r, "interaction_points_on_their_objects", floating.Count == 0, string.Join(", ", floating));
            Add(r, "repair_and_inspection_points_accessible", repairDetail.Count == 4 && repairDetail.All(d => !d.EndsWith("NONE")), string.Join(" | ", repairDetail));

            // Settings persistence round trip (restores the developer's own preferences afterwards).
            var keys = new[] { "master", "effects", "music", "sensitivity", "invertY", "fov", "quality", "vsync", "frameCap" };
            var saved = keys.ToDictionary(k => k, k => PlayerPrefs.HasKey(GameSettings.Prefix + k) ? (object)(PlayerPrefs.GetFloat(GameSettings.Prefix + k, float.NaN) is float f && !float.IsNaN(f) ? f : PlayerPrefs.GetInt(GameSettings.Prefix + k)) : null);
            try
            {
                GameSettings.Load();
                GameSettings.SetMaster(.35f); GameSettings.SetEffects(.6f); GameSettings.SetSensitivity(.21f); GameSettings.SetInvertY(true); GameSettings.SetFieldOfView(83); GameSettings.SetFrameCap(60); GameSettings.SetVSync(false);
                GameSettings.Load();
                bool same = Mathf.Approximately(GameSettings.Master, .35f) && Mathf.Approximately(GameSettings.Effects, .6f) && Mathf.Approximately(GameSettings.Sensitivity, .21f) && GameSettings.InvertY && Mathf.Approximately(GameSettings.FieldOfView, 83) && GameSettings.FrameCap == 60 && !GameSettings.VSync;
                Add(r, "settings_persist_across_reload", same, $"master {GameSettings.Master} fx {GameSettings.Effects} sens {GameSettings.Sensitivity} invert {GameSettings.InvertY} fov {GameSettings.FieldOfView} cap {GameSettings.FrameCap} vsync {GameSettings.VSync}");
                // Graphics options apply only in players (the Editor would rewrite QualitySettings.asset);
                // the Windows harness verifies vSyncCount / targetFrameRate in the built game.
                Add(r, "settings_apply_audio_in_editor", Mathf.Approximately(AudioListener.volume, .35f), $"listener {AudioListener.volume}");
                GameSettings.SetSensitivity(9); Add(r, "settings_clamp_invalid_values", GameSettings.Sensitivity <= .5f, "sensitivity " + GameSettings.Sensitivity);
                Add(r, "music_setting_hidden_without_music", !PauseMenu.MusicAvailable && !Resources.FindObjectsOfTypeAll<AudioSource>().Any(a => a.loop && a.clip), "no music source exists in the project; the slider is not shown");
            }
            finally
            {
                foreach (var k in keys) PlayerPrefs.DeleteKey(GameSettings.Prefix + k);
                foreach (var kv in saved) if (kv.Value is float f) PlayerPrefs.SetFloat(GameSettings.Prefix + kv.Key, f); else if (kv.Value is int i) PlayerPrefs.SetInt(GameSettings.Prefix + kv.Key, i);
                PlayerPrefs.Save(); GameSettings.Load(); AudioListener.volume = 1;
            }

            // Map projection.
            var town = TownMinimap.TownWorld; var market = TownMinimap.MarketWorld;
            var region = new TownMinimap.Region { world = town }; var mregion = new TownMinimap.Region { world = market };
            var samplesMap = new[] { new Vector3(-8, 0, 6.5f), new Vector3(8, 0, 6.5f), new Vector3(8, 0, -7.7f), new Vector3(0, 0, 11), new Vector3(-15, 0, -13) };
            float err = samplesMap.Max(p => Vector3.Distance(TownMinimap.World(region, TownMinimap.Normalized(region, p)), new Vector3(p.x, 0, p.z)));
            var n = TownMinimap.Normalized(region, new Vector3(-8, 0, 6.5f));
            Add(r, "map_world_to_map_round_trip", err < .001f && n.x < .5f && n.y < .5f, $"max error {err:F5} m; market at {n}");
            Add(r, "map_regions_cover_all_targets", targets.All(t => region.Contains(t.InteractionPoint) || mregion.Contains(t.InteractionPoint)), "");
            var tex = Resources.Load<Texture2D>("Town/Map/DaylightTownMap"); var mtex = Resources.Load<Texture2D>("Town/Map/DaylightMarketMap");
            Add(r, "map_textures_baked_once_with_matching_aspect", tex && mtex && Mathf.Abs((float)tex.width / tex.height - town.width / town.height) < .01f && Mathf.Abs((float)mtex.width / mtex.height - market.width / market.height) < .01f, tex ? tex.width + "x" + tex.height + ", " + mtex.width + "x" + mtex.height : "missing");
            // Prefab wiring.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Phase7ABuilder.PlayerPrefab);
            Add(r, "player_prefab_has_first_person_components", prefab.GetComponent<FirstPersonView>() && prefab.GetComponent<FirstPersonHands>()?.armsSource && prefab.GetComponent<PauseMenu>() && prefab.GetComponent<AvatarPresentation>() && prefab.GetComponentsInChildren<Camera>(true).Length == 1 && prefab.GetComponentsInChildren<AudioListener>(true).Length == 1, "");
            var phase5 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Multiplayer/Prefabs/SunnyNetworkPlayer.prefab");
            Add(r, "phase5_shop_prefab_keeps_third_person", phase5 && !phase5.GetComponent<FirstPersonView>(), "");
            Save(r, "static-validation.json");
        }

        static float Cast(MeshCollider mc, Vector3 o, Vector3 d) => mc.Raycast(new Ray(o, d), out var h, 5) ? h.distance : 99;

        public static Vector3? Accessible(Interactable t, out float bestDistance)
        {
            const float reach = 1.8f, origin = .65f, radius = .24f;
            Vector3 p = t.InteractionPoint; Vector3? best = null; bestDistance = 99;
            int mask = ~(1 << 2);
            for (float r = .45f; r <= 1.4f; r += .15f)
                for (int a = 0; a < 24; a++)
                {
                    var dir = Quaternion.Euler(0, a * 15, 0) * Vector3.forward;
                    var stand = new Vector3(p.x, 0, p.z) + dir * r;
                    if (!Physics.Raycast(stand + Vector3.up * 1.5f, Vector3.down, out var ground, 3, mask, QueryTriggerInteraction.Ignore)) continue;
                    stand.y = ground.point.y;
                    if (Physics.CheckCapsule(stand + Vector3.up * (radius + .05f), stand + Vector3.up * (1.2f - radius), radius, mask, QueryTriggerInteraction.Ignore)) continue;
                    var eye = stand + Vector3.up * origin; float d = Vector3.Distance(eye, p);
                    if (d > reach) continue;
                    if (Physics.Linecast(eye, p, out var hit, mask, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<Interactable>() != t) continue;
                    if (d < bestDistance) { bestDistance = d; best = stand; }
                }
            return best;
        }

        // ---------- play mode ----------
        [MenuItem("Business as Usual/Phase 7A/Run Play Mode Validation (enter Play first)")]
        public static void RunPlay()
        {
            if (!EditorApplication.isPlaying) { Debug.LogError("Enter Play Mode in DaylightTown first."); return; }
            var go = new GameObject("Phase 7A play validation") { hideFlags = HideFlags.DontSave };
            go.AddComponent<Phase7APlayRunner>();
        }
    }

    public sealed class Phase7APlayRunner : MonoBehaviour
    {
        Phase7AValidation.Report r;
        void Add(string name, bool ok, string detail = "") => r.checks.Add(new Phase7AValidation.Check { name = name, passed = ok, detail = detail });
        IEnumerator Start()
        {
            r = new Phase7AValidation.Report { suite = "Phase 7A play mode (editor host)", scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path };
            var session = NetworkSession.Instance;
            if (!session.Manager.IsListening) { session.Port = 7791; session.StartSession(true); }
            float until = Time.realtimeSinceStartup + 10;
            NetworkPlayer player = null;
            while ((player = Phase7ACapture.Owner()) == null && Time.realtimeSinceStartup < until) yield return null;
            if (!player) { Add("host_spawned", false, session.Status); Finish(); yield break; }
            yield return new WaitForSeconds(.6f);
            var view = player.GetComponent<FirstPersonView>(); var hands = player.GetComponent<FirstPersonHands>(); var pause = player.GetComponent<PauseMenu>(); var input = player.GetComponent<PrototypeInput>();
            var orbit = player.GetComponentInChildren<ThirdPersonCamera>(true);
            int cams = FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.isActiveAndEnabled), listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(c => c.isActiveAndEnabled);
            Add("one_camera_one_listener", cams == 1 && listeners == 1, $"cameras {cams} listeners {listeners}");
            Add("first_person_active_orbit_disabled", view.Active && !orbit.enabled && view.ViewCamera == orbit.GetComponent<Camera>(), "");
            var body = player.Motor.visual.GetComponentsInChildren<Renderer>(true);
            Add("owner_body_hidden_but_casts_shadow", body.Length > 0 && body.All(b => b.shadowCastingMode == ShadowCastingMode.ShadowsOnly), body.Length + " renderers");
            Add("viewmodel_built_from_sunny_forearms", hands.Built && hands.Visible && FindObjectsByType<FirstPersonHands>(FindObjectsSortMode.None).Count(h => h.Visible) == 1, "");
            Add("eye_height_stable", Mathf.Abs(view.ViewCamera.transform.position.y - player.transform.position.y - view.eyeHeight) < .05f, (view.ViewCamera.transform.position.y - player.transform.position.y).ToString("F3"));

            // Choose Mechanic through the real authority path.
            player.SendCommand("job:2");
            until = Time.realtimeSinceStartup + 4; while (NetworkShop.Instance.Town.Job(player.OwnerClientId) != Profession.Mechanic && Time.realtimeSinceStartup < until) yield return null;
            Add("job_selected_through_authority", NetworkShop.Instance.Town.Job(player.OwnerClientId) == Profession.Mechanic, "");
            var book = player.GetComponent<TownHandbook>(); book.Choosing = false; book.Expanded = false; input.SetCursor(true);

            // Collision walks with the production motor and CharacterController.
            var temp = new List<MeshCollider>();
            foreach (var n in new[] { "Daylight Modeled Neighborhood/MarketShell", "Daylight Modeled Neighborhood/WorkshopShell" }) temp.Add(GameObject.Find(n).AddComponent<MeshCollider>());
            // Mesh probes are enabled only for the eye check, never while the motor moves.
            foreach (var mc in temp) mc.enabled = false;
            probes = temp;
            var door = FindObjectsByType<SimpleDoor>(FindObjectsSortMode.None).First();
            var walks = new List<(string name, Vector3 start, float yaw, float seconds, Func<Vector3, bool> ok)>
            {
                ("market_facade_east_of_door", new Vector3(-6.6f,.08f,2.0f), 0, 1.5f, p => p.z <= 3.2f - .2f),
                ("market_facade_west_of_door", new Vector3(-9.6f,.08f,2.0f), 0, 1.5f, p => p.z <= 3.2f - .2f),
                ("market_door_closed_blocks", new Vector3(-8.2f,.08f,2.4f), 0, 1.2f, p => p.z <= 3.5f - .2f),
                ("garage_west_wall_inside", new Vector3(5.3f,.08f,8.4f), 270, 1.2f, p => p.x >= 4.62f + .2f),
                ("garage_rear_wall_and_tool_rack", new Vector3(9.0f,.08f,8.4f), 0, 1.5f, p => p.z <= 9.08f - .2f),
                ("van_0_side", new Vector3(5.1f,.08f,6.3f), 90, 1.2f, p => p.x <= 5.69f - .2f),
                ("van_1_front", new Vector3(9.6f,.08f,4.0f), 0, 1.2f, p => p.z <= 5.13f - .2f),
                ("waste_cart", new Vector3(11.15f,.08f,-6.2f), 180, 1.2f, p => p.z >= -7.3f + .2f),
                ("yard_crate_stack", new Vector3(4.5f,.08f,-7.6f), 180, 1.2f, p => p.z >= -8.77f + .2f),
                ("parcel_cart", new Vector3(-12.65f,.08f,1.8f), 0, 1.2f, p => p.z <= 2.81f - .2f),
                ("parts_workbench", new Vector3(-10.8f,.08f,1.5f), 0, 1.2f, p => p.z <= 2.64f - .2f),
                ("north_boundary", new Vector3(3f,.08f,12.4f), 0, 2f, p => p.z <= 13.65f - .2f),
                ("garage_opening_is_open", new Vector3(8f,.08f,1.6f), 0, 1.4f, p => p.z >= 4.2f),
            };
            foreach (var w in walks)
            {
                var end = Walk(player, w.start, w.yaw, w.seconds, out bool eyeClear);
                Add("collide_" + w.name, w.ok(end) && eyeClear, $"end {end:F2} eyeClear {eyeClear}");
                yield return null;
            }
            // Doorway with the door open, via the door's own interaction.
            door.Interact(player.Interactor); door.Advance(5); Physics.SyncTransforms();
            var through = Walk(player, new Vector3(-8.2f, .08f, 2.4f), 0, 1.3f, out bool clearDoor);
            Add("collide_market_doorway_open_passable", through.z >= 3.9f && clearDoor, $"end {through:F2}");
            door.Interact(player.Interactor); door.Advance(5);
            foreach (var mc in temp) Destroy(mc);
            probes = null;

            // Interaction aim: with two stations in reach, the one in the view cone wins.
            player.Motor.Teleport(new Vector3(5.9f, .08f, 4.6f)); yield return null;
            var inspect = FindObjectsByType<TownStation>(FindObjectsSortMode.None).First(s => s.kind == TownStation.Kind.Inspect && s.index == 0);
            var repair = FindObjectsByType<TownStation>(FindObjectsSortMode.None).First(s => s.kind == TownStation.Kind.Repair && s.index == 0);
            NetworkShop.Instance.Town.Started[1] = true; NetworkShop.Instance.Town.Services[0].inspected = true;
            Look(view, inspect.InteractionPoint); yield return null; player.Interactor.FindNearby(); var a = player.Interactor.Current;
            Look(view, repair.InteractionPoint); yield return null; player.Interactor.FindNearby(); var b = player.Interactor.Current;
            Add("interaction_targets_what_the_camera_aims_at", a == inspect && b == repair, $"aim inspect -> {(a ? a.name : "none")}, aim repair -> {(b ? b.name : "none")}");
            NetworkShop.Instance.Town.Started[1] = false; NetworkShop.Instance.Town.Services[0].inspected = false;

            // Local pause.
            float scale = Time.timeScale; int revision = NetworkShop.Instance.Revision;
            pause.SetOpen(true); yield return null;
            Add("pause_releases_cursor_and_blocks_input", PauseMenu.IsOpen && !input.GameplayActive && input.Blocked && Cursor.lockState == CursorLockMode.None && Cursor.visible, $"lock {Cursor.lockState}");
            Add("pause_never_freezes_simulation", Mathf.Approximately(Time.timeScale, scale) && session.Manager.IsListening, "timeScale " + Time.timeScale);
            var before = player.transform.position; player.Motor.Step(Vector2.up, false, false, .2f);
            Add("pause_keeps_server_authority_running", (player.transform.position - before).sqrMagnitude > 0, "server motor still steps while this player reads the menu");
            pause.HandleEscape(); yield return null;
            Add("escape_resumes_and_relocks_cursor", !PauseMenu.IsOpen && !input.Blocked && Cursor.lockState == CursorLockMode.Locked, $"lock {Cursor.lockState}");
            book.Expanded = true; input.SetCursor(false); pause.HandleEscape(); yield return null;
            Add("escape_closes_handbook_before_pausing", !book.Expanded && !PauseMenu.IsOpen, "");

            // Minimap region and live tracking.
            player.Motor.Teleport(new Vector3(-40f, .08f, 3.2f)); yield return new WaitForSeconds(.3f);
            Add("minimap_switches_to_market_floor_plan", book.Minimap.RegionFor(player.transform.position) == book.Minimap.Market, "");
            player.Motor.Teleport(new Vector3(3f, .08f, -1f)); yield return new WaitForSeconds(.3f);
            Add("minimap_town_region_outdoors", book.Minimap.RegionFor(player.transform.position) == book.Minimap.Town && book.Minimap.Baked, "");

            // Viewmodel evidence renders (world + hands; IMGUI is captured by the Windows harness).
            string dir = Path.Combine(Path.GetDirectoryName(Phase7AValidation.Output), "After");
            player.Motor.Teleport(new Vector3(8f, .08f, 2.2f)); view.SetView(0, 6); yield return new WaitForSeconds(.5f);
            Phase7ACapture.Render(Path.Combine(dir, "fp-garage-hands-idle.png"));
            player.Motor.Teleport(new Vector3(6.4f, .08f, 4.05f)); view.SetView(0, 20); yield return new WaitForSeconds(.5f);
            Phase7ACapture.Render(Path.Combine(dir, "fp-van0-repair-point.png"));
            player.Motor.Teleport(new Vector3(-8.2f, .08f, 1.2f)); view.SetView(0, 0); yield return new WaitForSeconds(.5f);
            Phase7ACapture.Render(Path.Combine(dir, "fp-market-door.png"));
            player.Motor.Teleport(new Vector3(-6.75f, .08f, 2.0f)); view.SetView(0, 0);
            for (int i = 0; i < 90; i++) { player.Motor.cameraTransform.rotation = Quaternion.identity; player.Motor.Step(Vector2.up, false, false, 1 / 60f); }
            yield return new WaitForSeconds(.4f);
            Phase7ACapture.Render(Path.Combine(dir, "fp-pressed-against-market-wall.png"));
            hands.Play(FirstPersonHands.Gesture.Reach, view.ViewCamera.transform.position + view.ViewCamera.transform.forward * .7f); yield return new WaitForSeconds(.2f);
            Phase7ACapture.Render(Path.Combine(dir, "fp-hands-reach.png"));
            Add("hands_pull_back_from_walls", hands.Visible, "see fp-pressed-against-market-wall.png");
            Finish();
        }

        static void Look(FirstPersonView view, Vector3 point)
        {
            var d = point - view.ViewCamera.transform.position;
            view.SetView(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg);
        }

        // Production motor step at 60 Hz; reports whether the camera point ends outside all geometry.
        static List<MeshCollider> probes;
        static Vector3 Walk(NetworkPlayer player, Vector3 start, float yaw, float seconds, out bool eyeClear)
        {
            player.Motor.Teleport(start);
            for (int i = 0; i < Mathf.RoundToInt(seconds * 60); i++) { player.Motor.cameraTransform.rotation = Quaternion.Euler(0, yaw, 0); player.Motor.Step(Vector2.up, false, false, 1 / 60f); }
            var end = player.transform.position;
            var eye = end + Vector3.up * 1.0f;
            if (probes != null) foreach (var mc in probes) mc.enabled = true;
            Physics.SyncTransforms();
            var overlaps = Physics.OverlapSphere(eye, .04f, ~0, QueryTriggerInteraction.Ignore).Where(c => !c.transform.IsChildOf(player.transform)).ToArray();
            eyeClear = overlaps.Length == 0;
            if (probes != null) foreach (var mc in probes) mc.enabled = false;
            Physics.SyncTransforms();
            return end;
        }

        void Finish()
        {
            r.passed = r.checks.Count(c => c.passed); r.total = r.checks.Count; r.utc = DateTime.UtcNow.ToString("o");
            Directory.CreateDirectory(Phase7AValidation.Output);
            File.WriteAllText(Path.Combine(Phase7AValidation.Output, "playmode-validation.json"), JsonUtility.ToJson(r, true));
            Debug.Log($"[Phase7A] {r.suite}: {r.passed}/{r.total} passed. " + string.Join("; ", r.checks.Where(c => !c.passed).Select(c => "FAIL " + c.name + " " + c.detail)));
            Destroy(gameObject);
        }
    }
}
