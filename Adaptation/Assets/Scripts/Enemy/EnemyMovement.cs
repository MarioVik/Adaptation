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

    bool dashEnabled;

    public void IncreaseSpeed(float increase)
    {
        navAgent.acceleration += increase;
        navAgent.speed += increase;
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        navAgent = GetComponent<NavMeshAgent>();
        //nav.speed += 1.5f;
        //nav.acceleration += 1.5f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            stopped = !stopped;
            
        if (stopped)
        {
            navAgent.SetDestination(transform.position);
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

    public void EnableDash()
    {
        dashEnabled = false;
    }

    private void Dash()
    {
        if (dashEnabled)
        {
            //Move a set distance in turned direction 
        }
    }
}