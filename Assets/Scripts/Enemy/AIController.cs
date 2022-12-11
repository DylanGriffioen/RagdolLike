using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;               //  Nav mesh agent component
    public float startWaitTime = 4;                 //  Wait time of every action
    public float timeToRotate = 2;                  //  Wait time when the enemy detect near the player without seeing
    public float speedWalk = 6;                     //  Walking speed, speed in the nav mesh agent
    public float speedRun = 9;                      //  Running speed
 
    public float viewRadius = 15;                   //  Radius of the enemy view
    public float viewAngle = 90;                    //  Angle of the enemy view
    public LayerMask playerMask;                    //  To detect the player with the raycast
    public LayerMask obstacleMask;                  //  To detect the obstacules with the raycast
    public float meshResolution = 1.0f;             //  How many rays will cast per degree
    public int edgeIterations = 4;                  //  Number of iterations to get a better performance of the mesh filter when the raycast hit an obstacule
    public float edgeDistance = 0.5f;               //  Max distance to calcule the a minumun and a maximum raycast when hits something

    Transform model;
    Animator anim;

    public float health = 10; 
 
    public float generateWaypointsRange = 9;        //  How far apart you want the waypoints to be generated
    public float generateWaypointsAmount = 3;       //  How many waypoints you want the enemy to generate
    public Vector3[] waypoints;                     //  All the generated waypoints where the enemy patros
    int m_CurrentWaypointIndex;                     //  Current waypoint where the enemy is going to
 
    Vector3 playerLastPosition = Vector3.zero;      //  Last position of the player when was near the enemy
    Vector3 m_PlayerPosition;                       //  Last position of the player when the player is seen by the enemy
 
    float m_WaitTime;                               //  Variable of the wait time that makes the delay
    float m_TimeToRotate;                           //  Variable of the wait time to rotate when the player is near that makes the delay
    bool m_playerInRange;                           //  If the player is in range of vision, state of chasing
    bool m_PlayerNear;                              //  If the player is near, state of hearing
    bool m_IsPatrol;                                //  If the enemy is patrol, state of patroling
    bool m_CaughtPlayer;                            //  if the enemy has caught the player

    public float attackDelayTime = 20;              //  Variable of the Delay time between attacs
    public float attackDelayTimer;                  //  Variable used as a timer for the Delay between attacks
    public float attackAnimationTime = 60;          //  Variable used to keep track of how long the attack animation should play
    public float attackAnimationTimer;              //  Variable used as a timer for the attack animation time
    private bool hasAttacked = false;
    private bool triggerAttackSet = false;

    public GameObject attackPrefab;                 //  The attack prefab that get's spawned in when attacking
    public Transform spawnpoint;                    //  The position where the attack prefab is being spawned on

    void Start()
    {
        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_CaughtPlayer = false;
        m_playerInRange = false;
        m_PlayerNear = false;
        m_WaitTime = startWaitTime;                 //  Set the wait time variable that will change
        m_TimeToRotate = timeToRotate;
        attackDelayTimer = attackDelayTime;
        attackAnimationTimer = attackAnimationTime;
        model = transform.GetChild(0);
        anim = model.GetComponent<Animator>();

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

    //todo fix this function it makes no sense
    private void Chasing()
    {
        //  The enemy is chasing the player
        m_PlayerNear = false;                       //  Set false that hte player is near beacause the enemy already sees the player
        playerLastPosition = Vector3.zero;          //  Reset the player near position
 
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
            if(!triggerAttackSet)
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Idle", false);
                anim.SetBool("Run", false);
                anim.SetTrigger("Attack");
                triggerAttackSet = true;
            }
            if(attackDelayTimer > 0 || hasAttacked)
            {
                attackDelayTimer--;
            }
            else
            {
                Instantiate(attackPrefab, spawnpoint.position, transform.rotation);
                hasAttacked = true;
            }
            if(attackAnimationTimer > 0)
            {
                attackAnimationTimer--;
            }
            else
            {
                attackDelayTimer = attackDelayTime;
                attackAnimationTimer = attackAnimationTime;
                hasAttacked = false;
                triggerAttackSet = false;
                FinishedAttack();
            }
        }
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)    //  Control if the enemy arrive to the player location
        {
            if (m_WaitTime <= 0 && !m_CaughtPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                //  Check if the enemy is not near to the player, returns to patrol after the wait time delay
                m_IsPatrol = true;
                m_PlayerNear = false;
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
                CaughtPlayer();
            }
        }
    }
 
    private void Patroling()
    {
        if (m_PlayerNear)
        {
            //  Check if the enemy detect near the player, so the enemy will move to that position
            if (m_TimeToRotate <= 0)
            {
                Move(speedWalk);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                //  The enemy wait for a moment and then go to the last player position
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            anim.SetBool("Walk", true);
            anim.SetBool("Idle", false);
            anim.SetBool("Run", false);
            m_PlayerNear = false;           //  The player is no near when the enemy is platroling
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
    }
 
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

    private Vector3 GenerateWaypointPosition()
    {
        float spawnPosX = Random.Range(transform.position.x-generateWaypointsRange, transform.position.x+generateWaypointsRange);
        float spawnPosZ = Random.Range(transform.position.z-generateWaypointsRange, transform.position.z+generateWaypointsRange);
        Vector3 randomPos = new Vector3(spawnPosX, 0, spawnPosZ);
        return randomPos;
    }

    void LookingPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);
        if (Vector3.Distance(transform.position, player) <= 0.3)
        {
            if (m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk);
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex]);
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }
 
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

    public void TakeDamage(int damage){
        health -= damage;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}