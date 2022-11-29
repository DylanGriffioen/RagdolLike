using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UseAbility : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject meleePrefab;
    public float ammo;
    public Transform spawnpoint;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnRangedAttack()
    {
        if(ammo > 0)
        {
            Instantiate(bulletPrefab, spawnpoint.position, transform.rotation);
            ammo--;
        }
    }

    void OnMeleeAttack()
    {
        Instantiate(meleePrefab, spawnpoint.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
