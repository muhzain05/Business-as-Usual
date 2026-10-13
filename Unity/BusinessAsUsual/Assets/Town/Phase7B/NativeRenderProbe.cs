using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BusinessAsUsual.Town
{
    // Read-only CPU probe: capture URP's actual camera target descriptor; enqueue no GPU pass.
    public sealed class NativeRenderProbe : ScriptableRendererFeature
    {
        public static int CameraId, Width, Height, Samples;
        public static float Scale;
        public override void Create() { }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (data.cameraData.cameraType != CameraType.Game) return;
            CameraId=data.cameraData.camera.GetInstanceID();
            var descriptor=data.cameraData.cameraTargetDescriptor;
            Width=descriptor.width;Height=descriptor.height;Samples=descriptor.msaaSamples;Scale=data.cameraData.renderScale;
        }
    }
}
