using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    //All the public variables needed for moving, dashing and smooth turning
    public float moveSpeed = 7;
    public float dashSpeed = 20;
    public float dashDuration = 15;
    public float dashCooldown = 60;
    public float smoothTurnTime = 0.125f;

    //Private floats needed for the dash duration and the cooldown
    private float dashTimer = 0;
    private float dashCooldownTimer = 0;

    private Rigidbody rb;
    private InputActions input;
    //Vectors for finding out what direction the player wants the character to move in
    private Vector2 inputDirection, moveDirection;
    //Floats needed for the smooth turning
    private float targetAngle, smoothAngle, smoothTurnVelocity;
    //This float is used as the actual speed float and can be changed to make the player dash
    private float variableSpeed;
    
    /**
        Gets the inputactions and rigidbody while setting the characters speed to moveSpeed so he can walk around at normal speed
    */
    void Start()
    {
        input = new InputActions();
        rb = GetComponent<Rigidbody>();
        variableSpeed = moveSpeed;
    }

    /**
        Every update the directional function is called to handle the movement of the player character
        After that the timers are checked and updated
    */
    void Update()
    {
        //Todo change into switch case
        Directional();
        if(dashTimer <= 0){
            variableSpeed = moveSpeed;
        } else {
            dashTimer--;
        }
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer--;
        }
    }

    /**
        This function handles moving the player character in the desired direction while also making sure the player model turns smoothly
    */
    void Directional()
    {
        moveDirection = inputDirection * variableSpeed; //inputDirection is the normalized direction, moveDirection includes moveSpeed
        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.y); //Assign velocity and keep current y-component of velocity

        //Smooth rotation
        if (inputDirection.magnitude > 0.125f)
        {
            targetAngle = (Vector2.SignedAngle(inputDirection, Vector2.up) + 360f) % 360f;
            smoothAngle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetAngle, ref smoothTurnVelocity, smoothTurnTime);
            transform.rotation = Quaternion.Euler(new Vector3(0, smoothAngle, 0));
        }
    }

    //This function keeps check of what direction the player wants to move in
    void OnMove(InputValue movementValue)
    {
        inputDirection = movementValue.Get<Vector2>();
    }

    /**
    If the dashCooldownTimer is 0 this function makes the player character dash by setting the variableSpeed to dashSpeed
    Then it properly sets the timer for the dash so it ends in time and it's cooldown so you cannot spam the dash
    */
    void OnDash(InputValue value)
    {
        if(dashCooldownTimer <= 0)
        {
            variableSpeed = dashSpeed;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }
}
