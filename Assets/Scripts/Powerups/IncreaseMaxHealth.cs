using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseMaxHealth : MonoBehaviour
{
    public int healthIncrease = 1;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update(){}

    /**
    Function to keep track of what needs to happen when the powerup attack collides with objects
    */
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            Destroy(gameObject);
            other.GetComponent<PlayerController>().IncreaseMaxHealth(healthIncrease);
        }
    }
}
