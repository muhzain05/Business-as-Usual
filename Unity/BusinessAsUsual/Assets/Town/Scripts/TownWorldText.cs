using UnityEngine;

namespace BusinessAsUsual.Town
{
    // Keep dynamic font atlases current without altering the font used by the handbook.
    [RequireComponent(typeof(TextMesh))]
    public sealed class TownWorldText : MonoBehaviour
    {
        TextMesh label;
        Material original, material;
        Renderer display;
        void Awake()
        {
            label=GetComponent<TextMesh>();display=GetComponent<Renderer>();
            original=display.sharedMaterial;
            material=new Material(original){hideFlags=HideFlags.DontSave};
            material.shader=Shader.Find("Business as Usual/Shelf Text");
            display.sharedMaterial=material;Refresh(label.font);
        }
        void OnEnable(){Font.textureRebuilt+=Refresh;}
        void OnDisable(){Font.textureRebuilt-=Refresh;}
        void Refresh(Font font)
        {
            if(material&&label&&font==label.font)material.mainTexture=font.material.mainTexture;
        }
        void OnDestroy()
        {
            if(display)display.sharedMaterial=original;
            if(material)Destroy(material);
        }
    }
}
