using UnityEngine;using UnityEditor;using UnityEditor.Animations;using System.IO;using System.Linq;using System.Globalization;
internal class CommandScript:IRunCommand{
public void Execute(ExecutionResult result){
string output=Path.GetFullPath("../../Docs/Diagnostics/Phase7B/Animation");Directory.CreateDirectory(output);
var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art/Sunny/Sunny_v004.fbx"));
var cameraObject=new GameObject("Temporary animation evidence camera");var cam=cameraObject.AddComponent<Camera>();cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.83f,.80f,.69f);cam.fieldOfView=42;
var fixtures=new GameObject("Temporary stance reference grid");var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.32f,.48f,.43f);material.SetFloat("_Smoothness",0);
var rt=new RenderTexture(1280,720,24);var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);var oldActive=RenderTexture.active;
try{
foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
for(int i=-8;i<=24;i++){var line=GameObject.CreatePrimitive(PrimitiveType.Cube);line.transform.SetParent(fixtures.transform);line.layer=29;line.transform.position=new Vector3(0,.003f,i*.2f);line.transform.localScale=new Vector3(2,.004f,.012f);line.GetComponent<Renderer>().sharedMaterial=material;}
var root=go;var foot=go.GetComponentsInChildren<Transform>().First(t=>t.name=="mixamorig:LeftFoot");
var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Prototype/Animation/SunnyLocomotion.controller");
var csv=new System.Collections.Generic.List<string>{"clip,length,cadence,speed,stance,worldStanceDrift"};
foreach(var word in new[]{"Walk","Run"})for(int version=0;version<2;version++){
string label=(version==0?"Phase7A":"Phase7B")+"-"+word;
var clip=version==0?controller.animationClips.First(c=>c.name.Contains(word)):AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Town/Phase7B/Animation/Sunny"+word+".anim");
float duty=word=="Walk"?(version==0?.62f:.60f):(version==0?.34f:.40f);
float cadence=word=="Walk"?(version==0?1.45f:1.6923077f):(version==0?1.85f:1.4f);
float speed=word=="Walk"?2.2f:4.2f;
go.transform.position=Vector3.zero;float start=clip.length*.04f;clip.SampleAnimation(root,start);var a=foot.position+Vector3.forward*(speed*start/cadence);float drift=0;
for(int i=0;i<=100;i++){float time=clip.length*Mathf.Lerp(.04f,duty-.04f,i/100f);clip.SampleAnimation(root,time);var b=foot.position+Vector3.forward*(speed*time/cadence);drift=Mathf.Max(drift,Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z)));}
csv.Add(label+","+string.Join(",",new[]{clip.length,cadence,speed,duty,drift}.Select(v=>v.ToString("F8",CultureInfo.InvariantCulture))));
string folder=Path.Combine(output,label);Directory.CreateDirectory(folder);
for(int i=0;i<24;i++){float time=clip.length*i/24f;clip.SampleAnimation(root,time);var movement=Vector3.forward*(speed*time/cadence);go.transform.position=movement;
cam.transform.position=movement+new Vector3(1.9f,.95f,1.9f);cam.transform.LookAt(movement+new Vector3(0,.62f,0));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D2")+".png"),texture.EncodeToPNG());}
go.transform.position=Vector3.zero;
}
File.WriteAllLines(Path.Combine(output,"imported-stance-validation.csv"),csv);result.Log(string.Join("\n",csv));
}finally{cam.targetTexture=null;RenderTexture.active=oldActive;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(go);Object.DestroyImmediate(fixtures);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(material);}
}}
