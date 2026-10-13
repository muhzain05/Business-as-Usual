using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BusinessAsUsual.FirstPerson;
namespace BusinessAsUsual.RescueV2
{
    public static class RescueCapture
    {
        // Test-only native Unity composition at display sizes the desktop cannot accommodate.
        public static IEnumerator Offscreen(int width,int height,string path){
            var ui=RescueInterface.Instance;var view=FirstPersonView.Local;if(!ui||!view)yield break;
            var camera=view.ViewCamera;var canvas=ui.GetComponent<Canvas>();var scaler=ui.GetComponent<CanvasScaler>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance,scale=canvas.scaleFactor,aspect=camera.aspect;bool enabled=scaler.enabled;var target=camera.targetTexture;
            var texture=new RenderTexture(width,height,24){name="Native UI resolution evidence"};texture.Create();
            try{
                camera.targetTexture=texture;camera.aspect=(float)width/height;scaler.enabled=false;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.01f;canvas.scaleFactor=Mathf.Sqrt((float)width*height/(1280*720));Canvas.ForceUpdateCanvases();yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
                var previous=RenderTexture.active;RenderTexture.active=texture;var png=new Texture2D(width,height,TextureFormat.RGB24,false);png.ReadPixels(new Rect(0,0,width,height),0,0);png.Apply();File.WriteAllBytes(path,png.EncodeToPNG());UnityEngine.Object.Destroy(png);RenderTexture.active=previous;
                File.WriteAllText(Path.ChangeExtension(path,"json"),JsonUtility.ToJson(RescueUIEvidence.Capture(width,height),true));
            }finally{camera.targetTexture=target;camera.aspect=aspect;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;canvas.scaleFactor=scale;scaler.enabled=enabled;texture.Release();UnityEngine.Object.Destroy(texture);Canvas.ForceUpdateCanvases();}
        }
    }
}
