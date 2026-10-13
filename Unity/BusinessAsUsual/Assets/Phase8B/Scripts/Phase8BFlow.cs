using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.RescueV2;
using BusinessAsUsual.FirstPerson;
namespace BusinessAsUsual.Phase8B {
    public sealed class Phase8BFlow:MonoBehaviour {
        public static Phase8BFlow Instance {get;private set;}
        static bool enabledMode;
        public static bool Enabled=>enabledMode||Environment.GetCommandLineArgs().Contains("--bau-phase8b")||SceneManager.GetActiveScene().name=="Phase8BFrontEnd";
        public static string Address="127.0.0.1",Mode="";public static float Progress;public static string LoadingLabel="Opening the neighborhood",Tip="Ordinary jobs, extraordinary suspicion.";public static bool Transitioning;
        public static readonly string ClientIdentity=Guid.NewGuid().ToString("N");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(!Enabled)return;enabledMode=true;if(!Instance)new GameObject("Phase 8B flow").AddComponent<Phase8BFlow>();}
        void Awake(){if(Instance){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);GameSettings.EnsureLoaded();SceneManager.sceneLoaded+=Loaded;
            if(!Environment.GetCommandLineArgs().Contains("-screen-width")&&GameSettings.Resolution.x>0)GameSettings.SetDisplay(GameSettings.DisplayMode,GameSettings.Resolution);
            if(!RescueInterface.Instance)Instantiate(Resources.Load<GameObject>("RescueV2/WorkdayCanvas"));
            if(NetworkTestDriver.Requested&&!FindFirstObjectByType<NetworkTestDriver>())new GameObject("Phase 8B native diagnostics").AddComponent<NetworkTestDriver>();
            if(!FindFirstObjectByType<Phase8BMusic>())gameObject.AddComponent<Phase8BMusic>();
        }
        void Loaded(Scene scene,LoadSceneMode mode){ if(scene.name=="DaylightTown"&&NetworkTestDriver.Requested){var drivers=FindObjectsByType<NetworkTestDriver>(FindObjectsSortMode.None);foreach(var driver in drivers)if(driver.gameObject.scene.name!="DontDestroyOnLoad")Destroy(driver);}if(scene.name=="Phase8BFrontEnd"&&!RescueInterface.Instance)Instantiate(Resources.Load<GameObject>("RescueV2/WorkdayCanvas"));}
        void OnDestroy(){SceneManager.sceneLoaded-=Loaded;if(Instance==this)Instance=null;}
        public static void Launch(string mode){if(Instance&&!Transitioning)Instance.StartCoroutine(Instance.Enter(mode));}
        IEnumerator Enter(string mode){Mode=mode;Transitioning=true;Progress=0;LoadingLabel="Opening Daylight";Tip="Choose a workplace at the staff hub.";RescueInterface.Instance.Show("Loading");
            var op=SceneManager.LoadSceneAsync("DaylightTown");op.allowSceneActivation=false;float began=Time.unscaledTime;
            while(op.progress<.9f||Time.unscaledTime-began<.65f){Progress=Mathf.Clamp01(op.progress/.9f)*.8f;yield return null;}op.allowSceneActivation=true;while(!op.isDone)yield return null;
            var session=NetworkSession.Instance;session.Address=Address;var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--bau-port");if(index>=0&&index+1<args.Length&&ushort.TryParse(args[index+1],out var port))session.Port=port;
            Progress=.9f;LoadingLabel="Checking in at the staff hub";bool ok=mode=="solo"?session.StartSolo():session.StartSession(mode=="host");
            float until=Time.unscaledTime+14;while(ok&&!session.Manager.IsConnectedClient&&Time.unscaledTime<until)yield return null;
            Progress=1;yield return new WaitForSecondsRealtime(.25f);Transitioning=false;
            RescueInterface.Instance.Show(session.Manager.IsConnectedClient?"Staff":"ConnectionError");
        }
        public static void ReturnToMenu(){if(Instance&&!Transitioning)Instance.StartCoroutine(Instance.Leave());}
        IEnumerator Leave(){Transitioning=true;Progress=.1f;LoadingLabel="Filing the workday";Tip="Receipts record a handoff. They do not prove a motive.";RescueInterface.Instance.Show("Loading");
            var oldSession=NetworkSession.Instance;if(oldSession)oldSession.Manager.Shutdown();yield return new WaitForSecondsRealtime(.3f);
            if(oldSession)Destroy(oldSession.Manager.gameObject);yield return null;
            var op=SceneManager.LoadSceneAsync("Phase8BFrontEnd");while(!op.isDone){Progress=Mathf.Clamp01(op.progress/.9f);yield return null;}Progress=1;Transitioning=false;RescueInterface.Instance.Show("Main");
        }
    }
    public sealed class Phase8BMusic:MonoBehaviour {
        AudioSource source;void Awake(){source=gameObject.AddComponent<AudioSource>();source.loop=true;source.spatialBlend=0;const int rate=22050;var data=new float[rate*12];float[] chord={130.81f,164.81f,196f,246.94f};
            for(int i=0;i<data.Length;i++){float t=(float)i/rate,v=0;for(int n=0;n<chord.Length;n++)v+=Mathf.Sin(t*chord[n]*Mathf.PI*2)*.018f*(.65f+.35f*Mathf.Sin(t*.45f+n));data[i]=v;}var clip=AudioClip.Create("Original Daylight ambient chord",data.Length,1,rate,false);clip.SetData(data,0);source.clip=clip;source.Play();}
        void Update(){source.volume=GameSettings.Music;}
    }
}
