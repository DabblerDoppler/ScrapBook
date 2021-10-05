using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChildArray : MonoBehaviour {

    public List<GameObject> myChildren; 

    // Start is called before the first frame update
    void Start() {
        myChildren = new List<GameObject>();
        foreach(Transform child in transform) {
            myChildren.Add(child.gameObject);
        }
    }


}
