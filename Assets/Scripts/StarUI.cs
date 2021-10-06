using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StarUI : MonoBehaviour {
    public bool isYours;
    public Player attachedPlayer;
    public string myText;
    string myText_Final;


    private void Start() {
        myText_Final = "";
    }

    /*
    private void OnClientConnect() {
        if (isYours) {
            attachedPlayer = NetworkClient.localPlayer.gameObject.GetComponent<Player>();
            myText.text = "Your Stars: ";
        }
        else {
            myText.text = "";
        }
    }
    */

    public void SetPlayer(Player player) {
        attachedPlayer = player;
        myText_Final = myText;
    }


    // Update is called once per frame
    void Update() {
        if (attachedPlayer == null) {
            GetComponent<Text>().text = "";
            myText_Final = "";
        } else {
            GetComponent<Text>().text = myText_Final + attachedPlayer.stars.ToString();
        }
    }
}