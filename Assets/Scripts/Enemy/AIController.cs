using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;                       // Nav mesh agent component
    public float startWaitTime                  = 4;        // Wait time of every action
    public float timeToRotate                   = 2;        // Wait time when the enemy detect near the player without seeing
    public float speedWalk                      = 6;        // Walking speed, speed in the nav mesh agent
    public float speedRun                       = 9;        // Running speed
 
    public float viewRadius                     = 15;       // Radius of the enemy view
    public float viewAngle                      = 90;       // Angle of the enemy view
    public LayerMask playerMask;                            // To detect the player with the raycast
    public LayerMask obstacleMask;                          // To detect the obstacules with the raycast
    public float meshResolution                 = 1.0f;     // How many rays will cast per degree
    public int edgeIterations                   = 4;        // Number of iterations to get a better performance of the mesh filter when the raycast hit an obstacule
    public float edgeDistance                   = 0.5f;     // Max distance to calcule the a minumun and a maximum raycast when hits something

    Transform model;
    Animator anim;

    public float health = 10; 
 
    public float generateWaypointsRange         = 9;        // How far apart you want the waypoints to be generated
    public float generateWaypointsAmount        = 3;        // How many waypoints you want the enemy to generate
    public Vector3[] waypoints;                             // All the generated waypoints where the enemy patros
    int m_CurrentWaypointIndex;                             // Current waypoint where the enemy is going to
 
    Vector3 playerLastPosition = Vector3.zero;              // Last position of the player when was near the enemy
    Vector3 m_PlayerPosition;                               // Last position of the player when the player is seen by the enemy
 
    float m_WaitTime;                                       // Variable of the wait time that makes the delay
    float m_TimeToRotate;                                   // Variable of the wait time to rotate when the player is near that makes the delay
    bool m_playerInRange;                                   // If the player is in range of vision, state of chasing
    bool m_IsPatrol;                                        // If the enemy is patrol, state of patroling
    bool m_CaughtPlayer;                                    // if the enemy has caught the player

    public float meleeAnimationD                = 0.8f;     // How long the melee attack animation plays
    public float meleeAnimationDC               = 0.0f;     // Timer for the melee attack animation
    public float meleeAttackD                   = 0.4f;     // Delay for when to spawn in the ranged prefab
    public float meleeAttackDC                  = 0.0f;     // Timer for the ranged attack prefab delay
    public bool hasAttackedMelee                = false;    // Bool used to makes sure only one attack is triggered

    private bool triggerMeleeAttackSet          = false;

    public GameObject meleePrefab;                          // The attack prefab that get's spawned in when attacking
    public Transform spawnpoint;                            // The position where the attack prefab is being spawned on

    // set variables to their intended values
    void Start()
    {
        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_CaughtPlayer = false;
        m_playerInRange = false;

        m_WaitTime = startWaitTime;                 //  Set the wait time variable that will change
        m_TimeToRotate = timeToRotate;

        model = transform.GetChild(0);
        anim = model.GetComponent<Animator>();      // get the animator component so that the animations wokr

        //  Generate the random waypoints that the enemy patrols
        for (int i = 0; i < generateWaypointsAmount; i++)
        {
            waypoints[i] = GenerateWaypointPosition();
        }

        m_CurrentWaypointIndex = 0;                 //  Set the initial waypoint
        navMeshAgent = GetComponent<NavMeshAgent>();
 
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;             //  Set the navemesh speed with the normal speed of the enemy
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]);    //  Set the destination to the first waypoint
    }
 
    /**
        Executes the enviromentview function
        If the enemy see's the player it will start chasing if not it will stay in patrol mode
    */
    private void Update()
    {
        EnviromentView();                       //  Check whether or not the player is in the enemy's field of vision

        if (!m_IsPatrol)
        {
            Chasing();
        }
        else
        {
            Patroling();
        }
    }

    /**
    The enemy will run after the player and if the enemy get's close enough it will attempt to attack the player
    */
    private void Chasing()
    {
        //  The enemy is chasing the player
        playerLastPosition = Vector3.zero;          //  Reset the player near position
 
        //keep running after the player as long as it has not been caught
        if (!m_CaughtPlayer)
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Idle", false);
            anim.SetBool("Run", true);
            Move(speedRun);
            navMeshAgent.SetDestination(m_PlayerPosition);          //  set the destination of the enemy to the player location
        }
        else
        {
            // this function spawns in the attack prefab and uses timers to sync the spawning up nicely to the attack animation
            if(!triggerMeleeAttackSet)
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", false);
                anim.SetBool("Run", false);
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
                hasAttackedMelee       = false;
                FinishedAttack();
            }
            else
            {
                meleeAnimationDC += Time.deltaTime;
            }
        }
        // check if the enemy is within distance of the player or the player's last seen position
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)    //  Control if the enemy arrive to the player location
        {
            if (m_WaitTime <= 0 && !m_CaughtPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                //  Check if the enemy is not near to the player, returns to patrol after the wait time delay
                m_IsPatrol = true;
                Move(speedWalk);
                m_TimeToRotate = timeToRotate;
                m_WaitTime = startWaitTime;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]);
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
                anim.SetBool("Run", false);
            }
            else if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 2.5f)
            {
                //  Wait if the current position is not the player position
                if(!m_CaughtPlayer)
                {
                    anim.SetBool("Walk", false);
                    anim.SetBool("Idle", true);
                    anim.SetBool("Run", false);
                }
                Stop();
                navMeshAgent.SetDestination(transform.position);
                m_WaitTime -= Time.deltaTime;
            }
            else if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= 1.5f)
            {
                // if the player get's too close the enemy will attempt to attacak it
                CaughtPlayer();
            }
        }
    }
 
    private void Patroling()
    {
        anim.SetBool("Walk", true);
        anim.SetBool("Idle", false);
        anim.SetBool("Run", false);
        playerLastPosition = Vector3.zero;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]);    //  Set the enemy destination to the next waypoint
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            //  If the enemy arrives to the waypoint position then wait for a moment and go to the next
            if (m_WaitTime <= 0)
            {
                NextPoint();
                Move(speedWalk);
                m_WaitTime = startWaitTime;
            }
            else
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", true);
                anim.SetBool("Run", false);
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }
 
    // makes the enemy go to the next waypoint
    public void NextPoint()
    {
        m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]);
    }
 
    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
    }
 
    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
    }
 
    void CaughtPlayer()
    {
        m_CaughtPlayer = true;
    }

    void FinishedAttack()
    {
        m_CaughtPlayer = false;
    }

    // function to generate a random waypoint position for the patrol mode
    private Vector3 GenerateWaypointPosition()
    {
        float spawnPosX = Random.Range(transform.position.x-generateWaypointsRange, transform.position.x+generateWaypointsRange);
        float spawnPosZ = Random.Range(transform.position.z-generateWaypointsRange, transform.position.z+generateWaypointsRange);
        Vector3 randomPos = new Vector3(spawnPosX, 0, spawnPosZ);
        return randomPos;
    }
 
    // check if the enemy can see the player
    void EnviromentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);   //  Make an overlap sphere around the enemy to detect the playermask in the view radius
 
        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);          //  Distance of the enmy and the player
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    m_playerInRange = true;             //  The player has been seeing by the enemy and then the nemy starts to chasing the player
                    m_IsPatrol = false;                 //  Change the state to chasing the player
                }
                else
                {
                    /*
                     *  If the player is behind a obstacle the player position will not be registered
                     * */
                    m_playerInRange = false;
                }
            }
            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                /*
                 *  If the player is further than the view radius, then the enemy will no longer keep the player's current position.
                 *  Or the enemy is a safe zone, the enemy will no chase
                 * */
                m_playerInRange = false;                //  Change the sate of chasing
            }
            if (m_playerInRange)
            {
                /*
                 *  If the enemy no longer sees the player, then the enemy will go to the last position that has been registered
                 * */
                m_PlayerPosition = player.transform.position;       //  Save the player's current position if the player is in range of vision
            }
        }
    }

    // function for taking damage from the player's attacks
    public void TakeDamage(int damage){
        health -= damage;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}