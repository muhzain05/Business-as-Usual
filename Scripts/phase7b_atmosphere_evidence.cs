using UnityEngine;using UnityEditor;using System.IO;using BusinessAsUsual.Town;
internal class CommandScript:IRunCommand{public void Execute(ExecutionResult result){
var a=Object.FindFirstObjectByType<TownAtmosphere>();bool enabled=a.enabled;double clock=a.ClockSeconds;a.enabled=false;
var go=new GameObject("Temporary atmosphere evidence camera");var cam=go.AddComponent<Camera>();cam.cullingMask=~(1<<30);cam.clearFlags=CameraClearFlags.Skybox;cam.nearClipPlane=.05f;cam.farClipPlane=150;
var rt=new RenderTexture(1920,1080,24);var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);var active=RenderTexture.active;
string output=Path.GetFullPath("../../Docs/Diagnostics/Phase7B/After");Directory.CreateDirectory(output);
var lines=new System.Collections.Generic.List<string>{"name,passed"};
try{
a.Apply(0);float day=a.sun.intensity;bool dayWindows=System.Array.TrueForAll(a.litWindows,r=>!r.enabled);var cloud=a.clouds[4].position;
cam.fieldOfView=60;cam.transform.position=new Vector3(0,14,-18);cam.transform.LookAt(new Vector3(0,1,3));Capture(cam,rt,texture,Path.Combine(output,"town.png"));
cam.fieldOfView=68;cam.transform.position=new Vector3(0,1.25f,-9);cam.transform.LookAt(new Vector3(0,2,6));Capture(cam,rt,texture,Path.Combine(output,"street-day.png"));
a.Apply(120);lines.Add("clouds-drift,"+(Vector3.Distance(cloud,a.clouds[4].position)>10));Capture(cam,rt,texture,Path.Combine(output,"clouds-later.png"));
a.Apply(306);lines.Add("sun-dims-at-night,"+(a.sun.intensity<day*.5f));lines.Add("street-lamps-light,"+System.Array.TrueForAll(a.lamps,l=>l.enabled&&l.intensity>1));lines.Add("windows-night-only,"+(dayWindows&&System.Array.TrueForAll(a.litWindows,r=>r.enabled)));Capture(cam,rt,texture,Path.Combine(output,"street-night.png"));
cam.fieldOfView=60;cam.transform.position=new Vector3(0,14,-18);cam.transform.LookAt(new Vector3(0,1,3));Capture(cam,rt,texture,Path.Combine(output,"night.png"));
File.WriteAllLines(Path.Combine(output,"atmosphere-validation.csv"),lines);result.Log(string.Join("\n",lines));
}finally{a.Apply(clock);a.enabled=enabled;cam.targetTexture=null;RenderTexture.active=active;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(go);}
}static void Capture(Camera cam,RenderTexture rt,Texture2D texture,string file){cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();File.WriteAllBytes(file,texture.EncodeToPNG());}}
