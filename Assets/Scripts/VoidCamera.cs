using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidCamera : MonoBehaviour
{


    // Update is called once per frame
    void Update(){

        GetComponent<Camera>().depth = -10;
    }
}
