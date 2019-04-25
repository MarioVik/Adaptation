using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    bool stopped;

    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    NavMeshAgent navAgent;

    public void IncreaseSpeed(float increase)
    {
        navAgent.acceleration += increase;
        navAgent.speed += increase;
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        navAgent = GetComponent<NavMeshAgent>();
        //nav.speed += 1.5f;
        //nav.acceleration += 1.5f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.O))
            stopped = !stopped;

        if (stopped)
        {
            navAgent.isStopped = true;
            return;
        }


        if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            navAgent.SetDestination(player.position);
        }
        else
        {
            navAgent.enabled = false;
        }
        
    }
}