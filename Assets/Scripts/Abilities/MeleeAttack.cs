using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public float lifeTime = 5;
    public int damage;

    // Start is called before the first frame update
    void Start() {}

    /**
    Update is called once per frame
    Keeps track of the lifetime of the attack object and destroys it when the time hits 0
    */
    void Update()
    {
        if(lifeTime <= 0){
            Destroy(gameObject);
        }
        lifeTime--;
    }

    /**
    Set the damage of the melee attack
    */
    public  void SetDamage(int newdamage)
    {
        damage = newdamage;
    }

    /**
    Increase the damage of the melee attack
    */
    public void IncreaseDamage(int damageIncrease)
    {
        damage += damageIncrease;
    }

    /**
    Function to keep track of what needs to happen when the melee attack collides with objects
    */
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Enemy"){
            Destroy(gameObject);
            other.GetComponent<AIController>().TakeDamage(damage);
        } 
    }
}
