using System.Collections.Generic;
using UnityEngine;

namespace BusinessAsUsual.FirstPerson
{
    // Small synthesized effect palette; no external audio assets or licences.
    // Effects follow the Sound Effects slider; the listener follows Master.
    public static class TownAudio
    {
        public enum Cue { Step, Pickup, Place, Confirm, Tool, Paper, Click, Land, Chime }
        static readonly Dictionary<Cue, AudioClip[]> clips = new Dictionary<Cue, AudioClip[]>();
        static AudioSource ui;
        public static int Played { get; private set; }

        public static void Play(Cue cue, Vector3? position = null, float volume = 1)
        {
            GameSettings.EnsureLoaded();
            var set = Clips(cue);
            var clip = set[Random.Range(0, set.Length)];
            float level = volume * GameSettings.Effects;
            if (level <= 0.001f) return;
            Played++;
            if (position.HasValue)
            {
                var go = new GameObject("SFX " + cue) { hideFlags = HideFlags.DontSave };
                go.transform.position = position.Value;
                var source = go.AddComponent<AudioSource>();
                source.clip = clip; source.volume = level; source.spatialBlend = 1; source.minDistance = 1.5f; source.maxDistance = 22; source.rolloffMode = AudioRolloffMode.Linear;
                source.pitch = Random.Range(.94f, 1.06f); source.Play();
                Object.Destroy(go, clip.length + .1f);
            }
            else
            {
                if (!ui) { var go = new GameObject("Local UI audio") { hideFlags = HideFlags.DontSave }; Object.DontDestroyOnLoad(go); ui = go.AddComponent<AudioSource>(); ui.spatialBlend = 0; }
                ui.pitch = Random.Range(.97f, 1.03f); ui.PlayOneShot(clip, level);
            }
        }

        static AudioClip[] Clips(Cue cue)
        {
            if (clips.TryGetValue(cue, out var set)) return set;
            set = new AudioClip[cue == Cue.Step ? 4 : 2];
            for (int i = 0; i < set.Length; i++) set[i] = Synthesize(cue, i);
            clips[cue] = set;
            return set;
        }

        static AudioClip Synthesize(Cue cue, int variant)
        {
            const int rate = 22050;
            float length = cue == Cue.Chime ? 1.8f : cue == Cue.Step ? .09f : cue == Cue.Tool ? .16f : cue == Cue.Paper ? .18f : cue == Cue.Land ? .14f : .12f;
            int n = Mathf.CeilToInt(length * rate);
            var data = new float[n];
            var random = new System.Random(((int)cue + 1) * 977 + variant * 31);
            float tone = cue == Cue.Confirm ? 660 : cue == Cue.Click ? 1250 : cue == Cue.Tool ? 1850 + variant * 230 : cue == Cue.Pickup ? 220 : cue == Cue.Place ? 150 : 95 + variant * 12;
            float lowpass = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate, u = t / length;
                float noise = (float)(random.NextDouble() * 2 - 1);
                lowpass += (noise - lowpass) * (cue == Cue.Paper ? .55f : .12f);
                float env, s;
                switch (cue)
                {
                    case Cue.Chime: env = Mathf.Exp(-u * 5); s = Mathf.Sin(2*Mathf.PI*523.25f*t)*.5f + Mathf.Sin(2*Mathf.PI*1046.5f*t)*.16f; break;
                    case Cue.Step: env = Mathf.Exp(-u * 9); s = lowpass * .9f + Mathf.Sin(2 * Mathf.PI * tone * t) * .35f; break;
                    case Cue.Land: env = Mathf.Exp(-u * 6); s = lowpass + Mathf.Sin(2 * Mathf.PI * 70 * t) * .5f; break;
                    case Cue.Paper: env = Mathf.Sin(Mathf.PI * u) * .7f; s = (noise - lowpass) * .6f; break;
                    case Cue.Tool: env = Mathf.Exp(-u * 7); s = Mathf.Sin(2 * Mathf.PI * tone * t) * .5f + Mathf.Sin(2 * Mathf.PI * tone * 2.76f * t) * .25f + noise * .08f; break;
                    case Cue.Confirm: env = Mathf.Exp(-u * 5); s = Mathf.Sin(2 * Mathf.PI * (u < .45f ? tone : tone * 1.5f) * t) * .45f; break;
                    case Cue.Click: env = Mathf.Exp(-u * 18); s = Mathf.Sin(2 * Mathf.PI * tone * t) * .5f; break;
                    default: env = Mathf.Exp(-u * 8); s = lowpass * .6f + Mathf.Sin(2 * Mathf.PI * tone * t * (1 - u * .3f)) * .55f; break;
                }
                data[i] = Mathf.Clamp(s * env * Mathf.Min(1, i / 40f), -1, 1) * .8f;
            }
            var clip = AudioClip.Create("Synth " + cue + " " + variant, n, 1, rate, false);
            clip.SetData(data, 0);
            clip.hideFlags = HideFlags.DontSave;
            return clip;
        }
    }
}
