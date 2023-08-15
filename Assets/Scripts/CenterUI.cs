using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterUI : MonoBehaviour {

    const int I_MAX = 4;
    string[] countdownText = new string[I_MAX];


    // Start is called before the first frame update
    void Start()
    {
        countdownText[0] = "Ready!";
        countdownText[1] = "Set!";
        countdownText[2] = "SCRAP!";
        countdownText[3] = "";

        
        GetComponent<Text>().text = countdownText[0];

        StartCoroutine(Countdown(1));
    }

    IEnumerator Countdown(int i) {
        yield return new WaitForSeconds(1.0f);
        GetComponent<Text>().text = countdownText[i];
        if(i+1 < I_MAX){
            StartCoroutine(Countdown(i+1));
        }
    }

}
