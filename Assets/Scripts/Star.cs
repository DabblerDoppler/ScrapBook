using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour {

    BoxCollider2D collider;
    ContactFilter2D contactFilter;

    Collider2D[] results;

    // Start is called before the first frame update
    void OnEnable() {
        results = new Collider2D[10];
        collider = GetComponent<BoxCollider2D>();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = 3;
    }

    // Update is called once per frame
    void Update() {
    }

    public void playerTouch() {
        gameObject.SetActive(false);
        GameObject.Find("StarManager").GetComponent<StarManager>().currentStar = null;
    }

}
