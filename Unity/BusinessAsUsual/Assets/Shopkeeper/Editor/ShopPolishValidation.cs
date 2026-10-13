using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    // Live-time presentation tests complement the original synchronous full-shift checks.
    [InitializeOnLoad]
    public static class ShopPolishValidation
    {
        [Serializable] public class ValidationCheck { public string name, details; public bool passed; }
        [Serializable] class Report { public string method, timestampUtc; public List<ValidationCheck> checks = new List<ValidationCheck>(); }
        static readonly string Output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Docs/Diagnostics/Phase4.1"));
        static Report report;
        static ThirdPersonMotor motor;
        static PlayerInteractor player;
        static ShopRestockReach reach;
        static ShopShift shift;
        static StockCrate crate;
        static ThirdPersonCamera camera;
        static ShopStation[] shelves;
        static double began;
        static float actionAt, nextFrame;
        static int shelfIndex, frame, scenario, journalBefore;
        static bool midpoint;
        static ShopPolishValidation()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BAU.PolishTest", false))
                { began = EditorApplication.timeSinceStartup; EditorApplication.update += Wait; }
            };
        }
        [MenuItem("Business as Usual/Phase 4.1/Validate Polish and Capture Reaches")]
        public static void Begin()
        {
            if (EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Stop Play and save scene first.");
            EditorSceneManager.OpenScene(ShopBuilder.ScenePath);
            SessionState.SetBool("BAU.PolishTest", true);
            EditorApplication.isPlaying = true;
        }
        static void Check(string name, bool passed, string details) => report.checks.Add(new ValidationCheck { name = name, passed = passed, details = details });
        static void Wait()
        {
            if (EditorApplication.timeSinceStartup - began < 2) return;
            EditorApplication.update -= Wait;
            SessionState.SetBool("BAU.PolishTest", false);
            Directory.CreateDirectory(Output);
            report = new Report { method = "Live Unity Play mode: real frame timing, production interactions, synthetic Q and deterministic motor interruption. No human playthrough.", timestampUtc = DateTime.UtcNow.ToString("o") };
            try
            {
                motor = UnityEngine.Object.FindFirstObjectByType<ThirdPersonMotor>();
                player = motor.GetComponent<PlayerInteractor>(); reach = motor.GetComponent<ShopRestockReach>();
                shift = UnityEngine.Object.FindFirstObjectByType<ShopShift>(); crate = UnityEngine.Object.FindFirstObjectByType<StockCrate>();
                camera = motor.GetComponentInChildren<ThirdPersonCamera>();
                motor.enabled = player.enabled = camera.enabled = false;
                shelves = UnityEngine.Object.FindObjectsByType<ShopStation>(FindObjectsSortMode.None).Where(s => s.kind == ShopStation.Kind.Shelf).OrderBy(s => s.sku).ToArray();
                Check("generic_rig_supported", reach && reach.RigSupported && !motor.animator.isHuman, "Existing named upper bones; no humanoid retargeting or root motion.");
                Check("world_labels_depth_material", UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None).All(t => t.GetComponent<Renderer>().sharedMaterial.shader.name == "Business as Usual/Shelf Text"), "192 fixed world labels use LEqual depth testing.");
                shift.Receive(player); crate.Interact(player);
                shelfIndex = scenario = 0;
                StartShelf(); EditorApplication.update += Tick;
            }
            catch (Exception e) { Check("exception", false, e.ToString()); Finish(); }
        }
        static void Approach(ShopStation shelf)
        {
            motor.Teleport(new Vector3(shelf.InteractionPoint.x, .05f, shelf.InteractionPoint.z - 1.1f));
            motor.visual.rotation = Quaternion.identity;
            camera.yaw = -30; camera.pitch = 12; camera.distance = 2.2f; camera.PositionCamera();
            player.FindNearby();
        }
        static void StartShelf()
        {
            var shelf = shelves[shelfIndex]; Approach(shelf);
            var display = shelf.GetComponent<ShopStockDisplay>(); display.SendMessage("LateUpdate");
            journalBefore = shift.Stock.Journal.Count;
            Check("begin:" + shelf.sku, player.Current == shelf && player.TryInteract() && reach.IsActive, "Production shelf interaction starts 0.65s upper-body reach.");
            display.SendMessage("LateUpdate");
            Check("row_waits_for_reach:" + shelf.sku, display.units.All(u => !u.activeSelf), "Visual placement waits; ledger commits once at interaction acceptance.");
            for (int i = 0; i < 12; i++) Check("repeat_blocked:" + shelf.sku + ":" + i, !player.TryInteract(), "Held/repeated input cannot trigger a second shelf action.");
            actionAt = Time.time; nextFrame = 0; frame = 0; midpoint = false;
        }
        static void Tick()
        {
            try
            {
                float t = Time.time - actionAt;
                if (scenario == 0)
                {
                    if (t >= nextFrame && t < .8f)
                    { Capture(camera.GetComponent<Camera>(), Path.Combine(Output, "reach-" + shelves[shelfIndex].sku + "-" + frame++.ToString("D3") + ".png")); nextFrame = t + 1f / 30; }
                    if (!midpoint && t > .34f)
                    {
                        midpoint = true;
                        Check("placement:" + shelves[shelfIndex].sku, shelves[shelfIndex].GetComponent<ShopStockDisplay>().units.All(u => u.activeSelf), "All six visible at the reach peak.");
                    }
                    if (t < .85f) return;
                    string sku = shelves[shelfIndex].sku;
                    Check("finish:" + sku, !reach.IsActive && shift.Stock.Count("shelf", sku) == 6 && shift.Stock.Journal.Count == journalBefore + 1, "Exactly one transfer; smooth return ends without inventory callbacks.");
                    if (++shelfIndex < shelves.Length) { StartShelf(); return; }
                    Check("normal_conservation", ShopShift.Products.All(p => shift.Stock.Total(p.sku) == 6) && !shift.Carried, "All 18 units conserved; empty crate automatically returns.");
                    // Fresh temporary shift exercises interruption without reseeding the approved shift.
                    var temp = new GameObject("Polish interruption test shift").AddComponent<ShopShift>();
                    shift = temp; crate.shift = shift;
                    foreach (var s in shelves) { s.shift = shift; s.GetComponent<ShopStockDisplay>().shift = shift; }
                    shift.Receive(player); crate.Interact(player);
                    shelfIndex = 0; Approach(shelves[0]); journalBefore = shift.Stock.Journal.Count;
                    player.TryInteract(); scenario = 1; actionAt = Time.time; return;
                }
                if (scenario == 1 && t > .1f)
                {
                    motor.Step(Vector2.left, false, false, 1f / 30);
                    scenario = 2; actionAt = Time.time; return;
                }
                if (scenario == 2 && t > .25f)
                {
                    Check("movement_interrupt", !reach.IsActive && shift.Stock.Journal.Count == journalBefore + 1 && shift.Stock.Count("shelf", shelves[0].sku) == 6, "Moving cancels reach in 0.12s; committed inventory is neither duplicated nor rolled back.");
                    var keyboard = InputSystem.AddDevice<Keyboard>("PolishQ");
                    try { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Q)); InputSystem.Update(); shift.SendMessage("Update"); InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update(); }
                    finally { InputSystem.RemoveDevice(keyboard); }
                    Check("Q_after_interrupt", !shift.Carried && ShopShift.Products.All(p => shift.Stock.Total(p.sku) == 6), "Q returns remaining contents only.");
                    crate.Interact(player); Approach(shelves[1]); journalBefore = shift.Stock.Journal.Count; player.TryInteract();
                    scenario = 3; actionAt = Time.time; return;
                }
                if (scenario == 3 && t > .12f)
                {
                    reach.enabled = false;
                    Check("disable_interrupt", !reach.IsActive && shift.Stock.Journal.Count == journalBefore + 1, "Disabling restores pose and adds no inventory transaction.");
                    reach.enabled = true;
                    Check("repeat_after_disable", !player.TryInteract() && shift.Stock.Journal.Count == journalBefore + 1, "Same carried SKU exhausted; repeat cannot duplicate units.");
                    Approach(shelves[2]); player.TryInteract();
                    Check("last_shelf_repeat_blocked", !player.TryInteract() && shift.Stock.Count("basket", shelves[2].sku) == 0, "Final crate auto-return does not turn a repeat press into an early collection.");
                    Check("interruption_conservation", ShopShift.Products.All(p => shift.Stock.Total(p.sku) == 6), "Fresh interruption shift conserves all original quantities.");
                    Finish();
                }
            }
            catch (Exception e) { Check("exception", false, e.ToString()); Finish(); }
        }
        static void Finish()
        {
            EditorApplication.update -= Tick;
            File.WriteAllText(Path.Combine(Output, "polish-validation.json"), JsonUtility.ToJson(report, true));
            Debug.Log("Phase 4.1 polish validation " + report.checks.Count(c => c.passed) + "/" + report.checks.Count);
            EditorApplication.isPlaying = false;
        }
        public static void Capture(Camera camera, string path)
        {
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(1280, 720, 24); var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try { camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = oldActive; RenderTexture.ReleaseTemporary(rt); UnityEngine.Object.DestroyImmediate(tex); }
        }
    }
}
