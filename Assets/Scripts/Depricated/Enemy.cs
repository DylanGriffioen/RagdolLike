//Depricated

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed, health, smoothTurnTime;

    float targetAngle, smoothAngle, smoothTurnVelocity;

    Vector3 moveDir;

    private Rigidbody rb;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;

        rb.velocity = (lookDirection * moveSpeed);

        Quaternion rotation = Quaternion.LookRotation(lookDirection);

        Quaternion current = transform.localRotation;

        transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime * moveSpeed);

        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collision.gameObject.name == "Bullet(Clone)")
        {
            //If the GameObject's name matches the one you suggest, output this message in the console
            health--;
        }
    }
}
