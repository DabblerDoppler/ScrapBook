using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : NetworkBehaviour {

    //input
    float horizontalInput;
    float verticalInput;
    bool jumpHeld;
    bool jumpPressed;

    //unity variables
    Collider2D collider;
    Controller2D controller;

    //physics
    float hsp;
    float vsp;

    int walljump_lock;

    const float HSP_FRIC_GROUND = 0.00008f * 500  ;
    const float HSP_FRIC_AIR = 0.00002f * 500  ;
    const float HSP_FRIC_WALLJUMP = 0.000005f * 500  ;
    const float HSP_MAX = 0.010f  ;
    const float VSP_MAX = 0.018f  ;
    const float VSP_MAX_WALL = 0.006f;
    const float GRAVITY = 0.000075f * 500 ;
    const float GRAVITY_HELD = 0.00005f * 500 ;
    const float JUMP_HEIGHT = 0.015f  ;
    const float WALL_JUMP_HEIGHT = 0.01f  ;
    const float WALL_JUMP_LENGTH = 0.09f  ;
    const float DEADZONE = 0.2f;
    const float WALK_SPEED = 0.0006f  * 500;
    const float AIR_SPEED = 0.00015f  * 500;



    const int LAYER_FLOOR = 6;
    const int LAYER_WALL = 7;

    private void Start() {
        collider = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();
        walljump_lock = 0;


    }

    private void Update() {
        HandleMovement();
    }

    void HandleMovement() {
        if(isLocalPlayer) {
            HandleInput();
            bool onGround = controller.collisions.below;
            int onWall = (controller.collisions.left) ? -1 : 0;
            onWall += (controller.collisions.right) ? 1 : 0;
            if(Math.Sign(onWall) != Math.Sign(horizontalInput) || onGround) {
                onWall = 0;
            }


            
            if(onWall != 0 || onGround) {
                walljump_lock = 0;
            }
            if (walljump_lock != 0) {
                if (vsp > 0) {
                    if (Math.Sign(horizontalInput) == Math.Sign(walljump_lock)) {
                        horizontalInput *= 0.2f;
                    }
                    else {
                        horizontalInput *= 0.05f;
                    }
                } else {
                    if (Math.Sign(horizontalInput) == Math.Sign(walljump_lock)) {
                        horizontalInput *= 0.4f;
                    }
                    else {
                        horizontalInput *= 0.2f;
                    }
                }
            }

            
            if(controller.collisions.above || controller.collisions.below) { vsp = 0; }
            if(controller.collisions.left || controller.collisions.right) { hsp = 0; }

            float moveSpeed = AIR_SPEED;
            if (onGround) { moveSpeed = WALK_SPEED; }
            hsp += moveSpeed * horizontalInput * Time.deltaTime;

           if(jumpPressed && onGround) {
                vsp = JUMP_HEIGHT;
           }

           if(jumpPressed && onWall != 0 && Math.Sign(horizontalInput) == Math.Sign(onWall) ) {
                vsp = WALL_JUMP_HEIGHT;
                hsp = WALL_JUMP_LENGTH * -onWall;
                walljump_lock = -onWall;
            }

            //apply gravity
            float gravity = GRAVITY;
            if(jumpHeld && vsp > 0) {gravity = GRAVITY_HELD;}
            vsp -= gravity * Time.deltaTime;

            float vspMaxFinal = VSP_MAX;
            if (onWall != 0 && Math.Sign(horizontalInput) == Math.Sign(onWall) && vsp <= 0) { vspMaxFinal = VSP_MAX_WALL; }

            //apply friction
            float friction = HSP_FRIC_AIR;
            if(onGround) {friction = HSP_FRIC_GROUND;}
            if(walljump_lock != 0) { friction = HSP_FRIC_WALLJUMP; }

            hsp = Approach(hsp, 0, friction);

            //clamp to 
            hsp = Clamp(hsp, -HSP_MAX, HSP_MAX);
            vsp = Clamp(vsp, -vspMaxFinal, vspMaxFinal);

            controller.Move(new Vector3(hsp, vsp) * Time.deltaTime * 600);
        }
    }

    void HandleInput() {
        horizontalInput = Input.GetAxis("Horizontal");
        //deadzone
        if(Math.Abs(horizontalInput) < DEADZONE) {
            horizontalInput = 0;
        }
        verticalInput = Input.GetAxis("Vertical");
        //deadzone
        if(Math.Abs(verticalInput) < DEADZONE) {
            verticalInput = 0;
        }
        jumpHeld = Input.GetButton("Jump");
        jumpPressed = Input.GetButtonDown("Jump");
    }


    float Approach(float argument0, float argument1, float argument2) {
        if (argument0 < argument1) {
            argument0 += argument2 * Time.deltaTime;
            if (argument0 > argument1) { return argument1; }
        }
        else {
            argument0 -= argument2 * Time.deltaTime;
            if (argument0 < argument1) { return argument1; }
        }
        return argument0;
    }

    float Clamp(float value, float min, float max) {
        if(value > max) {
            value = max;
        }
        if(value < min) {
            value = min;
        }
        return value;
    }




}
