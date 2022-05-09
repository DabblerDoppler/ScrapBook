using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject : MonoBehaviour {

    public Transform associatedTransform;
    public float mapStart;
    public float mapSize;
    public float levelSize;
    public float leftTellyBound;
    public float rightTellyBound;



    private void Awake() {
        transform.SetParent(GameObject.Find("InGame").transform);
    }

    // Start is called before the first frame update
    void Start() {
        GameObject map = GameObject.Find("Map");
        transform.position = new Vector3(transform.position.x, map.transform.position.y, transform.position.z);

        mapStart = -(map.GetComponent<RectTransform>().rect.width / 2);

        mapSize = map.GetComponent<RectTransform>().rect.width;

        GameObject leftTelly = GameObject.Find("Teleporter_Left");
        GameObject rightTelly = GameObject.Find("Teleporter_Right");

        leftTellyBound = leftTelly.transform.position.x + (leftTelly.GetComponent<BoxCollider2D>().bounds.size.x / 2);
        rightTellyBound = rightTelly.transform.position.x - (rightTelly.GetComponent<BoxCollider2D>().bounds.size.x / 2);

        levelSize = rightTellyBound - leftTellyBound;



    }

    // Update is called once per frame
    void Update()  {
        if(associatedTransform != null) {
            float newXPosition = (mapStart + ((associatedTransform.position.x - leftTellyBound) / levelSize) * mapSize);
            Vector3 localPosition = GetComponent<RectTransform>().localPosition;
            GetComponent<RectTransform>().localPosition =  new Vector3(newXPosition  , localPosition.y , localPosition.z)  ;
        }
    }
}
