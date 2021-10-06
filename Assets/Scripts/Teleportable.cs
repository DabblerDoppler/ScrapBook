using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Teleportable : MonoBehaviour {


    public int horizontalRayCount = 2;
    float horizontalRaySpacing;
    public LayerMask teleporterCollisionMask;
    const float SKIN_WIDTH = 0.0125f;
    RaycastOrigins raycastOrigins;
    public Collider2D collider;

    private void Awake() {
        collider = GetComponent<Collider2D>();   
    }

    private void Start() {
        CalculateRaySpacing();
        teleporterCollisionMask = LayerMask.GetMask("Teleporter");
    }

    public void CheckTeleporters(Vector3 velocity) {
        UpdateRaycastOrigins();
        if (velocity.x != 0) {
            HorizontalCollisions_Teleporters(ref velocity);
        }
    }

    void HorizontalCollisions_Teleporters(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, teleporterCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                //velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                //rayLength = hit.distance;
                TeleportToOtherTeleporter(hit);
            }
        }
    }

    void TeleportToOtherTeleporter(RaycastHit2D hit) {
        GameObject teleportTo;
        float sizeX ;

        if(collider.GetType() == typeof(BoxCollider2D)) {
            sizeX = GetComponent<BoxCollider2D>().size.x;
            //this is only for circle colliders but i need it to compile
            // -> else if (collider.GetType() == typeof(CircleCollider2D)) i'm sorry little one
            // this will error if collider is not a boxcollider or circle collider theoretically
        } else {
            sizeX = 2 * GetComponent<CircleCollider2D>().radius;
        }

        if (hit.transform.name == "Teleporter_Left") {
            teleportTo = GameObject.Find("Teleporter_Right");
            //GetComponent<Rigidbody2D>().simulated = false;
            //StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.min.x - sizeX / 2, transform.position.y, transform.position.z);
        }
        else {
            teleportTo = GameObject.Find("Teleporter_Left");
            //GetComponent<Rigidbody2D>().simulated = false;
            //StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.max.x + sizeX / 2, transform.position.y, transform.position.z);
        }
    }



    public void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing() {
        Bounds bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
    }

    public struct CollisionInfo {
        public bool above, below;
        public bool left, right;

        public void Reset() {
            above = below = left = right = false;
        }

    }


    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

}
