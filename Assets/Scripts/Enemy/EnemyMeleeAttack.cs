using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    public float lifeTime = 0.3f;
    public int damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    // Keeps track of the lifetime of the attack object and destroys it when the time hits 0
    void Update(){}

    /**
    Function to keep track of what needs to happen when the melee attack collides with objects
    */
    void OnTriggerEnter(Collider other){
        if(other.gameObject.name == "Player"){
            Destroy(gameObject);
            other.GetComponent<PlayerController>().TakeDamage(damage);
        }
    }
}
