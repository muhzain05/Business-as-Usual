using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.Town;
using BusinessAsUsual.FirstPerson;

namespace BusinessAsUsual.Multiplayer
{
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        public readonly NetworkVariable<float> Speed = new NetworkVariable<float>();
        public readonly NetworkVariable<float> Facing = new NetworkVariable<float>();
        public readonly NetworkVariable<bool> Grounded = new NetworkVariable<bool>();
        public readonly NetworkVariable<bool> Running = new NetworkVariable<bool>();
        public readonly NetworkVariable<bool> Jumping = new NetworkVariable<bool>();
        // Public look direction for remote head turns (Phase 7A); never private information.
        public readonly NetworkVariable<float> LookYaw = new NetworkVariable<float>();
        public readonly NetworkVariable<float> LookPitch = new NetworkVariable<float>();
        public ThirdPersonMotor Motor { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        PrototypeInput input;
        InputAction returnCrate;
        Transform direction;
        Vector2 movement;
        bool sprint, jump;
        float lookYaw, lookPitch;
        FirstPersonView firstPerson;
        FirstPersonHands hands;
        PauseMenu pause;
        public bool FirstPersonPrefab => firstPerson;
        public bool CarryingLoad => NetworkShop.Instance&&(NetworkShop.Instance.Holder==OwnerClientId||(NetworkShop.Instance.Town&&NetworkShop.Instance.Town.HasBag(OwnerClientId))||(NetworkShop.Instance.Workday&&(NetworkShop.Instance.Workday.Holding(OwnerClientId)||NetworkShop.Instance.Workday.Domain.State.cartPusher==OwnerClientId)));
        float lastInput, nextSend, nextState;
        uint inputSequence, acceptedInput, commandSequence;
        public int InputsSent { get; private set; }
        public int InputsAccepted { get; private set; }
        public int CommandsSent { get; private set; }
        public string LastFeedback { get; private set; }
        public bool WorkHeld { get; private set; }
        bool testHold;
        public void TestHold(bool held){if(NetworkTestDriver.Automated&&IsOwner)testHold=held;}
        public void TestMove(Vector2 move,bool run,bool leap,float yaw)
        { if(!NetworkTestDriver.Automated||!IsOwner)return; bool fp=firstPerson&&firstPerson.Active; // idle harness packets keep the first-person look
          InputServerRpc(move,run,leap,fp&&move.sqrMagnitude<.0001f?firstPerson.Yaw:yaw,fp?firstPerson.Pitch:0,++inputSequence,testHold); }
        [ServerRpc]
        public void TestTeleportServerRpc(Vector3 position)
        { if(NetworkTestDriver.Automated)Motor.Teleport(position); }
        public void TestCommand(string target,uint sequence,int revision,string order)
        { if(NetworkTestDriver.Automated&&IsOwner){commandSequence=System.Math.Max(commandSequence,sequence);CommandServerRpc(target,sequence,revision,order);} }
        void Awake()
        {
            Motor=GetComponent<ThirdPersonMotor>(); Interactor=GetComponent<PlayerInteractor>();
            input=GetComponent<PrototypeInput>();
            firstPerson=GetComponent<FirstPersonView>(); hands=GetComponent<FirstPersonHands>(); pause=GetComponent<PauseMenu>();
            input.enabled=false; Motor.enabled=false; Interactor.enabled=false;
            GetComponent<ShopHUD>().enabled=false;
            GetComponent<CharacterController>().enabled=false;
            foreach(var c in GetComponentsInChildren<Camera>(true)) c.enabled=false;
            foreach(var a in GetComponentsInChildren<AudioListener>(true)) a.enabled=false;
            foreach(var c in GetComponentsInChildren<ThirdPersonCamera>(true)) c.enabled=false;
        }
        public override void OnNetworkSpawn()
        {
            gameObject.name="Sunny Player "+OwnerClientId;
            Motor.Controller.enabled=IsServer;
            if(IsServer) {
                direction=new GameObject("Server movement direction").transform;
                direction.SetParent(transform); Motor.cameraTransform=direction;
                var spawn=NetworkShop.Instance.Town?new Vector3(((int)(OwnerClientId%4)-1.5f)*.75f,.08f,-.8f):new Vector3(OwnerClientId==0?-.65f:.65f,.06f,3.2f);
                Motor.Teleport(spawn);Motor.respawnPosition=spawn;
                StartCoroutine(SendJoinState());
            }
            if(IsOwner) {
                if(NetworkSession.Instance.PreviewCamera)NetworkSession.Instance.PreviewCamera.enabled=false;
                input.enabled=true; Interactor.enabled=true;
                var hud=GetComponent<ShopHUD>(); hud.shift=NetworkShop.Instance.Shift; hud.player=Interactor; hud.enabled=!NetworkShop.Instance.Town;
                var townHud=GetComponent<TownHandbook>();if(townHud)townHud.enabled=true;
                var orbit=GetComponentInChildren<ThirdPersonCamera>(true); orbit.target=transform; orbit.input=input;
                orbit.GetComponent<Camera>().enabled=true; orbit.GetComponent<AudioListener>().enabled=true;
                // Phase 7A town prefab: first person replaces the orbit camera; same Camera/AudioListener.
                if(firstPerson){orbit.yaw=0;firstPerson.enabled=true;if(hands)hands.enabled=true;}else orbit.enabled=true;
                if(pause)pause.enabled=true;
                returnCrate=new InputAction("Return shared crate",InputActionType.Button,"<Keyboard>/q"); returnCrate.Enable();
            }
        }
        IEnumerator SendJoinState() { yield return new WaitForSeconds(.5f); if(IsSpawned) NetworkShop.Instance.Publish(); }
        void Update()
        {
            if(!IsSpawned)return;
            if(IsOwner && (!NetworkTestDriver.Automated||NetworkTestDriver.RealInput) && Time.unscaledTime>=nextSend) {
                nextSend=Time.unscaledTime+1f/30;
                var orbit=GetComponentInChildren<ThirdPersonCamera>(true);
                bool fp=firstPerson&&firstPerson.Active;
                InputServerRpc(input.GameplayActive?input.Move.ReadValue<Vector2>():Vector2.zero,
                    input.GameplayActive&&input.Sprint.IsPressed(),pendingJump,fp?firstPerson.Yaw:orbit.yaw,fp?firstPerson.Pitch:0,++inputSequence,input.GameplayActive&&input.Interact.IsPressed());
                pendingJump=false; InputsSent++;
            }
            if(IsOwner) {
                pendingJump |= input.GameplayActive&&input.Jump.WasPressedThisFrame();
                if(input.GameplayActive&&returnCrate.WasPressedThisFrame()) SendCommand("return");
            }
            if(IsServer) {
                if(Time.unscaledTime-lastInput>.3f) { movement=Vector2.zero; sprint=false; jump=false;WorkHeld=false; }
                bool pushing=NetworkShop.Instance&&NetworkShop.Instance.Workday&&NetworkShop.Instance.Workday.Domain.State.cartPusher==OwnerClientId;
                Motor.Step(movement,sprint&&!pushing,jump&&!pushing,Mathf.Min(Time.deltaTime,.05f)); jump=false;
                // First person: a standing Sunny turns to face where its player looks.
                if(firstPerson&&movement.sqrMagnitude<.01f)Motor.visual.rotation=Quaternion.RotateTowards(Motor.visual.rotation,Quaternion.Euler(0,lookYaw,0),360*Time.deltaTime);
                if(transform.position.y<Motor.respawnBelowY)Motor.Teleport(Motor.respawnPosition);
                if(Time.unscaledTime>=nextState) { nextState=Time.unscaledTime+1f/30; Speed.Value=Motor.PlanarSpeed; Facing.Value=Motor.visual.eulerAngles.y; Grounded.Value=Motor.Grounded; Running.Value=sprint&&!pushing&&Motor.PlanarSpeed>.2f; Jumping.Value=Motor.VerticalVelocity>0&&!Motor.Grounded; LookYaw.Value=lookYaw; LookPitch.Value=lookPitch; }
            } else {
                Motor.visual.rotation=Quaternion.Slerp(Motor.visual.rotation,Quaternion.Euler(0,Facing.Value,0),1-Mathf.Exp(-20*Time.deltaTime));
                Motor.animator.SetFloat("Speed",Speed.Value,.12f,Time.deltaTime);
            }
        }
        bool pendingJump;
        [ServerRpc(Delivery=RpcDelivery.Unreliable)]
        void InputServerRpc(Vector2 move, bool run, bool leap, float yaw, float pitch, uint sequence,bool workHeld)
        {
            if(sequence<=acceptedInput || !float.IsFinite(move.x)||!float.IsFinite(move.y)||!float.IsFinite(yaw)||!float.IsFinite(pitch))return;
            lookYaw=Mathf.Repeat(yaw,360); lookPitch=Mathf.Clamp(pitch,-89,89);
            acceptedInput=sequence; movement=Vector2.ClampMagnitude(move,1); sprint=run; jump|=leap;
            WorkHeld=workHeld;
            direction.rotation=Quaternion.Euler(0,yaw%360,0); lastInput=Time.unscaledTime; InputsAccepted++;
        }
        public bool RequestInteraction()
        {
            if(!IsOwner||!IsSpawned)return false;
            Interactor.FindNearby();
            var town=NetworkShop.Instance.Town;
            if(NetworkShop.Instance.Workday&&Interactor.Current is WorkdayStation workpoint){
                if(workpoint.action=="bolt"&&(!NetworkTestDriver.Automated||NetworkTestDriver.RealInput)){NetworkShop.Instance.Workday.BeginTorque(this,workpoint);return true;}
                SendCommand(NetworkShop.Id(Interactor.Current));if(hands&&hands.enabled)hands.Play(FirstPersonHands.Gesture.Reach,Interactor.Current.InteractionPoint);return true;}
            if(town&&town.HasBag(OwnerClientId)&&!(Interactor.Current is TownStation sorting&&sorting.kind==TownStation.Kind.Sort)){SendCommand("drop");return true;}
            // E at a shelf must stock it before the carrying fallback places the crate.
            // Use the same nearby target as the HUD; the server still validates the action.
            bool shelf=Interactor.Current is ShopStation station&&station.kind==ShopStation.Kind.Shelf;
            if(NetworkShop.Instance.Holder==OwnerClientId&&!shelf) { SendCommand("drop"); return true; }
            if(!Interactor.Current)return false;
            if(hands&&hands.enabled)hands.Play(FirstPersonHands.Gesture.Press,Interactor.Current.InteractionPoint);
            SendCommand(NetworkShop.Id(Interactor.Current)); return true;
        }
        public void SendCommand(string target)
        {
            if(!IsOwner||!IsSpawned)return;
            CommandsSent++;
            CommandServerRpc(target,++commandSequence,NetworkShop.Instance.Revision,NetworkShop.Instance.Shift.CurrentOrder?.id??"");
        }
        [ServerRpc]
        void CommandServerRpc(string target,uint sequence,int revision,string order)
        { NetworkShop.Instance.Execute(this,target,sequence,revision,order); }
        public int FeedbackReceived { get; private set; }
        public bool LastCommandAccepted {get;private set;}
        public void SendFeedback(string message,string reachId="",bool accepted=true)
        {
            if(!IsServer)return;
            FeedbackClientRpc(message,accepted,new ClientRpcParams {Send=new ClientRpcSendParams {TargetClientIds=new[]{OwnerClientId}}});
            if(!string.IsNullOrEmpty(reachId))RestockClientRpc(reachId);
        }
        [ClientRpc]
        public void FeedbackClientRpc(string message,bool accepted,ClientRpcParams delivery=default)
        {
            if(IsOwner){Interactor.Notify(message);LastFeedback=message;LastCommandAccepted=accepted;FeedbackReceived++;TownAudio.Play(TownAudio.Cue.Paper,null,.5f);}
        }
        [ClientRpc] public void WorkdayGestureClientRpc(string action,Vector3 point,ClientRpcParams delivery=default)
        {
            var authored=GetComponent<BusinessAsUsual.RescueV2.RescueHands>();if(IsOwner&&authored){authored.Begin(action,point);TownAudio.Play(action=="bolt"||action=="install"?TownAudio.Cue.Tool:TownAudio.Cue.Place,point,.6f);return;}
            if(!IsOwner||!hands||!hands.enabled)return;
            hands.Play(action=="bolt"||action=="install"?FirstPersonHands.Gesture.ToolTurn:action=="check"||action=="diagnose"||action=="inspect"||action=="verify"?FirstPersonHands.Gesture.Probe:action.StartsWith("sort-")?FirstPersonHands.Gesture.Sort:FirstPersonHands.Gesture.Reach,point);
            TownAudio.Play(action=="bolt"||action=="install"?TownAudio.Cue.Tool:TownAudio.Cue.Place,point,.6f);
        }
        [ClientRpc]
        public void RestockClientRpc(string reachId)
        {
            if(NetworkShop.Instance.Targets.TryGetValue(reachId,out var target)){GetComponent<ShopRestockReach>().Begin((ShopStation)target);
                if(IsOwner&&hands&&hands.enabled)hands.Play(FirstPersonHands.Gesture.Restock,target.InteractionPoint);TownAudio.Play(TownAudio.Cue.Place,target.InteractionPoint,.6f);}
        }
        public override void OnNetworkDespawn()
        {
            if(NetworkShop.Instance)NetworkShop.Instance.ReleaseBeforeDespawn(OwnerClientId);
            if(pause)pause.enabled=false; if(hands)hands.enabled=false; if(firstPerson)firstPerson.enabled=false;
            input.enabled=false; Interactor.enabled=false; GetComponent<ShopHUD>().enabled=false;
            var townHud=GetComponent<TownHandbook>();if(townHud)townHud.enabled=false;
            returnCrate?.Dispose(); returnCrate=null;
            if(direction)Destroy(direction.gameObject);
            base.OnNetworkDespawn();
        }
    }
}

