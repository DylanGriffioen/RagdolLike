using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
   public float speed = 10;
   public float lifeTime = 180;
   public int damage = 1;

    // private Rigidbody BulletRB;
    // Start is called before the first frame update
    void Start()
    {
        // BulletRB = GetComponent<Rigidbody>();      
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
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
