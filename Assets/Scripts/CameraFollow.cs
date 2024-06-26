using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Controller2D target;
    public bool hasTarget;
    public Vector2 focusAreaSize;
    public FocusArea focusArea;
    public float moveStagesRight;

    public float lookAheadDistX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;
    public float verticalOffset;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirectionX;
    float smoothLookVelocityX;
    float smoothVelocityY;

    public float levelDistance;

    const float LOWEST_Y = 7.25f;
    const float HIGHEST_Y = -1.0f;

    private void Start() {
        hasTarget = false;
        //0.71
        levelDistance = (54.99879f - 0.71f);
    }

    private void LateUpdate() {
        hasTarget = (target != null);
        if (!hasTarget) {
            if (!(GameObject.FindGameObjectsWithTag("Player").Length == 0)) {
                target = NetworkClient.localPlayer.gameObject.GetComponent<Controller2D>();
                focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
                hasTarget = true;
            }
        } else {
            //focusArea.Update(target.collider.bounds);
            //Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;
            Vector2 focusPosition = (Vector2)target.transform.position + Vector2.up * verticalOffset;
            transform.position = (Vector3)focusPosition + Vector3.forward * -10 + Vector3.right * moveStagesRight * (levelDistance);
        }


        if(transform.root != transform) {
            transform.position = new Vector3(transform.position.x, transform.parent.position.y, transform.position.z);
        }

        if (transform.position.y >= LOWEST_Y) {
            transform.position = new Vector3(transform.position.x, LOWEST_Y, transform.position.z);
        }

        if (transform.position.y <= HIGHEST_Y) {
            transform.position = new Vector3(transform.position.x, HIGHEST_Y, transform.position.z);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }



    public struct FocusArea {
        public Vector2 center;
        public Vector2 velocity;
        float left, right;
        float top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size) {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.center.y - size.y / 2;
            top = targetBounds.center.y + size.y / 2;

            velocity = Vector2.zero;
            center = new Vector2( ((left + right) / 2), ((top + bottom) / 2));
        }

        public void Update(Bounds targetBounds) {


            float shiftX = 0;
            if(targetBounds.min.x < left) {
                shiftX = targetBounds.min.x - left;
            } else if (targetBounds.max.x > right) { 
                    shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom) {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top) {
                shiftY = targetBounds.max.y - top;
            }
            bottom += shiftY;
            top += shiftY;

            center = new Vector2((left + right) / 2, (top + bottom) / 2);

            velocity = new Vector2(shiftX, shiftY);
        }
    }



}
