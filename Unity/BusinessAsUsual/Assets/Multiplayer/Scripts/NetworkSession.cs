using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Multiplayer
{
    public sealed class NetworkSession : MonoBehaviour
    {
        public static NetworkSession Instance { get; private set; }
        public static bool Active => Instance&&Instance.Manager&&Instance.Manager.IsListening;
        public NetworkManager Manager;
        public Camera PreviewCamera;
        public string Address="127.0.0.1";
        public ushort Port=7777;
        public string Status="Offline / choose Host or Client";
        public bool Solo {get;private set;}
        ShopUITheme theme;
        GUIStyle addressStyle;
        readonly System.Collections.Generic.Dictionary<ulong,string> identities=new System.Collections.Generic.Dictionary<ulong,string>();
        public string IdentityFor(ulong id)=>identities.TryGetValue(id,out var value)?value:"";
        bool started,reloading;
        float startedAt;
        void Awake()
        {
            Instance=this; theme=new ShopUITheme(); Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
            Manager.OnClientConnectedCallback+=Connected;
            Manager.OnClientDisconnectCallback+=Disconnected;
            Manager.NetworkConfig.ConnectionApproval=true;
            Manager.ConnectionApprovalCallback=(request,response)=> { if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)identities[request.ClientNetworkId]=System.Text.Encoding.UTF8.GetString(request.Payload??System.Array.Empty<byte>()); int capacity=Solo?1:FindFirstObjectByType<BusinessAsUsual.Town.TownJobs>()?4:2; response.Approved=Manager.ConnectedClientsIds.Count<capacity; response.CreatePlayerObject=response.Approved; response.Reason=response.Approved?"":"This shift is full ("+capacity+" players)."; response.Pending=false; };
        }
        void Connected(ulong id) { Status=(Manager.IsHost?"HOST":"CLIENT")+" / Sunny "+Manager.LocalClientId+" / "+Manager.ConnectedClientsIds.Count+" players"; }
        System.Collections.IEnumerator Start()
        {
            var args=System.Environment.GetCommandLineArgs();string mode="";
            for(int i=0;i<args.Length-1;i++){if(args[i]=="--bau-mode")mode=args[i+1];if(args[i]=="--bau-address")Address=args[i+1];if(args[i]=="--bau-port"&&ushort.TryParse(args[i+1],out var requestedPort))Port=requestedPort;}
            yield return null;
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)yield break;
            if(mode=="solo")StartSolo();else if(mode=="single")SinglePlayer();else if(mode=="host"||mode=="client")StartSession(mode=="host");
        }
        void Disconnected(ulong id)
        {
            if(!Manager.IsServer||id==Manager.LocalClientId) { Status="Disconnected / host unavailable"; Stop(); }
            else Status="HOST / colleague disconnected";
        }
        public bool StartSession(bool host)
        {
            if(started)return false;
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)Manager.NetworkConfig.ConnectionData=System.Text.Encoding.UTF8.GetBytes(BusinessAsUsual.Phase8B.Phase8BFlow.ClientIdentity);
            if(!System.Net.IPAddress.TryParse(Address,out var ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork){Status="Enter an IPv4 address.";return false;}
            Manager.GetComponent<UnityTransport>().SetConnectionData(Address,Port,host?(Solo?"127.0.0.1":"0.0.0.0"):null);
            started=true; startedAt=Time.unscaledTime; Status=host?"Starting host...":"Connecting...";
            bool ok=host?Manager.StartHost():Manager.StartClient();
            if(!ok){started=false;Status="Could not start session. Check the port/address.";}return ok;
        }
        public void Stop()
        {
            if(reloading)return; reloading=true; if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled){BusinessAsUsual.Phase8B.Phase8BFlow.ReturnToMenu();return;}Manager.Shutdown();
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            if(!NetworkTestDriver.Automated)Invoke(nameof(Reload),.5f);
        }
        void Reload() { var scene=SceneManager.GetActiveScene();SceneManager.MoveGameObjectToScene(gameObject,scene);SceneManager.LoadScene(scene.name); }
        public bool StartSolo(){
            if(started)return false;Solo=true;Address="127.0.0.1";
            using(var socket=new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Dgram,System.Net.Sockets.ProtocolType.Udp)){
                socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback,0));Port=(ushort)((System.Net.IPEndPoint)socket.LocalEndPoint).Port;
            }
            return StartSession(true);
        }
        public void SinglePlayer() { if(FindFirstObjectByType<BusinessAsUsual.Town.TownJobs>()&&BusinessAsUsual.RescueV2.RescueInterface.Requested){StartSolo();return;}SceneManager.MoveGameObjectToScene(gameObject,SceneManager.GetActiveScene());SceneManager.LoadScene("ShopkeeperShift"); }
        void Update() { if(started&&!reloading&&!Manager.IsConnectedClient&&Time.unscaledTime-startedAt>12){Status="Connection timed out.";Stop();} }
        void OnDestroy() { if(Instance==this)Instance=null; if(Manager){Manager.OnClientConnectedCallback-=Connected;Manager.OnClientDisconnectCallback-=Disconnected;} }
        void OnGUI()
        {
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance)return;
            if(started&&Manager.IsConnectedClient)Status=(Manager.IsHost?"HOST":"CLIENT")+" / Sunny "+Manager.LocalClientId+" / "+FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Count(p=>p.IsSpawned)+" players";
            var old=GUI.matrix;float scale=ShopHUD.Scale(Screen.width,Screen.height);var offset=ShopHUD.Offset(Screen.width,Screen.height,scale);
            GUI.matrix=Matrix4x4.TRS(new Vector3(offset.x,offset.y,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(!started) {
                ShopUITheme.Fill(new Rect(570,280,780,500),ShopUITheme.Paper);theme.Text(620,316,"Staff connection desk",40,2,660);
                theme.Text(620,390,FindFirstObjectByType<BusinessAsUsual.Town.TownJobs>()?"Four-player LAN / living workday":"Two-player LAN / localhost shift",26,0,650);theme.Text(620,444,"Host IPv4 address / UDP 7777",24,1,650);
                if(addressStyle==null){addressStyle=new GUIStyle(GUI.skin.textField){font=Resources.Load<Font>("Handbook/Fonts/RoundedMplus1c-Regular"),fontSize=28};addressStyle.normal.textColor=ShopUITheme.Ink;}
                Address=GUI.TextField(new Rect(620,492,670,52),Address,48,addressStyle);
                theme.Pill(620,578,310,58,"START HOST");if(ShopUITheme.Hit(new Rect(620,578,310,58)))StartSession(true);
                theme.Pill(980,578,310,58,"START CLIENT");if(ShopUITheme.Hit(new Rect(980,578,310,58)))StartSession(false);
                theme.Text(620,678,Status,24,0,660,0,true);
                theme.Pill(620,722,670,42,"SINGLE PLAYER",ShopUITheme.Pale,22);if(ShopUITheme.Hit(new Rect(620,722,670,42)))SinglePlayer();
            } else {
                var hud=FindObjectsByType<ShopHUD>(FindObjectsSortMode.None).FirstOrDefault(h=>h.isActiveAndEnabled);
                var townHud=FindObjectsByType<BusinessAsUsual.Town.TownHandbook>(FindObjectsSortMode.None).FirstOrDefault(h=>h.isActiveAndEnabled);
                if(townHud&&(townHud.Expanded||townHud.Choosing||townHud.LocalJob==BusinessAsUsual.Town.Profession.None)){
                    ShopUITheme.Fill(new Rect(48,1032,1824,34),ShopUITheme.Paper);theme.Text(340,1034,Status+" / shared town",18,1,1500);
                    if(Cursor.lockState!=CursorLockMode.Locked){theme.Pill(48,1032,260,34,"DISCONNECT",ShopUITheme.Pale,18);if(ShopUITheme.Hit(new Rect(48,1032,260,34)))Stop();}GUI.matrix=old;return;
                }
                if(townHud){
                    if(!BusinessAsUsual.Town.LivingWorkday.Instance)theme.Text(54,190,(Manager.IsHost?"HOST":"CLIENT")+" / "+Manager.ConnectedClientsIds.Count+" STAFF",17,1,400);
                    if(Cursor.lockState!=CursorLockMode.Locked){theme.Pill(550,46,170,38,"DISCONNECT",ShopUITheme.Pale,18);if(ShopUITheme.Hit(new Rect(550,46,170,38)))Stop();}GUI.matrix=old;return;
                }
                if(hud&&hud.CurrentPanel!=ShopHUD.Panel.None){ShopUITheme.Fill(new Rect(48,1032,1824,34),ShopUITheme.Paper);theme.Text(66,1034,Status+" / shared shift",18,1,1700);GUI.matrix=old;return;}
                ShopUITheme.Fill(new Rect(48,172,510,78),ShopUITheme.Paper);theme.Text(66,183,Status,20,1,472);
                theme.Text(66,213,townHud?"E work / hold E timed jobs / J professions":"Shared basket / E shelf or drop / Q return",18,0,472);
                if(Cursor.lockState!=CursorLockMode.Locked) { theme.Pill(48,267,260,40,"DISCONNECT",ShopUITheme.Pale,20);if(ShopUITheme.Hit(new Rect(48,267,260,40)))Stop(); }
            }
            GUI.matrix=old;
        }
    }
}
