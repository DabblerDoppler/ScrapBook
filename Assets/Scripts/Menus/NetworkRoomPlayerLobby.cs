using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.PlayerLoop;

public class NetworkRoomPlayerLobby : NetworkBehaviour {

    [Header("UI")]
    [SerializeField] public GameObject lobbyUI;
    [SerializeField] public Text[] playerNameTexts = new Text[8];
    [SerializeField] public Text[] playerReadyTexts = new Text[8];
    [SerializeField] private Button startGameButton = null;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading.";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    
    

    public bool isLeader;

    
    //figure this out when u got a chance, the problem lies with the room here
    private MyNetworkManager room;

    private MyNetworkManager Room {
        get {
            if(room != null) {return room;}
            return room = NetworkManager.singleton as MyNetworkManager;
        }
    }


    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        startGameButton = GameObject.Find("NetworkManager/MainMenuCanvas/LobbyMenu/StartButton").GetComponent<Button>();
    }

    public override void OnStartAuthority() {
        Debug.Log("authority started, isLeader = " + isLeader);
        CmdSetDisplayName(PlayerPrefs.GetString(PlayerNameInput.PLAYER_PREFS_NAME_KEY));
        lobbyUI.SetActive(true);
    }

    public override void OnStartClient() {
        Room.RoomPlayers.Add(this);
        UpdateDisplay();
    }

    public override void OnStopClient() {
        Room.RoomPlayers.Remove(this);
        CmdDestroySelf();
        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay() {
        Debug.Log("display updating...");
        if(!hasAuthority) {
            
            Debug.Log("no authority");
            
            foreach(var player in Room.RoomPlayers) {
                if(player.hasAuthority) {
                    player.UpdateDisplay();
                    break;
                }
                return;
            }
        }
        for (int i = 0; i < Room.RoomPlayers.Count; i++) {
                playerNameTexts[i].text = "Waiting For Player...";
                playerReadyTexts[i].text = string.Empty;
        }

        for(int i = 0; i< Room.RoomPlayers.Count; i++) {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            if(Room.RoomPlayers[i].isReady) {
                playerReadyTexts[i].text =  "Ready";
                playerReadyTexts[i].color = Color.green;
            } else {
                playerReadyTexts[i].text =  "Not Ready";
                playerReadyTexts[i].color = Color.red;
            }

        }
    }

    public void HandleReadyToStart(bool readyToStart) {
        if(isLeader) {
            startGameButton.interactable = readyToStart;
        }
    }

    [Command]
    private void CmdSetDisplayName(string name) {
        DisplayName = name;
    }

    [Command]
    public void CmdReadyUp() {
        isReady = !isReady;
        //Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdDestroySelf() {
        Destroy(gameObject);
        UpdateDisplay();
    }

    /*
    [ClientRpc]
    public void RpcDisableCanvas(GameObject myCanvas) {
        myCanvas.SetActive(false);
    }
    */


}
