using System;
using UnityEngine;
using Unity.Netcode;
using BusinessAsUsual.FirstPerson;

namespace BusinessAsUsual.Town
{
    // Presentation only. NGO's synchronized server clock also works for late joiners.
    // This is an ambient cycle, not a round timer, job deadline or day/meeting system.
    public sealed class TownAtmosphere : MonoBehaviour
    {
        public Light sun;
        public Light[] lamps = Array.Empty<Light>();
        public Renderer[] litWindows = Array.Empty<Renderer>();
        public Transform[] clouds = Array.Empty<Transform>(), trees = Array.Empty<Transform>();
        public float cycleSeconds = 600;
        public static float Phase(double seconds, float duration = 600) => Mathf.Repeat((float)(seconds % duration) / duration + .34f, 1);
        public float CurrentPhase { get; private set; }
        public double ClockSeconds { get; private set; }
        public bool UsingServerClock { get; private set; }
        Vector3[] cloudOrigins;
        Quaternion[] treeRest;
        MaterialPropertyBlock block;
        int lastHour = -1;
        Material originalSky, runtimeSky;
        void Awake()
        {
            cloudOrigins = Array.ConvertAll(clouds, t => t.localPosition);
            treeRest = Array.ConvertAll(trees, t => t.localRotation);
            block = new MaterialPropertyBlock();
            originalSky=RenderSettings.skybox;
            if(originalSky){runtimeSky=new Material(originalSky);RenderSettings.skybox=runtimeSky;}
        }
        void Update()
        {
            var manager = NetworkManager.Singleton;
            UsingServerClock = manager && manager.IsListening;
            ClockSeconds = UsingServerClock ? manager.ServerTime.Time : Time.timeAsDouble;
            Apply(ClockSeconds);
        }
        public void Apply(double seconds)
        {
            if (block == null) block = new MaterialPropertyBlock();
            CurrentPhase = Phase(seconds, cycleSeconds);
            float daylight = Mathf.SmoothStep(0, 1, Mathf.Clamp01(Mathf.Sin((CurrentPhase - .25f) * Mathf.PI * 2) * 2.2f));
            float evening = 1 - daylight;
            if(runtimeSky&&runtimeSky.HasProperty("_Exposure"))runtimeSky.SetFloat("_Exposure",Mathf.Lerp(.15f,1f,daylight));
            if(runtimeSky&&runtimeSky.HasProperty("_Daylight"))runtimeSky.SetFloat("_Daylight",daylight);
            Shader.SetGlobalColor("_TownCloudTint",Color.Lerp(new Color(.32f,.40f,.54f,1),Color.white,daylight));
            if (sun)
            {
                sun.transform.rotation = Quaternion.Euler(CurrentPhase * 360 - 90, -32, 0);
                sun.intensity = Mathf.Lerp(.12f, .95f, daylight);
                sun.color = Color.Lerp(new Color(.46f,.57f,.80f), new Color(1,.91f,.78f), daylight);
            }
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.30f,.37f,.49f), new Color(.57f,.66f,.70f), daylight);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(.23f,.29f,.37f), new Color(.43f,.43f,.34f), daylight);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(.17f,.23f,.25f), new Color(.24f,.30f,.24f), daylight);
            RenderSettings.fogColor = Color.Lerp(new Color(.19f,.26f,.38f), new Color(.63f,.73f,.76f), daylight);
            foreach (var lamp in lamps) if (lamp) { lamp.intensity = evening * 1.7f; lamp.enabled = evening > .02f; }
            block.SetColor("_EmissionColor", new Color(1,.53f,.16f) * (evening * 1.1f));
            foreach (var window in litWindows) if (window) { window.enabled=evening>.02f; window.SetPropertyBlock(block); }
            if (cloudOrigins != null) for (int i=0;i<clouds.Length;i++) if(clouds[i])
                clouds[i].localPosition = cloudOrigins[i] + new Vector3(Mathf.Repeat((float)(seconds * .18 + i * 7), 100) - 50, 0, Mathf.Sin((float)seconds * .003f + i) * 2);
            if (treeRest != null) for(int i=0;i<trees.Length;i++) if(trees[i])
                trees[i].localRotation = treeRest[i] * Quaternion.Euler(0,0,Mathf.Sin((float)seconds * 1.1f + i * 1.7f) * .8f);
            int hour = Mathf.FloorToInt(CurrentPhase * 24);
            if (lastHour >= 0 && lastHour != hour && hour % 3 == 0)
                TownAudio.Play(TownAudio.Cue.Chime, new Vector3(0,2.5f,6.5f), .25f);
            lastHour = hour;
        }
        void OnDestroy(){if(runtimeSky){if(RenderSettings.skybox==runtimeSky)RenderSettings.skybox=originalSky;Destroy(runtimeSky);}}
    }
}
