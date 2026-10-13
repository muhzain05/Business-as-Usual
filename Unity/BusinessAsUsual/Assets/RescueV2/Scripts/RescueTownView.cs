using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using BusinessAsUsual.FirstPerson;
namespace BusinessAsUsual.RescueV2
{
    // A live exterior view in the existing offset interior. The doorway still uses one walking threshold.
    // This camera has no listener, input or player authority, and renders only near the shop entrance.
    public sealed class RescueTownView:MonoBehaviour
    {
        Camera townCamera;RenderTexture texture;Material material;float next;
        public void Setup(){
            var surface=GameObject.CreatePrimitive(PrimitiveType.Quad);surface.name="Live town beyond market door";Destroy(surface.GetComponent<Collider>());surface.transform.SetParent(transform,false);surface.transform.position=new Vector3(-40,1.10f,-.14f);surface.transform.rotation=Quaternion.Euler(0,180,0);surface.transform.localScale=new Vector3(1.44f,2.20f,1);
            texture=new RenderTexture(480,768,24){name="Market exterior view",filterMode=FilterMode.Bilinear};texture.Create();material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.mainTexture=texture;surface.GetComponent<Renderer>().sharedMaterial=material;
            var go=new GameObject("Market exterior view camera");go.transform.SetParent(transform,false);townCamera=go.AddComponent<Camera>();townCamera.enabled=false;townCamera.fieldOfView=70;townCamera.nearClipPlane=.04f;townCamera.farClipPlane=28;townCamera.clearFlags=CameraClearFlags.Skybox;townCamera.GetUniversalAdditionalCameraData().renderPostProcessing=false;townCamera.transform.SetPositionAndRotation(new Vector3(-8.15f,1.05f,2.35f),Quaternion.Euler(0,180,0));
        }
        void LateUpdate(){var view=FirstPersonView.Local;if(!view||!view.ViewCamera||view.ViewCamera.transform.position.x>-25||view.ViewCamera.transform.position.z>7||Time.unscaledTime<next)return;next=Time.unscaledTime+.10f;townCamera.SubmitRenderRequest(new UniversalRenderPipeline.SingleCameraRequest{destination=texture});}
        void OnDestroy(){if(texture){texture.Release();Destroy(texture);}if(material)Destroy(material);}
    }
}


