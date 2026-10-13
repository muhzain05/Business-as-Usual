using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BusinessAsUsual.Town
{
    // Owner-only scenery. Depth-tested symbols point at the actual production interaction point.
    public sealed class TownWayfinding
    {
        readonly TownGraphics graphics;
        readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        readonly List<GameObject> pins=new List<GameObject>(),rings=new List<GameObject>();
        readonly List<TownObjective> objectives=new List<TownObjective>();
        GameObject root;
        Mesh quad;
        public int Count=>pins.Count;
        public TownWayfinding(TownGraphics graphics){this.graphics=graphics;}
        Material Mat(string icon)
        {
            if(materials.TryGetValue(icon,out var material))return material;
            material=new Material(Shader.Find("Business as Usual/Town Marker")){hideFlags=HideFlags.DontSave};material.mainTexture=graphics.Texture(icon);materials.Add(icon,material);return material;
        }
        GameObject Sprite(string name,string icon)
        {
            if(!quad){quad=new Mesh {name="Local paper pin",hideFlags=HideFlags.DontSave};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateBounds();}
            var go=new GameObject(name);go.transform.SetParent(root.transform);go.AddComponent<MeshFilter>().sharedMesh=quad;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=Mat(icon);r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;
        }
        public void Bind(TownGuidance view)
        {
            ClearPins();if(view.job==Profession.None)return;
            if(!root)root=new GameObject("Local town work markers"){hideFlags=HideFlags.DontSave};
            foreach(var objective in view.objectives){if(!objective.active||!objective.target)continue;objectives.Add(objective);var pin=Sprite(objective.title,objective.icon);pins.Add(pin);var block=new MaterialPropertyBlock();block.SetColor("_Color",Color.white);pin.GetComponent<Renderer>().SetPropertyBlock(block);
                bool bay=objective.target is TownStation station&&(station.kind==TownStation.Kind.Inspect||station.kind==TownStation.Kind.Repair);
                var ring=bay?Sprite("Local bay work pad","ring"):null;if(ring){ring.transform.rotation=Quaternion.Euler(90,0,0);ring.transform.localScale=Vector3.one*.72f;}rings.Add(ring);
            }
        }
        public void Tick(Camera camera)
        {
            if(!camera)return;
            for(int i=0;i<pins.Count;i++){
                Vector3 p=objectives[i].Position;float distance=Vector3.Distance(camera.transform.position,p);
                var pin=pins[i];pin.SetActive(distance<33);pin.transform.position=p+Vector3.up*.72f;pin.transform.rotation=camera.transform.rotation;pin.transform.localScale=Vector3.one*Mathf.Clamp(distance*.045f,.27f,.70f);
                if(rings[i])rings[i].transform.position=new Vector3(p.x,.075f,p.z);
            }
        }
        void ClearPins(){foreach(var pin in pins)if(pin)Object.Destroy(pin);foreach(var ring in rings)if(ring)Object.Destroy(ring);pins.Clear();rings.Clear();objectives.Clear();}
        public void Dispose(){ClearPins();if(root)Object.Destroy(root);foreach(var mat in materials.Values)if(mat)Object.Destroy(mat);materials.Clear();if(quad)Object.Destroy(quad);}
    }
}
