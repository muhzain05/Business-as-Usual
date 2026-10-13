using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusinessAsUsual.RescueV2
{
    // Opt-in native-player evidence. Never sent through ordinary gameplay snapshots or RPCs.
    [Serializable] public sealed class RescueUIEvidence
    {
        public string screen,action,headingFont;public int tab,records;public bool menuOpen;
        public Vector2 reference;public int activeCanvases;public TextEvidence[] text;
        public string handPose,interaction;public int authoredGestures;public bool authoredHandsVisible;
        [Serializable]public sealed class TextEvidence{public string value,font;public Vector2 at,size,preferred;public float pointSize;public bool offscreen,overflows;}
        public static RescueUIEvidence Capture(int width=0,int height=0)
        {
            var ui=RescueInterface.Instance;if(!ui)return null;
            var scaler=ui.GetComponent<CanvasScaler>();
            var canvas=ui.GetComponent<Canvas>();var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;if(width==0)width=Screen.width;if(height==0)height=Screen.height;
            var hands=UnityEngine.Object.FindFirstObjectByType<RescueHands>();var local=UnityEngine.Object.FindObjectsByType<BusinessAsUsual.Multiplayer.NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.IsOwner);
            return new RescueUIEvidence{handPose=hands?hands.Pose:"legacy",authoredGestures=hands?hands.Gestures:0,authoredHandsVisible=hands&&hands.Visible,interaction=local&&local.Interactor.Current?local.Interactor.Current.name:"",screen=ui.ScreenName,action=ui.CurrentAction,tab=ui.SelectedTab,records=ui.PrivateRecordCount,menuOpen=ui.MenuOpen,reference=scaler.referenceResolution,activeCanvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Count(c=>c.isActiveAndEnabled&&c.isRootCanvas),
                text=ui.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.isActiveAndEnabled&&!string.IsNullOrEmpty(t.text)).Select(t=>{
                    t.ForceMeshUpdate();var corners=new Vector3[4];t.rectTransform.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);var rect=t.rectTransform.rect;var pref=t.GetPreferredValues(t.text,rect.width,0);
                    return new TextEvidence{value=t.text,font=t.font?t.font.name:"missing",at=a,size=b-a,preferred=pref,pointSize=t.fontSize,offscreen=a.x<-.5f||a.y<-.5f||b.x>width+.5f||b.y>height+.5f,overflows=pref.y>rect.height+2||pref.x>rect.width+2};}).ToArray()};
        }
    }
}
