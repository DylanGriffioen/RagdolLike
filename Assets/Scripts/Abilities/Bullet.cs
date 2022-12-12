using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //public floats that keep track of the bullets stats
    public float speed = 10;
    public float lifeTime = 1;
    public int damage;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /**
    Update is called once per frame
    Keeps track of the lifetime of the attack object and destroys it when the time hits 0
    */
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    /**
    Set the damage of the bullet
    */
    public  void SetDamage(int newdamage)
    {
        damage = newdamage;
    }

    /**
    Increase the damage of the bullet
    */
    public void IncreaseDamage(int damageIncrease)
    {
        damage += damageIncrease;
    }

    /**
    Function to keep track of what needs to happen when the bullet collides with objects
    */
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Enemy"){
            Destroy(gameObject);
            other.GetComponent<AIController>().TakeDamage(damage);
        }
    }
}
