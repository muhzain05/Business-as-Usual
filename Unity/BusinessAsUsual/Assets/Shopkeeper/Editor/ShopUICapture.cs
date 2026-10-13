using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BusinessAsUsual.Shopkeeper.Editor
{
    // Editor-only evidence capture. Never saves or modifies the game scene.
    public static class ShopUICapture
    {
        static EditorWindow view;
        static int originalSize {get=>SessionState.GetInt("BAU.UI.OriginalResolution",-1);set=>SessionState.SetInt("BAU.UI.OriginalResolution",value);}
        public static bool Pending { get; private set; }
        public static string Output => Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Docs/Diagnostics/Phase4.2"));
        public static void Resolution(int width, int height)
        {
            var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            view=EditorWindow.GetWindow(type);
            var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            if(originalSize<0) originalSize=(int)type.GetProperty("selectedSizeIndex",flags).GetValue(view);
            type.GetMethod("SetCustomResolution",flags).Invoke(view,new object[]{new Vector2(width,height),"BAU UI validation"});
            view.Focus(); view.Repaint(); EditorApplication.QueuePlayerLoopUpdate();
        }
        public static void Capture(string name)
        {
            if(Pending) throw new InvalidOperationException("Wait for the preceding screenshot.");
            Directory.CreateDirectory(Output); Pending=true; int frames=0;
            EditorApplication.CallbackFunction tick=null;
            tick=()=> {
                view.Repaint(); EditorApplication.QueuePlayerLoopUpdate();
                if(++frames<12)return;
                EditorApplication.update-=tick;
                ScreenCapture.CaptureScreenshot(Path.Combine(Output,name+".png"));
                Pending=false;
            };
            EditorApplication.update+=tick;
        }
        public static void RestoreResolution(int fallback=-1)
        {
            if(!view)view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            int index=fallback>=0?fallback:originalSize;if(index<0)return;
            view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,index);
            originalSize=-1; view.Repaint();
        }
    }
}
