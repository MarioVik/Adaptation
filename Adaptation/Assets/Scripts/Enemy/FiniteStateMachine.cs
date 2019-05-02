using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Approach, Withdraw, MeleeAttack, RangedAttack, Avoid, Dead };

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
    bool playerHasMelee;
    MeleeAttackFeature playerMeleeAttacking;
    MeleeRangeTracker playerMeleeRangeTracker;

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
    bool MidleRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && DistanceToPlayer > (rangedAttacking.Range / 3) * 1.2f; } }
    bool CloseRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && !MidleRangeIncrement; } }

    int incomingCollisions = 0;

    float blockTimer = 0;
    float blockTimeMargin = 0.5f;

    public void IncrementColllisions() => incomingCollisions++;

    public void DecrementCollisions() => incomingCollisions--;

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
        playerHasMelee = player.GetComponent<PlayerTraits>().Melee;
        if (playerHasMelee)
        {
            playerMeleeAttacking = player.GetComponentInChildren<MeleeAttackFeature>();
            playerMeleeRangeTracker = player.GetComponentInChildren<MeleeRangeTracker>();
        }

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
            case EnemyState.Avoid:
                UpdateAvoid();
                break;
            case EnemyState.Dead:
                navAgent.enabled = false;
                controlManager.Dead = true;
                return;
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

        //Debug.Log(currentState.ToString());
    }

    void UpdateIdle()
    {
        navAgent.SetDestination(transform.position);

        if (Input.GetKeyDown(KeyCode.O))
            currentState = EnemyState.Approach;
    }

    void UpdateApproach()
    {
        if (!hasDash)
        {
            navAgent.SetDestination(player.position);
            UpdateInput();

            if (hasMelee && InMeleeDistance)
                currentState = EnemyState.MeleeAttack;
            else if (hasRanged && InRangedDistance)
                currentState = EnemyState.RangedAttack;

            if (AvoidableAttackIncoming())
                currentState = EnemyState.Avoid;
        }
        else
        {
            if (!dashing.Dashing)
            {
                navAgent.SetDestination(player.position);
                UpdateInput();

                if (hasMelee && InMeleeDistance)
                    currentState = EnemyState.MeleeAttack;
                else if (hasRanged && InRangedDistance)
                    currentState = EnemyState.RangedAttack;
            }

            if (hasMelee)
            {
                if (dashing.Ready && (DistanceToPlayer > dashing.Range / 4))
                {
                    UpdateDirection();
                    controlManager.FeatureInput = true;
                }
                else
                {
                    if (AvoidableAttackIncoming())
                        currentState = EnemyState.Avoid;
                }
            }
            else if (hasRanged)
            {
                if (dashing.Ready && (DistanceToPlayer >= dashing.Range + rangedAttacking.Range))
                {
                    UpdateDirection();
                    controlManager.FeatureInput = true;
                }
                else
                {
                    if (AvoidableAttackIncoming())
                        currentState = EnemyState.Avoid;
                }
            }
        }
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

            if (AvoidableAttackIncoming())
                currentState = EnemyState.Avoid;
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
        if (!controlManager.canMove)
            return;

        UpdateInput();

        if (AvoidableAttackIncoming())
        {
            currentState = EnemyState.Avoid;
            return;
        }

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
        if (!controlManager.canMove)
            return;

        UpdateInput();

        if (AvoidableAttackIncoming())
        {
            currentState = EnemyState.Avoid;
            return;
        }

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
            if (playerHasMelee)
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

    bool MeleeAttackIncoming()
    {
        if (playerMeleeAttacking.Attacking)
        {
            if (playerMeleeRangeTracker.NormalRange || playerMeleeRangeTracker.ComboRange)
            {
                return true;
            }
        }
        return false;
    }

    bool AvoidableAttackIncoming()
    {
        if ((hasDash && dashing.Ready) || (hasBlock && blocking.Ready))
        {
            if (playerHasMelee)
            {
                if (MeleeAttackIncoming())
                {
                    Debug.Log("Attack Incoming");
                    return true;
                }
            }
            else
            {
                if (incomingCollisions > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void UpdateAvoid()
    {
        UpdateInput();

        if (hasBlock)
        {
            if (blocking.Ready || blocking.Blocking)
            {
                controlManager.FeatureInput = true;
            }

            if (playerHasMelee)
            {
                if (!MeleeAttackIncoming())
                    blockTimer += Time.deltaTime;
                else
                    blockTimer = 0;
            }
            else
            {
                if (incomingCollisions <= 0)
                    blockTimer += Time.deltaTime;
                else
                    blockTimer = 0;
            }

            if ((blockTimer >= blockTimeMargin) || blocking.BlockStop)
            {
                controlManager.FeatureInput = false;
                currentState = EnemyState.Approach;
            }
        }
        else if (hasDash)
        {
            UpdateDirection();
            controlManager.FeatureInput = true;
        }
    }
}