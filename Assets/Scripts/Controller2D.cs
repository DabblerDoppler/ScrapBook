using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : NetworkBehaviour {

    bool incrementStars;
    public BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public LayerMask collisionMask;
    public LayerMask playerCollisionMask;
    public LayerMask starCollisionMask;
    public CollisionInfo collisions;
    public CollisionInfo playerCollisions;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    const float SKIN_WIDTH = 0.0125f;

    private void Awake() {
        collider = GetComponent<BoxCollider2D>();
    }

    public virtual void Start() {
        CalculateRaySpacing();
        playerCollisionMask = LayerMask.GetMask("Players");
        collisionMask = LayerMask.GetMask("Floor");
        starCollisionMask = LayerMask.GetMask("Star");
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        incrementStars = false;
        collisions.Reset();
        playerCollisions.Reset();
        if(velocity.x != 0) {
            HorizontalCollisions(ref velocity);
            HorizontalCollisions_Players(ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
            VerticalCollisions_Players(ref velocity);
        }

        VerticalCollisions_Stars(ref velocity);
        HorizontalCollisions_Stars(ref velocity);

        if(incrementStars) {
            GetComponentInParent<Player>().stars += 1;
        }

        transform.position += velocity;

    }


    void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if(hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    public bool VerticalCollisions_uncrouch() {
        float rayLength = 0.5f;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i );
            RaycastHit2D foundHit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);
            if (foundHit) {
                return true;
            }
        }
        return false;
    }

    void VerticalCollisions_Stars(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, starCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                hit.collider.GetComponentInParent<Star>().playerTouch();
                incrementStars = true;
            }
        }
    }


    void VerticalCollisions_Players(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, playerCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit && hit.collider != collider) {
                velocity.y += (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                playerCollisions.below = directionY == -1;
                playerCollisions.above = directionY == 1;
            }
        }

    }



    void HorizontalCollisions_Stars(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, starCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                hit.collider.GetComponentInParent<Star>().playerTouch();
                incrementStars = true;
            }
        }
    }

    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }

    void HorizontalCollisions_Players(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, playerCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit && hit.collider != collider) {
                velocity.x += (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                playerCollisions.left = directionX == -1;
                playerCollisions.right = directionX == 1;
            }
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
        verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
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
