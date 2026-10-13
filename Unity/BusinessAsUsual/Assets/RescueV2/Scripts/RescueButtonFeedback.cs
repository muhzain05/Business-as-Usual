using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BusinessAsUsual.RescueV2
{
    public sealed class RescueButtonFeedback : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler
    {
        Image image;Button button;RescueSkin skin;Color normal,target;bool hover,down;TMP_Text label;Outline focus;
        public void Configure(RescueSkin theme,Color rest){skin=theme;normal=rest;target=rest;image=GetComponent<Image>();button=GetComponent<Button>();}
        void Start(){label=GetComponentInChildren<TMP_Text>();focus=gameObject.AddComponent<Outline>();focus.effectColor=skin.ink;focus.effectDistance=new Vector2(1,-1);focus.enabled=false;}
        void Update(){if(!image||!skin)return;bool active=button&&button.interactable;var c=!active?skin.rule:down?skin.green:hover?skin.mustard:normal;image.color=c;if(label)label.color=down&&active?skin.white:skin.ink;if(focus)focus.enabled=hover&&active;}
        public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=false;down=false;}
        public void OnPointerDown(PointerEventData e){down=true;}public void OnPointerUp(PointerEventData e){down=false;}
        public void OnSelect(BaseEventData e){hover=true;}public void OnDeselect(BaseEventData e){hover=false;}
    }
}
