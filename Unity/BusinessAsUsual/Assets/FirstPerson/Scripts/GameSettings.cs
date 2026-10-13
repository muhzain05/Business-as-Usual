using System;
using System.Linq;
using UnityEngine;

namespace BusinessAsUsual.FirstPerson
{
    // Local, per-machine preferences. Nothing here is networked or affects authority.
    public static class GameSettings
    {
        public const string Prefix = "bau.settings.";
        public static readonly int[] FrameCaps = { 0, 30, 60, 120, 144 };
        public static float Master { get; private set; } = 1;
        public static float Effects { get; private set; } = .8f;
        public static float Music { get; private set; } = .7f;
        public static float Sensitivity { get; private set; } = .12f;
        public static bool InvertY { get; private set; }
        public static float FieldOfView { get; private set; } = 74;
        public static FullScreenMode DisplayMode { get; private set; } = FullScreenMode.FullScreenWindow;
        public static Vector2Int Resolution { get; private set; }
        public static int Quality { get; private set; } = -1;
        public static bool VSync { get; private set; } = true;
        public static int FrameCap { get; private set; } = 0;
        public static event Action Changed;
        static bool loaded;

        public static void EnsureLoaded() { if (!loaded) Load(); }
        public static void Load()
        {
            loaded = true;
            Master = PlayerPrefs.GetFloat(Prefix + "master", 1);
            Effects = PlayerPrefs.GetFloat(Prefix + "effects", .8f);
            Music = PlayerPrefs.GetFloat(Prefix + "music", .7f);
            Sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "sensitivity", .12f), .02f, .5f);
            InvertY = PlayerPrefs.GetInt(Prefix + "invertY", 0) == 1;
            FieldOfView = Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "fov", 74), 60, 95);
            DisplayMode = (FullScreenMode)PlayerPrefs.GetInt(Prefix + "displayMode", (int)Screen.fullScreenMode);
            if (DisplayMode != FullScreenMode.Windowed) DisplayMode = FullScreenMode.FullScreenWindow;
            Resolution = new Vector2Int(PlayerPrefs.GetInt(Prefix + "width", 0), PlayerPrefs.GetInt(Prefix + "height", 0));
            Quality = PlayerPrefs.GetInt(Prefix + "quality", QualitySettings.GetQualityLevel());
            VSync = PlayerPrefs.GetInt(Prefix + "vsync", 1) == 1;
            FrameCap = PlayerPrefs.GetInt(Prefix + "frameCap", 0);
            Apply(false);
        }
        public static void SetMaster(float v) { Master = Mathf.Clamp01(v); Save(); }
        public static void SetEffects(float v) { Effects = Mathf.Clamp01(v); Save(); }
        public static void SetMusic(float v) { Music = Mathf.Clamp01(v); Save(); }
        public static void SetSensitivity(float v) { Sensitivity = Mathf.Clamp(v, .02f, .5f); Save(); }
        public static void SetInvertY(bool v) { InvertY = v; Save(); }
        public static void SetFieldOfView(float v) { FieldOfView = Mathf.Clamp(Mathf.Round(v), 60, 95); Save(); }
        public static void SetQuality(int v) { Quality = Mathf.Clamp(v, 0, QualitySettings.names.Length - 1); Save(true); }
        public static void SetVSync(bool v) { VSync = v; Save(true); }
        public static void SetFrameCap(int v) { FrameCap = v; Save(true); }
        public static void SetDisplay(FullScreenMode mode, Vector2Int resolution) { DisplayMode = mode; Resolution = resolution; Save(true); }

        // Distinct sizes the monitor supports, largest refresh discarded; windowed play may use any.
        public static Vector2Int[] Resolutions()
        {
            var list = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Where(r => r.x >= 1024 && r.y >= 600).Distinct().OrderBy(r => r.x * r.y).ToList();
            var current = new Vector2Int(Screen.width, Screen.height);
            if (!list.Contains(current)) list.Add(current);
            if (list.Count == 0) list.Add(new Vector2Int(1280, 720));
            return list.OrderBy(r => r.x * r.y).ToArray();
        }

        static void Save(bool display = false)
        {
            PlayerPrefs.SetFloat(Prefix + "master", Master);
            PlayerPrefs.SetFloat(Prefix + "effects", Effects);
            PlayerPrefs.SetFloat(Prefix + "music", Music);
            PlayerPrefs.SetFloat(Prefix + "sensitivity", Sensitivity);
            PlayerPrefs.SetInt(Prefix + "invertY", InvertY ? 1 : 0);
            PlayerPrefs.SetFloat(Prefix + "fov", FieldOfView);
            PlayerPrefs.SetInt(Prefix + "displayMode", (int)DisplayMode);
            PlayerPrefs.SetInt(Prefix + "width", Resolution.x);
            PlayerPrefs.SetInt(Prefix + "height", Resolution.y);
            PlayerPrefs.SetInt(Prefix + "quality", Quality);
            PlayerPrefs.SetInt(Prefix + "vsync", VSync ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "frameCap", FrameCap);
            PlayerPrefs.Save();
            Apply(display);
        }

        static void Apply(bool display)
        {
            AudioListener.volume = Master;
            // In the Editor these calls would rewrite ProjectSettings/QualitySettings.asset;
            // graphics preferences apply only in a player build.
            if (!Application.isEditor)
            {
                if (Quality >= 0 && Quality < QualitySettings.names.Length && Quality != QualitySettings.GetQualityLevel()) QualitySettings.SetQualityLevel(Quality, true);
                // Quality levels carry their own vSyncCount; apply the player's choice afterwards.
                QualitySettings.vSyncCount = VSync ? 1 : 0;
                Application.targetFrameRate = VSync || FrameCap <= 0 ? -1 : FrameCap;
            }
            // Automated harnesses size their windows explicitly; never fight their command line.
            if (display && !Application.isEditor)
            {
                var size = Resolution.x > 0 ? Resolution : new Vector2Int(Screen.width, Screen.height);
                Screen.SetResolution(size.x, size.y, DisplayMode);
            }
            Changed?.Invoke();
        }

        public static string QualityName(int level) => level >= 0 && level < QualitySettings.names.Length ? (QualitySettings.names[level] == "Mobile" ? "Performance" : QualitySettings.names[level] == "PC" ? "High" : QualitySettings.names[level]) : "Default";
    }
}
