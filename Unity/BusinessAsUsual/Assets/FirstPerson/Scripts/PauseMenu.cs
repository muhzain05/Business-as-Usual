using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.Multiplayer;

namespace BusinessAsUsual.FirstPerson
{
    // Owner-only, local pause. Never touches Time.timeScale or the network simulation:
    // the colleague and the host keep playing while this player reads the menu.
    public sealed class PauseMenu : MonoBehaviour
    {
        public enum Page { Main, Settings, Controls, Leaving }
        public static bool IsOpen => Instance && Instance.open;
        public static PauseMenu Instance { get; private set; }
        public Page CurrentPage { get; private set; }
        public int Opened { get; private set; }
        public string LastAction { get; private set; } = "";
        bool open, leaving;
        float openedAt;
        ShopUITheme theme;
        PrototypeInput input;
        NetworkPlayer player;
        InputAction escape;
        Vector2Int[] resolutions;
        int resolutionIndex;
        Vector2 settingsScroll;

        void Awake() { input = GetComponent<PrototypeInput>(); player = GetComponent<NetworkPlayer>(); enabled = false; }
        void OnEnable()
        {
            GameSettings.EnsureLoaded();
            Instance = this;
            theme ??= new ShopUITheme();
            escape ??= new InputAction("Pause menu", InputActionType.Button, "<Keyboard>/escape");
            escape.Enable();
            if (input) input.EscapeHandledExternally = true;
        }
        void OnDisable()
        {
            escape?.Disable();
            if (open) SetOpen(false, false);
            if (input) { input.EscapeHandledExternally = false; input.Blocked = false; }
            if (Instance == this) Instance = null;
        }
        void OnDestroy() { escape?.Dispose(); }

        void Update()
        {
            if (leaving) return;
            if (escape.WasPressedThisFrame()) HandleEscape();
        }

        // Esc closes the innermost layer: sub-page, then menu; otherwise a handbook, then opens the menu.
        public void HandleEscape()
        {
            var rescue=BusinessAsUsual.RescueV2.RescueInterface.Instance;
            if(rescue&&rescue.HandleEscape())return;
            if (open) { if (CurrentPage == Page.Settings || CurrentPage == Page.Controls) { CurrentPage = Page.Main; Click(); } else SetOpen(false); return; }
            var book = GetComponent<BusinessAsUsual.Town.TownHandbook>();
            if (book && book.isActiveAndEnabled && book.LocalJob != BusinessAsUsual.Town.Profession.None && (book.Expanded || book.Choosing || book.TutorialOpen)) { book.CloseFromEscape(); return; }
            SetOpen(true);
        }

        public void SetOpen(bool value, bool sound = true)
        {
            if (open == value) return;
            open = value; openedAt = Time.unscaledTime; CurrentPage = Page.Main;
            if (input) { input.Blocked = value; input.SetCursor(!value && !HandbookWantsCursor()); }
            if (value) { Opened++; resolutions = GameSettings.Resolutions(); resolutionIndex = Closest(resolutions, new Vector2Int(Screen.width, Screen.height)); }
            LastAction = value ? "opened" : "resumed";
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance)BusinessAsUsual.RescueV2.RescueInterface.Instance.Show(value?"Pause":"HUD");
            if (sound) Click();
        }
        bool HandbookWantsCursor()
        {
            var book = GetComponent<BusinessAsUsual.Town.TownHandbook>();
            return book && book.isActiveAndEnabled && (book.Expanded || book.Choosing || book.LocalJob == BusinessAsUsual.Town.Profession.None);
        }
        static int Closest(Vector2Int[] list, Vector2Int size)
        {
            int best = 0; for (int i = 1; i < list.Length; i++) if (Mathf.Abs(list[i].x * list[i].y - size.x * size.y) < Mathf.Abs(list[best].x * list[best].y - size.x * size.y)) best = i; return best;
        }
        static void Click() => TownAudio.Play(TownAudio.Cue.Click);
        public void ShowPage(Page page) { if (!open) SetOpen(true, false); if (page != Page.Leaving) CurrentPage = page; }

        // Leaving while carrying: ask the authority to return the load first, then disconnect.
        // The server also returns loads on disconnect, so a failed request still conserves stock.
        public void Leave(bool quit)
        {
            if (leaving) return;
            leaving = true; CurrentPage = Page.Leaving; LastAction = quit ? "quit" : "leave";
            StartCoroutine(LeaveRoutine(quit));
        }
        IEnumerator LeaveRoutine(bool quit)
        {
            if (player && player.IsSpawned && player.CarryingLoad) { player.SendCommand("return"); float until = Time.unscaledTime + 1.2f; while (Time.unscaledTime < until && player && player.IsSpawned && player.CarryingLoad) yield return null; }
            if (quit)
            {
                if (NetworkSession.Active) NetworkSession.Instance.Manager.Shutdown();
                yield return new WaitForSecondsRealtime(.2f);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else if (NetworkSession.Instance) NetworkSession.Instance.Stop();
        }

        void OnGUI()
        {
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance)return;
            if (!open) return;
            var original = GUI.matrix; var color = GUI.color;
            float scale = ShopHUD.Scale(Screen.width, Screen.height); var offset = ShopHUD.Offset(Screen.width, Screen.height, scale);
            GUI.depth = -100;
            // Dim the live game behind the menu; the world keeps simulating.
            GUI.color = new Color(.07f, .15f, .16f, .45f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = color;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            try
            {
                float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / ShopUITheme.PaperSeconds);
                GUI.matrix *= Matrix4x4.Translate(Vector3.up * ((1 - t) * (1 - t) * 12));
                if (CurrentPage == Page.Settings) Settings();
                else if (CurrentPage == Page.Controls) Controls();
                else Main();
            }
            finally { GUI.matrix = original; GUI.color = color; }
        }

        void Card(Rect r) => BusinessAsUsual.Town.TownGraphics.Card(r);
        bool Button(float x, float y, float w, float h, string label, bool primary = false, int size = 26)
        {
            theme.Pill(x, y, w, h, label, primary ? ShopUITheme.Mustard : ShopUITheme.Paper, size);
            return ShopUITheme.Hit(new Rect(x, y, w, h));
        }

        void Main()
        {
            Card(new Rect(660, 190, 600, 700));
            theme.Text(710, 222, "PAUSED / LOCAL", 21, 1, 500);
            theme.Text(710, 262, "Staff break", 46, 2, 500);
            theme.Text(710, 330, NetworkSession.Active ? "The town keeps working while you read." : "Take your time.", 24, 0, 500);
            theme.Line(710, 378, 500, true);
            if (leaving) { theme.Text(710, 430, "Returning your load and signing off...", 26, 1, 500, 0, true, 120); return; }
            if (Button(710, 410, 500, 66, "RESUME", true)) SetOpen(false);
            if (Button(710, 494, 500, 66, "SETTINGS")) { CurrentPage = Page.Settings; Click(); }
            if (Button(710, 578, 500, 66, "CONTROLS")) { CurrentPage = Page.Controls; Click(); }
            if (Button(710, 662, 500, 66, NetworkSession.Active ? "LEAVE SESSION" : "LEAVE")) Leave(false);
            if (Button(710, 746, 500, 66, "QUIT GAME")) Leave(true);
            theme.Text(710, 838, player && player.CarryingLoad ? "Your carried load returns before you leave." : "Esc  Resume", 21, 0, 500);
        }

        void Slider(float y, string label, float value, System.Action<float> set, float min = 0, float max = 1, string text = null)
        {
            theme.Text(470, y, label, 26, 1, 360);
            var rect = new Rect(850, y + 14, 520, 10);
            ShopUITheme.Fill(rect, ShopUITheme.Rule);
            float n = Mathf.InverseLerp(min, max, value);
            ShopUITheme.Fill(new Rect(rect.x, rect.y, rect.width * n, rect.height), ShopUITheme.Green);
            ShopUITheme.Fill(new Rect(rect.x + rect.width * n - 9, rect.y - 11, 18, 32), ShopUITheme.Ink);
            theme.Text(1465, y, text ?? Mathf.RoundToInt(n * 100) + "%", 24, 1, 80, 2);
            var hit = new Rect(rect.x - 12, y - 4, rect.width + 24, 44);
            var e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && hit.Contains(e.mousePosition))
            { set(Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - rect.x) / rect.width))); e.Use(); }
        }
        void Choice(float y, string label, string value, System.Action previous, System.Action next)
        {
            theme.Text(470, y, label, 26, 1, 360);
            if (Button(850, y - 4, 64, 46, "<", false, 24)) { previous(); Click(); }
            theme.Text(1110, y, value, 25, 1, 380, 1);
            if (Button(1306, y - 4, 64, 46, ">", false, 24)) { next(); Click(); }
        }

        void Settings()
        {
            Card(new Rect(420, 110, 1080, 860));
            theme.Text(470, 140, "SETTINGS / THIS COMPUTER", 21, 1, 700);
            theme.Text(470, 176, "Staff preferences", 42, 2, 900);
            float y = 260;
            theme.Text(470, y, "SOUND", 20, 1, 300); y += 36;
            Slider(y, "Master volume", GameSettings.Master, GameSettings.SetMaster); y += 58;
            Slider(y, "Sound effects", GameSettings.Effects, GameSettings.SetEffects); y += 58;
            // Music volume appears only once the game has a music track to control.
            if (MusicAvailable) { Slider(y, "Music", GameSettings.Music, GameSettings.SetMusic); y += 58; }
            theme.Text(470, y, "CONTROLS", 20, 1, 300); y += 36;
            Slider(y, "Mouse sensitivity", GameSettings.Sensitivity, GameSettings.SetSensitivity, .02f, .5f, (GameSettings.Sensitivity * 10).ToString("0.0")); y += 58;
            Choice(y, "Invert Y-axis", GameSettings.InvertY ? "On" : "Off", () => GameSettings.SetInvertY(!GameSettings.InvertY), () => GameSettings.SetInvertY(!GameSettings.InvertY)); y += 58;
            Slider(y, "Field of view", GameSettings.FieldOfView, GameSettings.SetFieldOfView, 60, 95, Mathf.RoundToInt(GameSettings.FieldOfView) + "°"); y += 58;
            theme.Text(470, y, "DISPLAY", 20, 1, 300); y += 36;
            bool windowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            Choice(y, "Display mode", windowed ? "Windowed" : "Fullscreen", ToggleWindowed, ToggleWindowed); y += 58;
            var res = resolutions != null && resolutions.Length > 0 ? resolutions[Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1)] : new Vector2Int(Screen.width, Screen.height);
            Choice(y, "Resolution", res.x + " x " + res.y, () => StepResolution(-1), () => StepResolution(1)); y += 58;
            Choice(y, "Graphics quality", GameSettings.QualityName(QualitySettings.GetQualityLevel()), () => GameSettings.SetQuality((QualitySettings.GetQualityLevel() + QualitySettings.names.Length - 1) % QualitySettings.names.Length), () => GameSettings.SetQuality((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length)); y += 58;
            string cap = GameSettings.VSync ? "VSync" : GameSettings.FrameCap <= 0 ? "Unlimited" : GameSettings.FrameCap + " FPS";
            Choice(y, "Frame rate", cap, () => StepFrame(-1), () => StepFrame(1)); y += 58;
            theme.Text(470, 900, "Saved on this computer. Nothing here changes the shared shift.", 21, 0, 760);
            if (Button(1250, 885, 210, 54, "BACK", true, 22)) { CurrentPage = Page.Main; Click(); }
        }
        public static bool MusicAvailable => false;
        void ToggleWindowed()
        {
            var mode = Screen.fullScreenMode == FullScreenMode.Windowed ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            var res = resolutions != null && resolutions.Length > 0 ? resolutions[resolutionIndex] : new Vector2Int(Screen.width, Screen.height);
            GameSettings.SetDisplay(mode, res);
        }
        void StepResolution(int step)
        {
            if (resolutions == null || resolutions.Length == 0) return;
            resolutionIndex = (resolutionIndex + step + resolutions.Length) % resolutions.Length;
            GameSettings.SetDisplay(Screen.fullScreenMode, resolutions[resolutionIndex]);
        }
        void StepFrame(int step)
        {
            // VSync, then the caps, then unlimited.
            var options = new[] { -1 }.Concat(GameSettings.FrameCaps.Skip(1)).Concat(new[] { 0 }).ToArray();
            int current = GameSettings.VSync ? 0 : System.Array.IndexOf(options, GameSettings.FrameCap);
            if (current < 0) current = 0;
            int next = (current + step + options.Length) % options.Length;
            if (options[next] == -1) GameSettings.SetVSync(true);
            else { GameSettings.SetFrameCap(options[next]); GameSettings.SetVSync(false); }
        }

        void Controls()
        {
            Card(new Rect(420, 130, 1080, 820));
            theme.Text(470, 160, "CONTROLS / STAFF CARD", 21, 1, 700);
            theme.Text(470, 196, "How to work", 42, 2, 900);
            string[,] rows = {
                {"W A S D","Walk"},{"Mouse","Look around"},{"Shift","Run"},{"Space","Jump"},
                {"E","Use what you are looking at / pick up / place"},{"Hold E","Inspect, repair and sort"},
                {"Q","Return your carried load"},{"Tab","Staff handbook"},{"J","Staff desk / professions"},
                {"M","Town map"},{"Esc","Pause menu"} };
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                float y = 280 + i * 54;
                theme.Pill(470, y, 190, 42, rows[i, 0], ShopUITheme.Pale, 20);
                theme.Text(690, y + 6, rows[i, 1], 25, 0, 720);
            }
            if (Button(1250, 865, 210, 54, "BACK", true, 22)) { CurrentPage = Page.Main; Click(); }
        }
    }
}
