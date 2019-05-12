using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterMeleeBehaviour : MonoBehaviour
{
    //bool isPlayer;

    private void Start()
    {
        //GetComponentInParent<MeleeAttackFeature>().player
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectile")
        {
            other.GetComponent<ProjectileBehaviour>().Sizzle();
        }
    }
}