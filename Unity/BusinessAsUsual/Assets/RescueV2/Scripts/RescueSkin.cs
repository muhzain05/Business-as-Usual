using TMPro;
using UnityEngine;

namespace BusinessAsUsual.RescueV2
{
    [CreateAssetMenu(menuName="Business as Usual/Workday UI skin")]
    public sealed class RescueSkin : ScriptableObject
    {
        public TMP_FontAsset display, labels, reading, readingBold;
        public TMP_FontAsset[] comparisonHeadings;
        public Color paper=Hex("F7EBCD"),ink=Hex("203F40"),white=Hex("FFF9E9"),dark=Hex("152E2D"),
            mustard=Hex("E7B647"),coral=Hex("D36F55"),green=Hex("3E735D"),rule=Hex("D6C5A1");
        public float feedbackSeconds=.18f;
        static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
    }
}
