using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed                  = 7;        // How fast the player walks around
    public float dashSpeed                  = 25;       // How fast the player walks during his dash
    public float smoothTurnTime             = 0.07f;    // How fast the player character turns around
    public int currentMagic                 = 10;       // How many times the player can still use the ranged attack
    public int maximumMagic                 = 10;       // How much magic the player is allowed to carry with him (important for refilling magic)
    public int currentHealth                = 10;       // How much health the player has
    public int maximumHealth                = 10;       // How much health the player is allowed to have (important for healing)
    public int lives                        = 3;

    public int meleeAttackStartingDamage    = 1;        // How much damage the melee attack deals without powerups boosting it
    public int rangedAttackStartingDamage   = 1;        // How much damage the ranged attack deals without powerups boosting it

    public float dashCD                     = 1.2f;     // How long the player has to wait in between dashes 
    public float dashCDCurrent              = 0.0f;     // Timer for tracking the cooldwon between dashes
    public float dashDuration               = 2f;       // How long the dash lasts
    public float dashDurationCurrent        = 0.0f;     // Timer for tracking how long the dash lasts
    public bool dashReady;                              // Boolean used to let the script now the player is allowed to dash

    public float rangedAnimationD           = 2f;       // How long the ranged attack animation plays
    public float rangedAnimationDC          = 0.0f;     // Timer for the ranged attack animation
    public float rangedAttackD              = 1f;       // Delay for when to spawn in the ranged prefab
    public float rangedAttackDC             = 0.0f;     // Timer for the ranged attack prefab delay
    public bool hasAttackedRanged           = false;    // Bool used to makes sure only one attack is triggered

    public float meleeAnimationD            = 2f;       // How long the melee attack animation plays
    public float meleeAnimationDC           = 0.0f;     // Timer for the melee attack animation
    public float meleeAttackD               = 1f;       // Delay for when to spawn in the ranged prefab
    public float meleeAttackDC              = 0.0f;     // Timer for the ranged attack prefab delay
    public bool hasAttackedMelee            = false;    // Bool used to makes sure only one attack is triggered

    private bool attackRanged               = false;    // Used to switch the script state from moving to attacking
    private bool attackMelee                = false;    // Used to switch the script state from moving to attacking
    private bool dashing                    = false;    // Keeps track of wether or not the player is dashing so that the animations work properly

    private bool triggerMeleeAttackSet      = false;    // Keeps track of if the attack trigger has already been set for that attack
    private bool triggerRangedAttackSet     = false;    // Keeps track of if the attack trigger has already been set for that attack

    Transform model;                                    // Needed to get acces to the animations attatched to the modeö
    Animator anim;                                      // Needed to set the animations for the model

    public GameObject meleePrefab;                      // The players melee attack
    public GameObject rangedPrefab;                     // The players ranged attack
    public Transform spawnpoint;                        // Empty object attatched in front of the player character used to spawn the attacks on that location

    private Rigidbody rb;
    private InputActions input;
    // Vectors for finding out what direction the player wants the character to move in
    private Vector2 inputDirection, moveDirection;
    // Floats needed for the smooth turning
    private float targetAngle, smoothAngle, smoothTurnVelocity;
    // This float is used as the actual speed float and can be changed to make the player dash
    private float variableSpeed;

    // Variables needed for the ui
    public GameObject GameOverUI;
    public GameObject MainGUI;
    public Text livesText;
    public Text healthText;
    public Text magicText;

    /**
        Sets all the variables to their intended values
    */
    private void Start()
    {
        input = new InputActions();
        rb = GetComponent<Rigidbody>();
        variableSpeed = moveSpeed; // making sure the player starts with the right movespeed
        meleePrefab.GetComponent<MeleeAttack>().SetDamage(meleeAttackStartingDamage); // Just making sure the attack starts with the right damage value
        rangedPrefab.GetComponent<Bullet>().SetDamage(rangedAttackStartingDamage);  // Just making sure the attack starts with the right damage value
 
        model = transform.GetChild(0);
        anim = model.GetComponent<Animator>();

        GameOverUI.SetActive(false);
        MainGUI.SetActive(true);
        magicText.text = "Magic: " + currentMagic.ToString();
        livesText.text = "Lives: " + lives.ToString();
        healthText.text = "Health: " + currentHealth.ToString();
    }

    /**
        Calls the movement function if not attacking otherwise it calls the correct attack function
        At the end it updates the dash timers
    */
    private void Update()
    {
        if(!attackRanged && !attackMelee)
        {
            Directional();
        }
        else
        {
            if(attackMelee)
            {
                AttackMelee();
            }
            else
            {
                AttackRanged();
            }
            rb.velocity = new Vector3(0,0,0);
        }
        updateDashTimers();
    }

    //  Update the timers needed for the dash
    private void updateDashTimers()
    {
        if(dashDurationCurrent >= dashDuration)
        {
            variableSpeed = moveSpeed;
            dashing = false;
        }
        else
        {
            dashDurationCurrent += Time.deltaTime;
        }
        if(dashCDCurrent >= dashCD)
        {
            dashReady = true;
        }
        else
        {
            dashReady = false;
            dashCDCurrent += Time.deltaTime;
        }
    }

    // This function handles moving the player character in the desired direction while also making sure the player model turns smoothly
    private void Directional()
    {
        if(!attackRanged && !attackMelee && !dashing)
        {
            // Setting the correct idle and walk animations if not attacking or dashing
            if(rb.velocity.x <= 1 && rb.velocity.y <= 1)
            {
                anim.SetBool("Run", false);
                anim.SetBool("Idle", true);
                anim.SetBool("Dash", false);
            }
            else
            {
                anim.SetBool("Idle", false);
                anim.SetBool("Run", true);
                anim.SetBool("Dash", false);
            }
        }

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
    If dashReady is true it makes the player dash by setting it's speed to the dashspeed
    It sets the timers to 0 so those work properly
    dashing is set to true to let the other functions know the player is dashing
    And the correct animation is played
    */
    private void OnDash(InputValue value)
    {
        if(dashReady && !attackRanged && !attackMelee)
        {
            dashCDCurrent = 0.0f;
            dashDurationCurrent = 0.0f;
            anim.SetBool("Idle", false);
            anim.SetBool("Run", false);
            anim.SetBool("Dash", true);
            dashing = true;
            variableSpeed = dashSpeed;
        }
    }

    /**
    If the player has any magic left the attackranged bool is set to true so that the player can use the attack ranged function
    */
    private void OnRangedAttack()
    {
        if(currentMagic > 0)
        {
            attackRanged = true;
        }
    }

    /**
    Function that plays the attack ranged animation and summons the ranged prefab
    Timers are used to make the animation and the summoning of the prefab line up nicely
    */
    private void AttackRanged()
    {
        if(!triggerRangedAttackSet)
        {
            anim.SetBool("Idle", false);
            anim.SetBool("Run", false);
            anim.SetBool("Dash", false);
            anim.SetTrigger("Ability");
            triggerRangedAttackSet = true;
        }
        if(rangedAttackDC >= rangedAttackD && !hasAttackedRanged)
        {
            Instantiate(rangedPrefab, spawnpoint.position, transform.rotation);
            currentMagic--;
            magicText.text = "Magic: " + currentMagic.ToString();
            hasAttackedRanged = true;
        }
        else
        {
            rangedAttackDC += Time.deltaTime;
        }
        if(rangedAnimationDC >= rangedAnimationD)
        {
            rangedAnimationDC       = 0.0f;
            rangedAttackDC          = 0.0f;
            triggerRangedAttackSet  = false;
            attackRanged            = false;
            hasAttackedRanged       = false;
        }
        else
        {
            rangedAnimationDC += Time.deltaTime;
        }
    }

    // sets attackmelee to true so that the player can use the attack melee function
    private void OnMeleeAttack()
    {
        attackMelee = true;
    }

    /**
    Function that plays the melee animation and summons the melee prefab
    Timers are used to make the animation and the summoning of the prefab line up nicely
    */
    private void AttackMelee()
    {
        if(!triggerMeleeAttackSet)
        {
            anim.SetBool("Idle", false);
            anim.SetBool("Run", false);
            anim.SetBool("Dash", false);
            anim.SetTrigger("Attack");
            triggerMeleeAttackSet = true;
        }
        if(meleeAttackDC >= meleeAttackD && !hasAttackedMelee)
        {
            Instantiate(meleePrefab, spawnpoint.position, transform.rotation);
            hasAttackedMelee = true;
        }
        else
        {
            meleeAttackDC += Time.deltaTime;
        }
        if(meleeAnimationDC >= meleeAnimationD)
        {
            meleeAnimationDC        = 0.0f;
            meleeAttackDC           = 0.0f;
            triggerMeleeAttackSet  = false;
            attackMelee            = false;
            hasAttackedMelee       = false;
        }
        else
        {
            meleeAnimationDC += Time.deltaTime;
        }
    }

    /**
    Refills the players magic
    Give this function a value below 0 if you want the magic to be fully refilled (for game balance purposes)
    */ 
    public void RefillMagic(int magic)
    {
        if(magic < 0)
        {
            currentMagic = maximumMagic;
        }
        else if((currentMagic + magic) < maximumMagic) // Cannot let the currentMagic exceed the maximumMagic value
        {
            currentMagic = currentMagic + magic;
        }
        else
        {
            currentMagic = maximumMagic;
        }
        magicText.text = "Magic: " + currentMagic.ToString();
    }

    /**
    Allows the player to take a variable amount of damage and keeps track of the players health
    */
    public void TakeDamage(int damage)
    {
        currentHealth = (currentHealth-damage);
        healthText.text = "Health: " + currentHealth.ToString();
        if(currentHealth <= 0)
        {
            if(lives > 0)
            {
                lives--;
                currentHealth = maximumHealth;
                livesText.text = "Lives: " + lives.ToString();
                healthText.text = "Health: " + currentHealth.ToString();
            }
            else
            {
                GameOverUI.SetActive(true);
                MainGUI.SetActive(false);
                Time.timeScale = 0f;
            }
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
        healthText.text = "Health: " + currentHealth.ToString();
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