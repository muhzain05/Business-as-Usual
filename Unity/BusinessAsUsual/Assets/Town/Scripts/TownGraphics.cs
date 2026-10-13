using System.Collections.Generic;
using UnityEngine;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Town
{
    // Original town symbols, paper cards and a north-up diagram; no tracking of other players.
    public sealed class TownGraphics
    {
        readonly Dictionary<string,Texture2D> icons=new Dictionary<string,Texture2D>();
        readonly ShopUITheme theme;
        public TownGraphics(ShopUITheme theme){this.theme=theme;}
        public Texture2D Texture(string name)
        {if(!icons.TryGetValue(name,out var icon)){icon=Resources.Load<Texture2D>("Town/Icons/"+name);icons[name]=icon;}return icon;}
        public void Icon(string name,Rect rect,float alpha=1)
        {var icon=Texture(name);if(!icon)return;var old=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(rect,icon);GUI.color=old;}
        public static void Card(Rect r)
        {ShopUITheme.Fill(new Rect(r.x+4,r.y+5,r.width,r.height),new Color(.07f,.15f,.16f,.5f));ShopUITheme.Fill(r,ShopUITheme.Ink);ShopUITheme.Fill(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),ShopUITheme.Paper);}
        public static Vector2 Point(Rect r,Vector3 world)
        {var p=TownGuidance.MapPosition(world);return new Vector2(r.x+r.width*Mathf.Clamp01((p.x+16)/32),r.y+r.height*Mathf.Clamp01((14-p.z)/28));}
        public void Map(Rect card,TownGuidance guide,Vector3 player,float facing,TownJobs jobs,bool large=false)
        {
            Card(card);theme.Text(card.x+15,card.y+11,"DAYLIGHT / TOWN MAP",large?24:18,1,card.width-40);
            var map=new Rect(card.x+14,card.y+42,card.width-28,card.height-83);
            ShopUITheme.Fill(map,ShopUITheme.Pale);
            Vector2 a=Point(map,new Vector3(-14,0,2.5f)),b=Point(map,new Vector3(14,0,-2.5f));ShopUITheme.Fill(new Rect(a.x,a.y,b.x-a.x,b.y-a.y),new Color(.70f,.76f,.65f));
            a=Point(map,new Vector3(-1.7f,0,9));b=Point(map,new Vector3(1.7f,0,2.5f));ShopUITheme.Fill(new Rect(a.x,a.y,b.x-a.x,b.y-a.y),ShopUITheme.Rule);
            Place(map,new Vector3(-8,0,6.5f),"MARKET","shop",large);Place(map,new Vector3(8,0,6.5f),"GARAGE","wrench",large);Place(map,new Vector3(8,0,-7.7f),"YARD","recycle",large);
            var square=Point(map,new Vector3(0,0,6.5f));Icon("clock",new Rect(square.x-10,square.y-10,20,20));
            var alley=Point(map,new Vector3(0,0,11.4f));theme.Line(map.x+10,alley.y,map.width-20,true);theme.Text(map.center.x,alley.y-18,"ALLEY",large?18:13,1,map.width,1);
            theme.Text(map.xMax-12,map.y+2,"N",16,1,20);
            foreach(var objective in guide.objectives){var p=Point(map,objective.Position);float size=large?28:20;Icon(objective.icon,new Rect(p.x-size/2,p.y-size/2,size,size),objective.active?1:.4f);}
            var self=Point(map,player);var old=GUI.matrix;GUIUtility.RotateAroundPivot(facing,self);Icon("you",new Rect(self.x-8,self.y-8,16,16));GUI.matrix=old;
            float progress=TownGuidance.Productivity(jobs);theme.Text(card.x+15,card.yMax-30,"TODAY",16,1,80);theme.Line(card.x+90,card.yMax-18,card.width-152,false,ShopUITheme.Rule,5);theme.Line(card.x+90,card.yMax-18,(card.width-152)*progress,false,ShopUITheme.Green,5);theme.Text(card.xMax-12,card.yMax-31,Mathf.RoundToInt(progress*100)+"%",18,1,62,2);
        }
        void Place(Rect map,Vector3 position,string name,string icon,bool large)
        {
            var p=Point(map,position);float w=large?106:68,h=large?56:35;
            ShopUITheme.Fill(new Rect(p.x-w/2,p.y-h/2,w,h),ShopUITheme.Rule);
            ShopUITheme.Fill(new Rect(p.x-w/2+2,p.y-h/2+2,w-4,h-4),ShopUITheme.Paper);
            theme.Text(p.x,p.y-h/2+4,name,large?18:13,1,w-4,1);
            Icon(icon,new Rect(p.x-8,p.y+1,16,16),.6f);
        }
    }
}
