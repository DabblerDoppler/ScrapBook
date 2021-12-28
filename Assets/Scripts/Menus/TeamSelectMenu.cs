using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



public class TeamSelectMenu : MonoBehaviour {
    
    const int GREEN = 0;
    const int RED = 1;
    const int YELLOW = 2;
    const int BLUE = 3;

    private void Start() {
        gameObject.SetActive(false);
    }

    public void SelectRed() {
        NetworkClient.localPlayer.GetComponent<Player>().SetTeam(RED);
    }

    public void SelectBlue() {
        NetworkClient.localPlayer.GetComponent<Player>().SetTeam(BLUE);
    }
    public void SelectYellow() {
        NetworkClient.localPlayer.GetComponent<Player>().SetTeam(YELLOW);
    }

    public void SelectGreen() {
        NetworkClient.localPlayer.GetComponent<Player>().SetTeam(GREEN);
    }

}
