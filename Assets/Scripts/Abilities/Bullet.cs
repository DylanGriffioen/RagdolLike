using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //public floats that keep track of the bullets stats
    public float speed = 10;
    public float lifeTime = 180;
    public int damage = 1;

    // Start is called before the first frame update
    // Keeps track of the lifetime of the attack object and destroys it when the time hits 0
    void Start(){}

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        if(lifeTime <= 0){
            Destroy(gameObject);
        }
        lifeTime--;
    }

    /**
    Function to keep track of what needs to happen when the bullet collides with objects
    */
    void OnTriggerEnter(Collider other){
        if(other.gameObject.name == "Capsule"){
            Destroy(gameObject);
            other.GetComponent<AIController>().TakeDamage(damage);
        }
    }
}
