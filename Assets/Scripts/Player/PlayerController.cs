using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7;                     //  How fast the player walks around
    public float dashSpeed = 20;                    //  How fast the player walks during his dash
    public float dashDuration = 15;                 //  How long the dash lasts
    public float dashCooldown = 60;                 //  How much time has to pass between uses of the dash
    public float smoothTurnTime = 0.07f;            //  How fast the player character turns around
    public float rangedAttackCooldown = 20;         //  How much time has to pass between ranged attacks
    public float meleeAttackCooldown = 20;          //  How much time has to pass between melee attacks
    public float rangedAnimationTime = 10;          //  How long the player stands still during the ranged attack animation
    public float meleeAnimationTime = 10;           //  How long the player stands still during the melee attack animation
    public int currentBullets = 10;                 //  How many times the player can still use the ranged attack
    public int maximumBullets = 10;                 //  How much bullets the player is allowed to carry with him (important for refilling bullets)
    public int currentHealth = 10;                  //  How much health the player has
    public int maximumHealth = 10;                  //  How much health the player is allowed to have (important for healing)

    public int meleeAttackStartingDamage = 1;       //  How much damage the melee attack deals without powerups boosting it
    public int rangedAttackStartingDamage = 1;      //  How much damage the ranged attack deals without powerups boosting it

    //  Timers that keep track of the various durations and cooldowns the player has
    private float dashTimer = 0; 
    private float dashCooldownTimer = 0;
    private float rangedAttackCooldownTimer = 0;
    private float meleeAttackCooldownTimer = 0;
    private float rangedAnimationTimer = 0;
    private float meleeAnimationTimer = 0;
    
    public GameObject meleePrefab;                   //  The players melee attack
    public GameObject rangedPrefab;                  //  The players ranged attack
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
        meleePrefab.GetComponent<MeleeAttack>().SetDamage(meleeAttackStartingDamage);
        rangedPrefab.GetComponent<Bullet>().SetDamage(rangedAttackStartingDamage);
    }

    /**
        Every update the directional function is called to handle the movement of the player character
        After that the timers are checked and updated
    */
    private void Update()
    {
        if(meleeAnimationTimer <= 0 && rangedAnimationTimer <= 0)
        {
            Directional();
        }
        else
        {
            rb.velocity = new Vector3(0,0,0);
        }
        updateDashTimers();
        UpdateAttackTimers();
    }

    //  Update the timers needed for the dash
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

    //  Update the timers needed for the ranged and melee attacks
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
        if(rangedAnimationTimer > 0)
        {
            rangedAnimationTimer--;
        }
        if(meleeAnimationTimer > 0)
        {
            meleeAnimationTimer--;
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
    If bullets is above 0 spawn in a rangedPrefab at the spawnpoint position and give it the player characters rotation
    It also sets the rangeAttackCooldownTimer so that you cannot spam the attack too much
    */
    private void OnRangedAttack()
    {
        if(currentBullets > 0 && rangedAttackCooldownTimer <= 0)
        {
            Instantiate(rangedPrefab, spawnpoint.position, transform.rotation);
            currentBullets--;
            rangedAttackCooldownTimer = rangedAttackCooldown;
            rangedAnimationTimer = rangedAnimationTime;
        }
    }

    /**
    Spawn in the meleePrefab at the spawnpoint position and give it the player charaters rotation
    It also sets the meleeAttackCooldownTimer so that you cannot spam the attack too much
    */
    private void OnMeleeAttack()
    {
        if(meleeAttackCooldownTimer <= 0)
        {
            Instantiate(meleePrefab, spawnpoint.position, transform.rotation);
            meleeAttackCooldownTimer = meleeAttackCooldown;
            meleeAnimationTimer = meleeAnimationTime;
        }
    }

    /**
    Refills the players bullets
    Give this function a value below 0 if you want the bullets to be fully refilled (for game balance purposes)
    */ 
    public void RefillBullets(int bullets)
    {
        if(bullets < 0)
        {
            currentBullets = maximumBullets;
        }
        else if((currentBullets + bullets) < maximumBullets) // Cannot let the currentBullets exceed the maximumBullets value
        {
            currentBullets = currentBullets + bullets;
        }
        else
        {
            currentBullets = maximumBullets;
        }
    }

    /**
    Allows the player to take a variable amount of damage and keeps track of the players health
    */
    public void TakeDamage(int damage)
    {
        currentHealth = (currentHealth-damage);
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    /**
    Heals the player
    Give this function a value below 0 if you want the player to be fully healed (for game balance purposes)
    */ 
    public void HealPlayer(int healing)
    {
        if(healing < 0)
        {
            currentHealth = maximumHealth;
        }
        else if((currentHealth + healing) < maximumHealth) // Cannot let the currentHealth exceed the maximumHealth value
        {
            currentHealth += healing;
        }
        else
        {
            currentHealth = maximumHealth;
        }
    }

    /**
    Increases the players maximum health
    */
    public void IncreaseMaxHealth(int healthIncrease)
    {
        maximumHealth += healthIncrease;
    }

    /**
    Increases the players maximum health
    */
    public void IncreaseMeleeDamage(int damageIncrease)
    {
        meleePrefab.GetComponent<MeleeAttack>().IncreaseDamage(damageIncrease);
    }

    /**
    Increases the players maximum health
    */
    public void IncreaseRangedDamage(int damageIncrease)
    {
        rangedPrefab.GetComponent<Bullet>().IncreaseDamage(damageIncrease);
    }
}