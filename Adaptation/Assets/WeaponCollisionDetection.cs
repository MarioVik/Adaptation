using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollisionDetection : MonoBehaviour
{
    [SerializeField]
    PlayerMeleeAttacking attackingScript;

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            attackingScript.HitEnemy(enemyHealth, enemyHealth.transform.position);
        }
    }
}