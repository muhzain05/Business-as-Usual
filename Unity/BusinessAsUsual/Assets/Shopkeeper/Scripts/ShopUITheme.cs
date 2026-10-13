using System.Collections.Generic;
using UnityEngine;

namespace BusinessAsUsual.Shopkeeper
{
    // Approved Staff Edition tokens and cached drawing primitives, shared by all six surfaces.
    public sealed class ShopUITheme
    {
        public static readonly Color Paper = Hex("F6E9C9"), Ink = Hex("244047"), Green = Hex("649B76"),
            Mustard = Hex("EFC864"), Coral = Hex("CE735A"), Pale = Hex("FFF7E5"), Rule = Hex("D4C7A8");
        public const float Width=1920, Height=1080, PaperSeconds=.15f, StampSeconds=.18f;
        readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        readonly Dictionary<string,int> fittedSizes=new Dictionary<string,int>();
        readonly Font[] fonts;
        public ShopUITheme()
        {
            fonts=new[]{Resources.Load<Font>("Handbook/Fonts/RoundedMplus1c-Regular"),
                Resources.Load<Font>("Handbook/Fonts/RoundedMplus1c-ExtraBold"),
                Resources.Load<Font>("Handbook/Fonts/DelaGothicOne-Regular"),Resources.Load<Font>("Handbook/Fonts/YuseiMagic-Regular")};
        }
        static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
        public bool FontsAvailable { get {foreach(var f in fonts)if(!f)return false;return true;} }
        GUIStyle Style(int size,int role,int align,bool wrap=false)
        {
            string key=size+":"+role+":"+align+":"+wrap;
            if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle(GUI.skin.label){font=fonts[role],fontSize=size,wordWrap=wrap,
                alignment=align==1?TextAnchor.UpperCenter:align==2?TextAnchor.UpperRight:TextAnchor.UpperLeft,
                padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0)};
            s.normal.textColor=Ink;styles.Add(key,s);return s;
        }
        public void Text(float x,float y,string value,int size=24,int role=0,float width=650,int align=0,bool wrap=false,float height=0)
        {
            // Fit only headings/short labels; essential data never shrinks below 24 reference pixels.
            string key=value+":"+size+":"+role+":"+width;
            int fitted=size;
            if(!wrap){if(!fittedSizes.TryGetValue(key,out fitted)){
                fitted=size;var content=new GUIContent(value);
                while(fitted>24 && Style(fitted,role,align).CalcSize(content).x>width)fitted--;
                fittedSizes[key]=fitted;
            }}
            var rect=new Rect(align==2?x-width:align==1?x-width/2:x,y,width,height>0?height:wrap?size*3.6f:size*1.65f);
            GUI.Label(rect,value,Style(fitted,role,align,wrap));
        }
        public Texture2D Texture(string name)
        {
            if(!textures.TryGetValue(name,out var t)){t=Resources.Load<Texture2D>("Handbook/"+name);textures[name]=t;}
            return t;
        }
        public void Art(string name,float x,float y,float width=0,float height=0)
        {
            var t=Texture(name);if(!t)return;
            GUI.DrawTexture(new Rect(x,y,width>0?width:t.width,height>0?height:t.height),t,ScaleMode.StretchToFill,true);
        }
        public static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        public void Line(float x,float y,float width,bool dash=false,Color? color=null,float thick=2)
        {
            if(dash){for(float n=0;n<width;n+=20)Fill(new Rect(x+n,y,Mathf.Min(10,width-n),thick),color??Rule);}
            else Fill(new Rect(x,y,width,thick),color??Rule);
        }
        public void Pill(float x,float y,float w,float h,string label,Color? fill=null,int size=22)
        {
            Fill(new Rect(x,y,w,h),Ink);Fill(new Rect(x+3,y+3,w-6,h-6),fill??Mustard);
            Text(x+w/2,y+5,label,size,1,w-12,1);
        }
        public void Tag(float x,float y,string label,float width=180){Pill(x+22,y-17,width,29,label,Mustard,18);}
        public void Key(float x,float y,string label){Fill(new Rect(x+3,y+4,61,60),Ink);Pill(x,y,58,56,label,Pale,32);}
        public void Item(float x,float y,string sku,float scale=1){Art(sku,x,y,64*scale,80*scale);}
        public static bool Hit(Rect r)=>GUI.Button(r,GUIContent.none,GUIStyle.none);
        public void Stamp(string name,float x,float y,float time,bool reduced)
        {
            float scale=reduced?1:Mathf.Lerp(1.07f,1,Mathf.Clamp01(time/StampSeconds));
            var t=Texture(name);if(!t)return;
            Art(name,x-(scale-1)*t.width/2,y-(scale-1)*t.height/2,t.width*scale,t.height*scale);
        }
    }
}
