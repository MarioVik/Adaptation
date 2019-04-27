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
    bool playerMelee;

    NavMeshAgent navAgent;

    bool hasMelee;
    bool hasRanged;
    bool hasBlock;
    bool hasDash;

    Vector3 DirectionToPlayer { get { return (player.position - transform.position).normalized; } }
    float DistanceToPlayer { get { return Vector3.Distance(transform.position, player.position); } }
    bool InMeleeDistance { get { return meleeAttacking.Range > DistanceToPlayer; } }
    bool InRangedDistance { get { return rangedAttacking.Range > DistanceToPlayer; } }
    bool FarRangeIncrement { get { return InRangedDistance && DistanceToPlayer > (rangedAttacking.Range / 4) * 3; } }
    bool MidleRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && DistanceToPlayer > rangedAttacking.Range / 4; } }
    bool CloseRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && !MidleRangeIncrement; } }

    bool stopped;

    void Start()
    {
        dashing = GetComponentInChildren<EnemyDashing>();
        controlManager = GetComponent<EnemyControlManager>();
        health = GetComponentInChildren<EnemyHealth>();
        traits = GetComponentInChildren<EnemyTraits>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        playerMelee = player.GetComponent<PlayerTraits>().Melee;

        navAgent = GetComponent<NavMeshAgent>();

        hasMelee = meleeAttacking.gameObject.activeSelf;
        hasRanged = rangedAttacking.gameObject.activeSelf;
        hasBlock = blocking.gameObject.activeSelf;
        hasDash = dashing.isActiveAndEnabled;

        currentState = EnemyState.Approach;
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
                UpdateWithraw();
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

        Debug.Log(currentState.ToString());
    }

    void UpdateApproach()
    {
        navAgent.SetDestination(player.position);

        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;

        if (InRangedDistance)
            currentState = EnemyState.RangedAttack;
    }

    void UpdateWithraw()
    {
        //if (hasDash)
        //{

        //}
        //else
        {
            Vector3 newPos = transform.position + (DirectionToPlayer * -2);

            navAgent.SetDestination(newPos);

            controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
            controlManager.NormalAttackInput = false;
            controlManager.ComboAttackInput = false;

            if (!CloseRangeIncrement)
                currentState = EnemyState.RangedAttack;
        }
    }


    void UpdateRangedAttack()
    {
        navAgent.SetDestination(transform.position);
        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);

        if (!InRangedDistance)
        {
            currentState = EnemyState.Approach;
        }
        else if (FarRangeIncrement)
        {
            controlManager.NormalAttackInput = false;
            controlManager.ComboAttackInput = true;
        }
        else if (MidleRangeIncrement)
        {
            controlManager.NormalAttackInput = true;
            controlManager.ComboAttackInput = false;
        }
        else if (CloseRangeIncrement)
        {
            //if (playerMelee)
            {
                currentState = EnemyState.Withdraw;
            }
            //else
            //{
            //    controlManager.NormalAttackInput = true;
            //    controlManager.ComboAttackInput = false;
            //}
        }
        else
        {
            throw new System.Exception("Error: Unknown distance to player");
        }
    }
}