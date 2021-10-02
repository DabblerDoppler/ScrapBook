using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportManager : MonoBehaviour {

    public GameObject leftCamera;
    public GameObject rightCamera;
    public GameObject mainCamera;



    public Camera mainCamera_Camera;

    //teleporter goin up
    public GameObject leftTeleporter;
    public GameObject rightTeleporter;

    float cameraWidth;
    float focusAreaWidth;


    // Start is called before the first frame update
    void Start() {
        mainCamera_Camera = mainCamera.GetComponent<Camera>();
        //rect is from 0 to 1

        Vector3 tempRight = mainCamera_Camera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 tempLeft =  mainCamera_Camera.ViewportToWorldPoint(new Vector3(0, 0, 0));

        cameraWidth = tempRight.x - tempLeft.x;

        focusAreaWidth = mainCamera.GetComponent<CameraFollow>().focusAreaSize.x;

    }

    // Update is called once per frame
    void Update() {

        //Camera.main.rect = new Rect(0, 0, 0.5f, 1);
        //leftCamera.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);


        //below doesn't work, use this https://catlikecoding.com/unity/tutorials/custom-srp/multiple-cameras/ maybe
        // another resource, mentions Masked RenderTextures https://forum.unity.com/threads/render-only-portion-of-camera.765743/
        // might be the way to go https://docs.unity3d.com/ScriptReference/RenderTexture.html
        
        /*
        if(mainCamera.transform.position.x < leftTeleporter.GetComponent<BoxCollider2D>().bounds.max.x + cameraWidth / 2) {

        } else if (mainCamera.transform.position.x > rightTeleporter.GetComponent<BoxCollider2D>().bounds.min.x + cameraWidth / 2) {

        } else {
            mainCamRect.xMin = 0.0f;
            mainCamRect.xMax = 1.0f;
            leftCamRect.xMin = 1.0f;
            rightCamRect.xMax = 0.0f;

        }
        */
    }
}
