using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using BusinessAsUsual.Phase8B;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.Town;
namespace BusinessAsUsual.RescueV2 {
    public sealed partial class RescueInterface {
        TextMeshProUGUI loadingValue,loadingTip,destinationName,destinationDistance,dayHeader;
        RectTransform destinationArrow;string hubFingerprint="",privateFingerprint="",pinnedErrand="",displayFingerprint="";bool firstPrivate,roomKnown,wasInside;float setupAt,roomUntil,nextSoloOpen;int pendingProfession;
        public BusinessAsUsual.Prototype.Interactable PinnedErrand=>Workday&&pinnedErrand!=""&&Workday.Stations.TryGetValue(pinnedErrand,out var point)?point:null;
        SuspiciousDayService Day=>NetworkShop.Instance?NetworkShop.Instance.Day:null;
        bool IsHost=>session&&session.Manager.IsHost;
        PrivateDayCopy Private=>Day?Day.Local:new PrivateDayCopy();
        bool BuildPhase8BScreen(string screen){
            if(screen=="Staff"&&Day&&Day.Public.started&&player&&Job!=Profession.None){ShiftCrew();return true;}
            switch(screen){case "Main":NeighborhoodMain();return true;case "Join":NeighborhoodJoin();return true;case "Credits":Credits();return true;case "Loading":case "Assigning":case "WorkplaceEntry":Loading();return true;case "Hub":Hub();return true;case "Private":PrivateSlip();return true;case "Board":StaffBoard();return true;case "ConnectionError":ConnectionError();return true;case "Report":DayReport();return true;case "Settings":NeighborhoodSettings();return true;}
            return false;
        }
        void ShiftCrew(){introduced=Job;var c=Card("Current shift crew",640,388);Text(c,"DAY 1 / STAFF",28,24,584,23,14,true);Text(c,"Your shift crew",28,64,584,48,32,true,true);Text(c,(Workday.IsAssistant(player.OwnerClientId)?"Helping ":"Working as ")+Job+" today",28,142,584,32,22,true);Text(c,"Keep this work slip for the day. Choose another job next shift.",28,193,584,54,18);Button(c,"SHARED COPIES",28,269,280,42,()=>Show("Board"));Button(c,"WORK SLIP",328,269,284,42,()=>{SelectedTab=0;Show("Handbook");});Button(c,"BACK TO WORK",28,327,584,34,()=>Show("HUD"),true);}
        void NeighborhoodMain(){
            firstPrivate=false;privateFingerprint="";introduced=Profession.None;pinnedErrand="";roomKnown=false;
            var c=Card("Neighborhood menu",444,616);c.anchorMin=c.anchorMax=new Vector2(0,.5f);c.anchoredPosition=new Vector2(46,308);
            Text(c,"DAYLIGHT NEIGHBORHOOD",28,25,388,22,14,true);Text(c,"Business\nas Usual",28,61,388,128,48,true,true);
            Text(c,"Ordinary jobs.\nExtraordinary suspicion.",28,190,388,54,20);
            string[] names={"START SOLO","HOST GAME","JOIN GAME","SETTINGS","CREDITS","QUIT"};Action[] actions={(()=>Phase8BFlow.Launch("solo")),(()=>Phase8BFlow.Launch("host")),(()=>Show("Join")),(()=>Show("Settings")),(()=>Show("Credits")),(()=>Application.Quit())};
            for(int i=0;i<6;i++)Button(c,names[i],28,262+i*47,388,39,actions[i],i==0);
            Text(c,"FIRST SUSPICIOUS DAY / PHASE 8B",28,566,388,24,14,true);
        }
        void NeighborhoodJoin(){var c=Card("Join neighborhood",540,354);Text(c,"Join the neighborhood",28,28,484,50,30,true,true);Text(c,"Host IPv4 address",28,104,484,24,18,true);
            var bg=Plate("Host address entry",c,28,143,484,44,skin.white);bg.raycastTarget=true;var field=bg.gameObject.AddComponent<TMP_InputField>();var text=Text(bg.transform,Phase8BFlow.Address,12,7,460,30,20);text.raycastTarget=true;field.textComponent=text;field.textViewport=bg.rectTransform;field.text=Phase8BFlow.Address;field.onValueChanged.AddListener(s=>Phase8BFlow.Address=s);
            Button(c,"JOIN GAME",28,217,230,42,()=>{if(System.Net.IPAddress.TryParse(Phase8BFlow.Address,out var ip)&&ip.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork)Phase8BFlow.Launch("client");else text.text="Enter an IPv4 address";},true);Button(c,"BACK",270,217,242,42,()=>Show("Main"));Text(c,"Same build · host must be reachable on UDP 7777.",28,289,484,35,16);
        }
        void Loading(){var c=Card("Loading neighborhood",540,338);Text(c,Phase8BFlow.LoadingLabel,28,26,484,62,30,true,true);Icon(c,"map",28,112,100);loadingTip=Text(c,Phase8BFlow.Tip,151,113,361,78,20);Plate("Loading rule",c,28,225,484,6,skin.rule);loadingValue=Text(c,"",28,256,484,37,20,true);loadingValue.alignment=TextAlignmentOptions.Center;}
        void ConnectionError(){var c=Card("Connection result",540,296);Text(c,"Could not check in",28,30,484,50,32,true,true);Text(c,session?session.Status:"Host unavailable",28,117,484,68,18);Button(c,"BACK TO MENU",28,220,484,42,()=>Phase8BFlow.ReturnToMenu(),true);}
        void Hub(){var c=Card("Daylight staff hub",700,508);Text(c,"DAY 1 / STAFF HUB",28,24,644,23,14,true);Text(c,"A shared day in Daylight",28,60,644,47,32,true,true);Icon(c,"map",542,36,100);
            var owners=NetworkShop.Instance.Town.Owners;for(int i=0;i<4;i++){float x=28+(i%2)*330,y=128+(i/2)*91;Plate("Staff place "+i,c,x,y,314,77,skin.white);string label=i<3?((Profession)(i+1)).ToString():"Shift assistant";Text(c,label,x+12,y+8,290,25,20,true);Text(c,owners[i]==NetworkShop.Nobody?(i==3?"Optional helping hand":"Unstaffed · town service"):Day.Public.ready.Contains(owners[i])?"Checked in · READY":"Checked in · reading brief",x+12,y+41,290,24,16);}
            Text(c,"One parcel. Three workplaces. Different paper copies.",28,336,644,31,18);Button(c,"CHOOSE JOB",28,384,200,40,()=>Show("Staff"));Button(c,"STAFF BOARD",248,384,200,40,()=>Show("Board"));
            bool ready=Day.Public.ready.Contains(player.OwnerClientId);Button(c,ready?"READY":"I'M READY",468,384,204,40,()=>player.SendCommand("day:ready"),true,!ready);
            if(IsHost)Button(c,"OPEN THE DAY",28,444,644,40,()=>player.SendCommand("day:start"),true,Day.Public.ready.Length==session.Manager.ConnectedClientsIds.Count);
            else Text(c,"The host opens the day when everyone is ready.",28,445,644,31,16);
        }
        void EnterPhase8BWork(){if(!Phase8BFlow.Enabled||Day&&Day.Public.started){introduced=Job;Show("HUD");return;}player.SendCommand("day:ready");Show("Hub");}
        void PrivateSlip(){var c=Card("Owner-only work note",640,514);Text(c,"PRIVATE / YOUR COPY",24,23,548,23,14,true);Text(c,"A little unfinished business",24,58,592,58,29,true,true);Button(c,"×",561,22,55,42,()=>Show("HUD"));
            Plate("Personal note",c,24,141,592,103,skin.white);Icon(c,"receipt",38,158,64);Text(c,Private.personalTitle??"Your work note",117,150,474,30,22,true);Text(c,Private.personalNote??"Keep working. Your note is private.",117,188,474,46,16);
            if(Private.hasArrangement){Text(c,Private.practice?"SOLO PRACTICE / NO HUMAN ACCUSATIONS":"AN ADDITIONAL PRIVATE ARRANGEMENT",24,264,592,23,14,true);string[] labels={"RESTAGE","DIRECT NOTE","USEFUL WORK"};bool[] done={Private.detour,Private.declaredDirect,Private.productive};string[] icons={"parcel","sign","check"};for(int i=0;i<3;i++){var p=Plate(labels[i],c,24+i*200,303,192,90,done[i]?skin.green:skin.white);Icon(p.transform,done[i]?"check":icons[i],71,6,50);Text(p.transform,labels[i],6,62,180,23,14,true,false,done[i]?skin.white:skin.ink).alignment=TextAlignmentOptions.Center;}Text(c,"E restage / R change slip / deliver / do useful work.",24,407,592,34,16);}
            else Text(c,"Your work can look suspicious without a bad intention.",24,303,592,65,20);
            Button(c,!Private.hasArrangement&&Job==Profession.Mechanic?"PIN SERVICE NOTE":"PIN RETURNS",24,457,185,34,()=>{pinnedErrand=!Private.hasArrangement&&Job==Profession.Mechanic?"Service amendment":"Returns counter";Show("HUD");});Button(c,"PIN YARD HOLD",224,457,185,34,()=>{pinnedErrand="Yard holding";Show("HUD");});Button(c,"BACK TO WORK",424,457,192,34,()=>{pinnedErrand="";Show("HUD");},true);
        }
        void StaffBoard(){var c=Card("Shared staff board",660,494);Text(c,"SHARED COPIES / STATEMENTS",28,24,604,22,14,true);Text(c,"Compare the paperwork",28,60,604,47,30,true,true);var papers=Day?Day.Public.papers:Array.Empty<SharedPaper>();
            if(papers.Length==0){Icon(c,"receipt",245,143,140);Text(c,"No copies pinned yet.",28,318,604,36,22,true).alignment=TextAlignmentOptions.Center;Text(c,"RECORDS → PIN COPY shares only the selected slip.",28,365,604,31,16).alignment=TextAlignmentOptions.Center;}
            else{recordIndex=Mathf.Clamp(recordIndex,0,papers.Length-1);var p=papers[recordIndex];Plate("Shared copy",c,28,135,604,262,skin.white);Text(c,p.heading,44,151,572,40,24,true);Text(c,p.interval+" / "+p.attribution,44,203,572,43,16);Text(c,p.text,44,260,572,101,18);Button(c,"‹",28,413,54,34,()=>{recordIndex=(recordIndex+papers.Length-1)%papers.Length;Show("Board");});Button(c,"›",92,413,54,34,()=>{recordIndex=(recordIndex+1)%papers.Length;Show("Board");});}
            Button(c,"BACK",458,433,174,34,()=>Show(Day&&Day.Public.started?"HUD":"Hub"));
        }
        void Credits(){var c=Card("Credits and acknowledgements",640,546);Text(c,"Business as Usual",28,29,584,50,36,true,true);Text(c,"Zain Asad / project design and direction",28,104,584,29,20,true);Text(c,"Implementation collaboration / Codex",28,145,584,29,18);
            Text(c,"BUILT WITH",28,205,584,22,14,true);Text(c,"Unity · TextMesh Pro · Blender · Meshy T2",28,239,584,44,18);Text(c,"Original workday illustrations and ambient music.",28,286,584,31,16);
            Text(c,"TYPE & THANKS",28,346,584,22,14,true);Text(c,"Bricolage Grotesque · Atkinson Hyperlegible\nFont authors credited in bundled SIL OFL notices.\nThe reference research informed this original town.",28,380,584,84,17);Button(c,"BACK",28,486,584,34,()=>Show("Main"));
        }
        void NeighborhoodSettings(){var c=Card("Personal settings",620,572);Text(c,"Make yourself comfortable",28,24,564,51,30,true,true);int y=96;
            void Row(string name,string value,Action minus,Action plus){Text(c,name,28,y,232,31,18,true);Text(c,value,264,y,160,31,18).alignment=TextAlignmentOptions.Center;Button(c,"−",438,y,66,31,minus);Button(c,"+",514,y,78,31,plus);y+=48;}
            void Refresh(Action action){action();Show("Settings");}
            Row("Master volume",Mathf.RoundToInt(GameSettings.Master*100)+"%",()=>Refresh(()=>GameSettings.SetMaster(GameSettings.Master-.1f)),()=>Refresh(()=>GameSettings.SetMaster(GameSettings.Master+.1f)));
            Row("Music",Mathf.RoundToInt(GameSettings.Music*100)+"%",()=>Refresh(()=>GameSettings.SetMusic(GameSettings.Music-.1f)),()=>Refresh(()=>GameSettings.SetMusic(GameSettings.Music+.1f)));
            Row("Sound effects",Mathf.RoundToInt(GameSettings.Effects*100)+"%",()=>Refresh(()=>GameSettings.SetEffects(GameSettings.Effects-.1f)),()=>Refresh(()=>GameSettings.SetEffects(GameSettings.Effects+.1f)));
            Row("Look sensitivity",GameSettings.Sensitivity.ToString("0.00"),()=>Refresh(()=>GameSettings.SetSensitivity(GameSettings.Sensitivity-.02f)),()=>Refresh(()=>GameSettings.SetSensitivity(GameSettings.Sensitivity+.02f)));
            Row("Field of view",GameSettings.FieldOfView+"°",()=>Refresh(()=>GameSettings.SetFieldOfView(GameSettings.FieldOfView-5)),()=>Refresh(()=>GameSettings.SetFieldOfView(GameSettings.FieldOfView+5)));
            Button(c,"REDUCED MOTION / "+(ReducedMotion?"ON":"OFF"),28,348,564,34,()=>{ReducedMotion=!ReducedMotion;PlayerPrefs.SetInt("bau.rescue.reducedMotion",ReducedMotion?1:0);PlayerPrefs.Save();Show("Settings");});Button(c,"DISPLAY & QUALITY",28,397,564,34,()=>Show("Display"));Button(c,"CONTROLS",28,446,564,34,()=>Show("Controls"));Button(c,"BACK",28,515,564,34,()=>Show(player?"Pause":"Main"));
        }
        void DayReport(){var s=Workday.Domain.State;var c=Card("Day 1 report",660,526);Text(c,"DAY 1 / SHIFT SUMMARY",28,24,604,23,14,true);Text(c,s.shiftSigned?"A day in Daylight":"The workday has ended",28,62,604,50,34,true,true);
            string[] names={"CUSTOMERS","REPAIR","COLLECTION"},icons={"checkout","wrench","cart"},values={s.customerOrders+" served",s.serviceSigned?"Tested & signed":"Unfinished",s.items.Count(i=>i.custody=="recorded")+" / 3 filed"};for(int i=0;i<3;i++){var p=Plate(names[i],c,28+i*206,140,192,154,skin.white);Icon(p.transform,icons[i],62,12,68);Text(p.transform,names[i],8,88,176,25,14,true).alignment=TextAlignmentOptions.Center;Text(p.transform,values[i],8,120,176,24,18,true).alignment=TextAlignmentOptions.Center;}
            Text(c,"Town supply  "+s.stability+" / 20",28,319,604,35,24,true).alignment=TextAlignmentOptions.Center;Text(c,Private.hasArrangement?(Private.practice?"Practice errand": "Your private arrangement")+" / "+Private.Progress+" of 3":Private.personalDone?"Your personal errand was handled.":"Your personal note stays private.",28,372,604,46,18).alignment=TextAlignmentOptions.Center;
            Button(c,"COMPARE COPIES",28,454,292,43,()=>Show("Board"));Button(c,"RETURN TO MENU",340,454,292,43,()=>Phase8BFlow.ReturnToMenu(),true);
        }
        void BuildPhase8BDestination(){foreach(Transform child in routePin)Destroy(child.gameObject);routePin.sizeDelta=new Vector2(182,68);Plate("Named route card",routePin,0,0,182,68,skin.paper);var arrow=Plate("Direction",routePin,8,19,25,25,skin.mustard);var texture=new Texture2D(32,32);for(int y=0;y<32;y++)for(int x=0;x<32;x++)texture.SetPixel(x,y,(y>10&&Mathf.Abs(x-16)<(31-y)*.7f||y<=17&&Mathf.Abs(x-16)<4)?Color.white:Color.clear);texture.Apply();arrow.sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f));arrow.type=Image.Type.Simple;destinationArrow=arrow.rectTransform;destinationArrow.pivot=Mid;destinationArrow.anchoredPosition+=new Vector2(12.5f,-12.5f);destinationName=Text(routePin,"",39,7,133,32,14,true);destinationDistance=Text(routePin,"",39,43,133,18,13);dayHeader=Text(hud,"DAY 1 / OPENING",0,125,326,23,14,true,false,skin.white);}
        void Phase8BDestination(TownGuidance guide){routePin.gameObject.SetActive(false);if(!FirstPersonView.Local||ScreenName!="HUD"||guide?.Primary==null)return;var p=guide.Primary;var camera=FirstPersonView.Local.ViewCamera;Vector3 local=camera.transform.InverseTransformPoint(p.Position);float distance=Vector3.Distance(player.transform.position,p.Position);if(distance<2.1f)return;
            var view=camera.WorldToViewportPoint(p.Position);var size=root.rect.size;bool onScreen=local.z>0&&view.x>.12f&&view.x<.82f&&view.y>.18f&&view.y<.82f;routePin.anchoredPosition=onScreen?new Vector2(Mathf.Clamp(view.x*size.x,360,size.x-204),-Mathf.Clamp((1-view.y)*size.y,170,size.y-164)):new Vector2(size.x/2-91,-size.y+184);
            float angle=Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg;destinationArrow.localRotation=Quaternion.Euler(0,0,-angle);destinationName.text=p.target is BusinessAsUsual.Shopkeeper.ShopStation?"TOWN EXIT":p.target is TownStation?"MARKET ENTRY":p.target is BusinessAsUsual.Prototype.SimpleDoor?"OPEN MARKET":p.target.name.StartsWith("Stock box")?"STOCKROOM":p.target.name.StartsWith("Stock shelf")?"MATCHING SHELF":p.target.name.StartsWith("Shelf product")?"ORDER SHELF":p.target.name=="Shipment"?"MARKET STOCKROOM":p.target.name=="Shift report"?"STAFF KIOSK":p.title.Replace("Daylight ","").ToUpperInvariant();destinationDistance.text=(onScreen?"":"Turn "+(angle<0?"left":"right")+" / ")+Mathf.RoundToInt(distance)+" m";routePin.gameObject.SetActive(true);
        }
        bool TickPhase8B(){
            if(ScreenName=="Display"){string f=Screen.width+":"+Screen.height+":"+Screen.fullScreenMode+":"+GameSettings.Quality;if(f!=displayFingerprint){displayFingerprint=f;Show("Display");}}
            if(Phase8BFlow.Transitioning){if(ScreenName!="Loading")Show("Loading");if(loadingValue)loadingValue.text=Mathf.RoundToInt(Phase8BFlow.Progress*100)+"% / "+Phase8BFlow.LoadingLabel;if(loadingTip)loadingTip.text=Phase8BFlow.Tip;return true;}
            if(!Day||!player)return false;
            if(ScreenName=="Assigning"){if(loadingValue)loadingValue.text=Job==(Profession)pendingProfession?"Work slip ready":"Checking your assignment";if(Job==(Profession)pendingProfession&&Time.unscaledTime-setupAt>.45f){introPage=0;Show("Intro");}else if(Time.unscaledTime-setupAt>4)Show("Staff");return true;}
            if(ScreenName=="WorkplaceEntry"){if(Time.unscaledTime>roomUntil)Show("HUD");return true;}
            bool inside=player.transform.position.x<-25;if(!roomKnown){roomKnown=true;wasInside=inside;}else if(inside!=wasInside&&ScreenName=="HUD"){wasInside=inside;Phase8BFlow.LoadingLabel=inside?"Daylight Market":"Back in the neighborhood";Phase8BFlow.Tip=inside?"Deliveries and labelled stock are at the back.":"Shared holds are for returns and temporary collection.";Phase8BFlow.Progress=1;roomUntil=Time.unscaledTime+.35f;Show("WorkplaceEntry");return true;}
            if(!Day.Public.started){if(ScreenName=="HUD")Show("Hub");if(ScreenName=="Hub"){string f=string.Join(",",Day.Public.ready)+string.Join(",",NetworkShop.Instance.Town.Owners);if(f!=hubFingerprint){hubFingerprint=f;Show("Hub");}if(session.Solo&&Day.Public.ready.Contains(player.OwnerClientId)&&Time.unscaledTime>nextSoloOpen){nextSoloOpen=Time.unscaledTime+.4f;player.SendCommand("day:start");}}return false;}
            if(!firstPrivate&&!string.IsNullOrEmpty(Private.personalTitle)){firstPrivate=true;Show("Private");}
            if(ScreenName=="Private"){string f=Private.hasArrangement+":"+Private.Progress+":"+Private.personalDone;if(f!=privateFingerprint){privateFingerprint=f;Show("Private");}}
            if(ScreenName=="Private"&&Keyboard.current!=null&&Keyboard.current.enterKey.wasPressedThisFrame)Show("HUD");
            if(dayHeader)dayHeader.text="DAY 1 / "+(Day.Public.band==3?"LAST COLLECTIONS":Day.Public.band==2?"MIDDAY RUSH":"OPENING WORK");return false;
        }
    }
}
