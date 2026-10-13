using System.IO;
using System.Linq;
using UnityEngine;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.Multiplayer;

namespace BusinessAsUsual.FirstPersonEditor
{
    // Editor-side evidence helper: renders the local first-person camera (world + viewmodel) to PNG.
    // IMGUI overlays are not part of a camera render; UI evidence comes from the Windows harness.
    public static class Phase7ACapture
    {
        public static string Render(string path, int width = 1280, int height = 720)
        {
            var view = FirstPersonView.Local;
            var cam = view && view.ViewCamera ? view.ViewCamera : Camera.main;
            if (!cam) return "no camera";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rt = new RenderTexture(width, height, 24);
            var previous = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = previous;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex); Object.DestroyImmediate(rt);
            return path;
        }
        public static NetworkPlayer Owner() => Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p => p.IsOwner && p.IsSpawned);
    }
}
