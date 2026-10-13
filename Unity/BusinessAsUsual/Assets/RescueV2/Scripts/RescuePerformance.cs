using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
namespace BusinessAsUsual.RescueV2
{
    public sealed class RescuePerformance:MonoBehaviour
    {
        ProfilerRecorder draws,batches,triangles;readonly List<float> frames=new List<float>();float began,nextPercentile;float median,p95;
        public static RescuePerformance Instance {get;private set;}
        [Serializable]public sealed class Evidence{public string gpu,cpu,graphics;public int samples;public float medianFrameMs,p95FrameMs;public long drawCalls,batches,triangles;public bool renderCountersAvailable;}
        void Awake(){Instance=this;began=Time.unscaledTime;draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");batches=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Batches Count");triangles=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count");}
        void Update(){if(Time.unscaledTime-began>5&&frames.Count<12000)frames.Add(Time.unscaledDeltaTime*1000);}
        public Evidence Capture(){if(Time.unscaledTime>=nextPercentile){nextPercentile=Time.unscaledTime+1;var sorted=frames.OrderBy(f=>f).ToArray();median=sorted.Length>0?sorted[sorted.Length/2]:0;p95=sorted.Length>0?sorted[(int)((sorted.Length-1)*.95f)]:0;}return new Evidence{gpu=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,graphics=SystemInfo.graphicsDeviceType.ToString(),samples=frames.Count,medianFrameMs=median,p95FrameMs=p95,drawCalls=draws.Valid?draws.LastValue:0,batches=batches.Valid?batches.LastValue:0,triangles=triangles.Valid?triangles.LastValue:0,renderCountersAvailable=draws.Valid&&batches.Valid&&triangles.Valid};}
        void OnDestroy(){draws.Dispose();batches.Dispose();triangles.Dispose();if(Instance==this)Instance=null;}
    }
}
