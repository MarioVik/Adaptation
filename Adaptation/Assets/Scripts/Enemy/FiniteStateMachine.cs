using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Approach, Withdraw, MeleeAttack, RangedAttack, Block, Dead };

public class FiniteStateMachine : MonoBehaviour
{
    EnemyState currentState;

    [SerializeField]
    MeleeAttackFeature meleeAttacking;
    [SerializeField]
    RangedAttackFeature rangedAttacking;
    [SerializeField]
    BlockingFeature blocking;

    DashingFeature dashing;
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

    int incomingCollisions = 0;

    Vector3 DirectionToPlayer { get { return (player.position - transform.position).normalized; } }
    float DistanceToPlayer { get { return Vector3.Distance(transform.position, player.position); } }
    bool InMeleeDistance { get { return meleeRangeTracker.NormalRange || meleeRangeTracker.ComboRange; } }
    bool InRangedDistance { get { return rangedAttacking.Range > DistanceToPlayer; } }
    bool FarRangeIncrement { get { return InRangedDistance && DistanceToPlayer > (rangedAttacking.Range / 3) * 2; } }
    bool MidleRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && DistanceToPlayer > (rangedAttacking.Range / 3) * 1.2f; } }
    bool CloseRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && !MidleRangeIncrement; } }

    public void IncreaseMovementSpeed(float increase)
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        navAgent.acceleration += increase;
        navAgent.speed += increase;
    }

    void Start()
    {
        dashing = GetComponentInChildren<DashingFeature>();
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
        hasBlock = traits.Block;
        hasDash = traits.Dash;

        currentState = EnemyState.Approach;
    }

    Vector3 GetCurrentDirection()
    {
        NavMeshHit hit;
        navAgent.SamplePathPosition(0, 1, out hit);

        Vector3 direction = hit.position - transform.position;

        return direction.normalized;
    }

    void UpdateDirection()
    {
        Vector3 direction = GetCurrentDirection();
        controlManager.VerticalInput = direction.z;
        controlManager.HorizontalInput = direction.x;
    }

    void UpdateInput()
    {
        controlManager.VerticalInput = navAgent.velocity.z;
        controlManager.HorizontalInput = navAgent.velocity.x;

        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;
        controlManager.FeatureInput = false;
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
                UpdateBlock();
                break;
            case EnemyState.Dead:
                navAgent.enabled = false;
                controlManager.Dead = true;
                break;
        }

        if (controlManager.canMove)
        {
            navAgent.isStopped = false;
        }
        else
        {
            if (!navAgent.isStopped)
                navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }

        if (Input.GetKeyDown(KeyCode.O))
            currentState = EnemyState.Idle;

        Debug.Log(currentState.ToString());
    }

    void UpdateIdle()
    {
        navAgent.SetDestination(transform.position);

        if (Input.GetKeyDown(KeyCode.O))
            currentState = EnemyState.Approach;
    }

    void UpdateApproach()
    {
        //if (!hasDash)
        //{
        navAgent.SetDestination(player.position);
        UpdateInput();

        if (hasMelee && InMeleeDistance)
            currentState = EnemyState.MeleeAttack;
        else if (hasRanged && InRangedDistance)
            currentState = EnemyState.RangedAttack;

        if (incomingCollisions > 0 && blocking.Ready)
            currentState = EnemyState.Block;
        ////}
        //else
        //{
        //    if (!dashing.Dashing)
        //    {
        //        navAgent.SetDestination(player.position);
        //        UpdateInput();

        //        if (hasMelee && InMeleeDistance)
        //            currentState = EnemyState.MeleeAttack;
        //        else if (hasRanged && InRangedDistance)
        //            currentState = EnemyState.RangedAttack;

        //        if (incomingCollisions > 0 && blocking.Ready)
        //            currentState = EnemyState.Block;
        //    }

        //    if (hasMelee)
        //    {
        //        if (dashing.Ready && (DistanceToPlayer > dashing.Range / 4))
        //        {
        //            UpdateDirection();
        //            controlManager.FeatureInput = true;
        //        }
        //    }
        //    else if (hasRanged)
        //    {
        //        if (dashing.Ready && (DistanceToPlayer >= dashing.Range + rangedAttacking.Range))
        //        {
        //            UpdateDirection();
        //            controlManager.FeatureInput = true;
        //        }
        //    }
        //}
    }

    void UpdateWithraw()
    {
        if (!hasDash)
        {
            Vector3 newPos = transform.position + (DirectionToPlayer * -2);
            navAgent.SetDestination(newPos);
            UpdateInput();

            if (!CloseRangeIncrement)
                currentState = EnemyState.RangedAttack;

            if (incomingCollisions > 0 && blocking.Ready)
                currentState = EnemyState.Block;
        }
        else
        {
            if (!dashing.Dashing)
            {
                Vector3 newPos = transform.position + (DirectionToPlayer * -2);
                navAgent.SetDestination(newPos);
                UpdateInput();

                if (!CloseRangeIncrement)
                    currentState = EnemyState.RangedAttack;
            }

            if (dashing.Ready)
            {
                UpdateDirection();
                controlManager.FeatureInput = true;
            }
        }
    }

    void UpdateMeleeAttack()
    {
        UpdateInput();

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

        if (hasBlock)
            if (incomingCollisions > 0 && blocking.Ready)
                currentState = EnemyState.Block;
    }

    void UpdateRangedAttack()
    {
        UpdateInput();

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

        if (hasBlock)
            if (incomingCollisions > 0 && blocking.Ready)
                currentState = EnemyState.Block;
    }

    private void UpdateBlock()
    {
        UpdateInput();

        if (blocking.Ready || blocking.Blocking)
        {
            controlManager.FeatureInput = true;
        }

        if (incomingCollisions <= 0 || blocking.BlockStop)
        {
            controlManager.FeatureInput = false;
            currentState = EnemyState.Approach;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBlock)
        {
            if (other.tag == "OuterProjectile")
            {
                incomingCollisions++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasBlock)
        {
            if (other.tag == "OuterProjectile")
            {
                incomingCollisions--;
            }
        }
    }
}