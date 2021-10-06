using System.Collections;
using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : NetworkBehaviour {

    bool incrementStars;
    bool killPlayer;
    Player knockdownPlayer;
    RaycastHit2D lastStarHit;
    public BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public LayerMask collisionMask;
    public LayerMask playerCollisionMask;
    public LayerMask starCollisionMask;
    public LayerMask spikeCollisionMask;
    public LayerMask teleporterCollisionMask;
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
        starCollisionMask = LayerMask.GetMask("Star", "FallenStar");

        spikeCollisionMask = LayerMask.GetMask("Spike");
        teleporterCollisionMask = LayerMask.GetMask("Teleporter");
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        incrementStars = false;
        killPlayer = false;
        knockdownPlayer = null;
        collisions.Reset();
        playerCollisions.Reset();
        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
            //HorizontalCollisions_Players(ref velocity);
            HorizontalCollisions_Spikes(ref velocity);
            HorizontalCollisions_Teleporters(ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
            VerticalCollisions_Players(ref velocity);
            VerticalCollisions_Spikes(ref velocity);
            HorizontalCollisions_Teleporters(ref velocity);
        }

        VerticalCollisions_Stars(ref velocity);
        HorizontalCollisions_Stars(ref velocity);

        if (knockdownPlayer != null) {
            if (isServer) {
                Debug.Log("Running RpcKnockdown.");
                knockdownPlayer.RpcKnockdown();
            }
            else {
                Debug.Log("Running CmdKnockdown.");
                CmdKnockdownPlayer(knockdownPlayer);
            }
        }
        
        if (incrementStars && GetComponentInParent<Player>().intangibility < 0) {
            GetComponentInParent<Player>().CmdAddStars(1);
            if (lastStarHit.collider.GetComponent<Star>() != null) {
                lastStarHit.collider.GetComponentInParent<Star>().playerTouch();
            } else {
                lastStarHit.collider.GetComponentInParent<DroppedStar>().playerTouch();
            }
        }
        if (killPlayer) {
            GetComponent<Player>().Death();
        }


        //GetComponent<Rigidbody2D>().velocity = velocity / Time.deltaTime;
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
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    void VerticalCollisions_Players(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength * 2, playerCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit && hit.transform.position != transform.position) {
                if (hit.collider.GetComponent<Player>().intangibility <= 0 && collider.GetComponent<Player>().intangibility <= 0) {
                    playerCollisions.below = directionY == -1;
                    playerCollisions.above = directionY == 1;
                    if (playerCollisions.below && transform.position.y > hit.transform.position.y) {
                        int direction = -1;
                        if (transform.position.x > hit.transform.position.x) {
                            direction = 1;
                        }
                        //knockdown
                        knockdownPlayer = hit.collider.GetComponent<Player>();
                    }
                }
            }
        }

    }


    void VerticalCollisions_Teleporters(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, teleporterCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                TeleportToOtherTeleporter(hit);
            }
        }
    }

    void TeleportToOtherTeleporter(RaycastHit2D hit) {
        GameObject teleportTo;
        if (hit.transform.name == "Teleporter_Left") {
            teleportTo = GameObject.Find("Teleporter_Right");
            GetComponent<Rigidbody2D>().simulated = false;
            StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.min.x - collider.size.x / 2, transform.position.y, transform.position.z);
            /*
            if (isServer) {
                RpcUnsimulate();
                RpcResimulate();
            }
            else {
                CmdUnsimulate();
                CmdResimulate();
            }
            */
        } else {
            teleportTo = GameObject.Find("Teleporter_Left");
            GetComponent<Rigidbody2D>().simulated = false;
            StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.max.x + collider.size.x / 2, transform.position.y, transform.position.z);
            /*
            if (isServer) {
                RpcUnsimulate();
                RpcResimulate();
            } else {
                CmdUnsimulate();
                CmdResimulate();
            }
            */
        }

        //bring the void camera forward for 1 frame and stop collisions and player rendering for 1 frame
        StartCoroutine(FreezeCam());

    }


    [Command(requiresAuthority = false)]
    void CmdResimulate() { RpcResimulate(); }

    [Command(requiresAuthority = false)]
    void CmdUnsimulate() { RpcUnsimulate(); }

    [ClientRpc]
    void RpcResimulate() { StartCoroutine(Resimulate()); }

    [ClientRpc]
    void RpcUnsimulate() {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().forceRenderingOff = true;
    }




    //might need to be space out for client
    IEnumerator Resimulate() {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Rigidbody2D>().simulated = true;
        //GetComponent<SpriteRenderer>().forceRenderingOff = false;
    }

    //might need to be space out for client
    IEnumerator FreezeCam() {
        Camera.main.clearFlags = CameraClearFlags.Nothing;
        Camera.main.cullingMask = 0;
        yield return null;
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.cullingMask = -1;
    }


    void VerticalCollisions_Spikes(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, spikeCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                killPlayer = true;
                
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
                lastStarHit = hit;
                incrementStars = true;
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
                lastStarHit = hit;
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
            if (hit && hit.transform.position != transform.position) {
                velocity.x += (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                playerCollisions.left = directionX == -1;
                playerCollisions.right = directionX == 1;
            }
        }
    }

    void HorizontalCollisions_Spikes(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, spikeCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
                GetComponent<Player>().Death();

            }
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


    [Command(requiresAuthority = false)]
    public void CmdKnockdownPlayer(Player player) {
        Debug.Log("CmdKnockdownPlayer run. Trying to call RpcKnockdown.");
        player.RpcKnockdown();
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
