using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Approach, Withdraw, MeleeAttack, RangedAttack, Block, Dead };

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
    MeleeRangeTracker meleeRangeTracker;

    bool hasMelee;
    bool hasRanged;
    bool hasBlock;
    bool hasDash;

    Vector3 DirectionToPlayer { get { return (player.position - transform.position).normalized; } }
    float DistanceToPlayer { get { return Vector3.Distance(transform.position, player.position); } }
    bool InMeleeDistance { get { return meleeRangeTracker.NormalRange || meleeRangeTracker.ComboRange; } }
    bool InRangedDistance { get { return rangedAttacking.Range > DistanceToPlayer; } }
    bool FarRangeIncrement { get { return InRangedDistance && DistanceToPlayer > (rangedAttacking.Range / 3) * 2; } }
    bool MidleRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && DistanceToPlayer > rangedAttacking.Range / 3; } }
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
        meleeRangeTracker = GetComponentInChildren<MeleeRangeTracker>();

        hasMelee = traits.Melee;
        hasRanged = traits.Ranged;
        hasBlock = blocking.gameObject.activeSelf;
        hasDash = dashing.isActiveAndEnabled;

        currentState = EnemyState.Approach;
    }

    void Update()
    {
        if (health.IsDead)
            currentState = EnemyState.Dead;

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Approach:
                UpdateApproach();
                break;
            case EnemyState.Withdraw:
                UpdateWithraw();
                break;
            case EnemyState.MeleeAttack:
                UpdateMeleeAttack();
                break;
            case EnemyState.RangedAttack:
                UpdateRangedAttack();
                break;
            case EnemyState.Block:

                break;
            case EnemyState.Dead:
                navAgent.enabled = false;
                controlManager.Dead = true;
                break;
        }

        if (Input.GetKeyDown(KeyCode.O))
            currentState = EnemyState.Idle;

        Debug.Log(currentState.ToString());
    }

    void UpdateIdle()
    {
        navAgent.SetDestination(transform.position);
        navAgent.isStopped = true;

        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;

        if (Input.GetKeyDown(KeyCode.O))
            currentState = EnemyState.Approach;
    }

    void UpdateApproach()
    {
        navAgent.SetDestination(player.position);

        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;

        if (hasMelee && InMeleeDistance)
            currentState = EnemyState.MeleeAttack;
        else if (hasRanged && InRangedDistance)
            currentState = EnemyState.RangedAttack;
    }

    void UpdateWithraw()
    {
        Vector3 newPos = transform.position + (DirectionToPlayer * -2);
        navAgent.SetDestination(newPos);

        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);
        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;

        if (dashing.isActiveAndEnabled && dashing.Ready)
            dashing.Activate();

        if (!CloseRangeIncrement)
            currentState = EnemyState.RangedAttack;
    }

    void UpdateMeleeAttack()
    {
        navAgent.SetDestination(transform.position);
        controlManager.MoveInput = new Vector3(navAgent.velocity.x, 0, navAgent.velocity.z);

        if (meleeRangeTracker.ComboRange)
        {
            controlManager.NormalAttackInput = false;
            controlManager.ComboAttackInput = true;
        }
        else if (meleeRangeTracker.NormalRange)
        {
            controlManager.NormalAttackInput = true;
            controlManager.ComboAttackInput = false;
        }

        if (!InMeleeDistance)
            currentState = EnemyState.Approach;
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
            if (playerMelee)
            {
                currentState = EnemyState.Withdraw;
            }
            else
            {
                controlManager.NormalAttackInput = true;
                controlManager.ComboAttackInput = false;
            }
        }
        else
        {
            throw new System.Exception("Error: Unknown distance to player");
        }
    }
}