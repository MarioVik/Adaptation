using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Approach, Withdraw, MeleeAttack, RangedAttack, Dash, Block, Dead };

public class FiniteStateMachine : MonoBehaviour
{
    EnemyState currentState;

    [SerializeField]
    EnemyMeleeAttacking meleeAttacking;
    [SerializeField]
    EnemyRangedAttacking rangedAttacking;
    [SerializeField]
    EnemyBlocking blocking;

    EnemyDashing dashing;
    EnemyControlManager controlManager;
    EnemyHealth health;
    EnemyTraits traits;

    Transform player;
    PlayerHealth playerHealth;

    NavMeshAgent navAgent;

    bool hasMelee;
    bool hasRanged;
    bool hasBlock;
    bool hasDash;

    bool stopped;

    void Start()
    {
        dashing = GetComponentInChildren<EnemyDashing>();
        controlManager = GetComponent<EnemyControlManager>();
        health = GetComponentInChildren<EnemyHealth>();
        traits = GetComponentInChildren<EnemyTraits>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();

        navAgent = GetComponent<NavMeshAgent>();

        hasMelee = meleeAttacking.gameObject.activeSelf;
        hasRanged = rangedAttacking.gameObject.activeSelf;
        hasBlock = blocking.gameObject.activeSelf;
        hasDash = dashing.isActiveAndEnabled;
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

        if (health.IsDead)
            currentState = EnemyState.Dead;

        switch (currentState)
        {
            case EnemyState.Approach:
                UpdateApproach();
                break;
            case EnemyState.Withdraw:
                break;
            case EnemyState.MeleeAttack:
                break;
            case EnemyState.RangedAttack:
                UpdateRangedAttack();
                break;
            case EnemyState.Dash:
                break;
            case EnemyState.Block:
                break;
            case EnemyState.Dead:
                navAgent.enabled = false;
                controlManager.Dead = true;
                break;
        }
    }

    void UpdateApproach()
    {
        navAgent.SetDestination(player.position);
        controlManager.SetMoveInput(navAgent.desiredVelocity.x, navAgent.desiredVelocity.z);

        if (rangedAttacking.Range > Vector3.Distance(transform.position, player.position))
        {
            currentState = EnemyState.RangedAttack;
        }
    }

    void UpdateRangedAttack()
    {
        //rangedAttacking.Attack();
        controlManager.SetAttackInput(true);
    }
}