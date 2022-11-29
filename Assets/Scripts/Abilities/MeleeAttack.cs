using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public float lifeTime = 5;
    public int damage = 1;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update()
    {
        if(lifeTime <= 0){
            Destroy(gameObject);
        }
        lifeTime--;
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.name == "Capsule"){
            Destroy(gameObject);
            other.GetComponent<AIController>().TakeDamage(damage);
        }
    }
}