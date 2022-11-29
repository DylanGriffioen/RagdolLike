using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //All the public variables needed for moving, dashing and smooth turning
    public float moveSpeed = 7;
    public float dashSpeed = 20;
    public float dashDuration = 15;
    public float dashCooldown = 60;
    public float smoothTurnTime = 0.125f;
    public float rangedAttackCooldown = 0.5f;
    public float meleeAttackCooldown = 0.5f;
    //Ammo is used to keep track of how many bullets you have left
    public float ammo = 10;

    //Private floats timers to keep track of varius cooldowns and the dash duration
    private float dashTimer = 0;
    private float dashCooldownTimer = 0;
    private float rangedAttackCooldownTimer = 0;
    private float meleeAttackCooldownTimer = 0;
    
    /**
    Attacking works by spawning in either the meleePrefab for the melee attack
    or the bulletPrefab for the ranged attack
    */
    public GameObject bulletPrefab;
    public GameObject meleePrefab;
    /**
    This is used to keep track of an empty object that is attached to the front of the player character
    and is used as a spawnpoint for the melee and bullet prefab
    */
    public Transform spawnpoint;

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
    private void Start()
    {
        input = new InputActions();
        rb = GetComponent<Rigidbody>();
        variableSpeed = moveSpeed;
    }

    /**
        Every update the directional function is called to handle the movement of the player character
        After that the timers are checked and updated
    */
    private void Update()
    {
        Directional();
        updateDashTimers();
        UpdateAttackTimers();
    }

    // Update the timers needed for the dash
    private void updateDashTimers()
    {
        if(dashTimer > 0){
            dashTimer--;
            if(dashTimer <= 0)
            {
                variableSpeed = moveSpeed;
            }
        }
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer--;
        }
    }

    // Update the timers needed for the ranged and melee attacks
    private void UpdateAttackTimers()
    {
        if(meleeAttackCooldownTimer > 0)
        {
            meleeAttackCooldownTimer--;
        }
        if(rangedAttackCooldownTimer > 0)
        {
            rangedAttackCooldownTimer--;
        }
    }

    /**
        This function handles moving the player character in the desired direction while also making sure the player model turns smoothly
    */
    private void Directional()
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
    private void OnMove(InputValue movementValue)
    {
        inputDirection = movementValue.Get<Vector2>();
    }

    /**
    If the dashCooldownTimer is 0 this function makes the player character dash by setting the variableSpeed to dashSpeed
    Then it properly sets the timer for the dash so it ends in time and it's cooldown so you cannot spam the dash
    */
    private void OnDash(InputValue value)
    {
        if(dashCooldownTimer <= 0)
        {
            variableSpeed = dashSpeed;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }

    /**
    If ammo is above 0 spawn in a bulletPrefab at the spawnpoint position and give it the player characters rotation
    It also sets the rangeAttackCooldownTimer so that you cannot spam the attack too much
    */
    private void OnRangedAttack()
    {
        if(ammo > 0 && rangedAttackCooldownTimer <= 0)
        {
            Instantiate(bulletPrefab, spawnpoint.position, transform.rotation);
            ammo--;
            rangedAttackCooldownTimer = rangedAttackCooldown;
        }
    }

    /**
    Spawn in the meleePrefab at the spawnpoint posistion and give it the player charaters rotation
    It also sets the meleeAttackCooldownTimer so that you cannot spam the attack too much
    */
    private void OnMeleeAttack()
    {
        if(meleeAttackCooldownTimer <= 0)
        {
            Instantiate(meleePrefab, spawnpoint.position, transform.rotation);
            meleeAttackCooldownTimer = meleeAttackCooldown;
        }
    }

}