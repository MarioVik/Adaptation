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

    EnemyControlManager controlManager;
    EnemyHealth health;
    EnemyTraits traits;
    DashingFeature dashing;

    bool hasMelee;
    bool hasRanged;
    bool hasBlock;
    bool hasDash;

    Transform player;
    PlayerHealth playerHealth;
    bool playerHasMelee, playerHasBlock;
    MeleeAttackFeature playerMeleeAttacking;
    BlockingFeature playerBlocking;

    NavMeshAgent navAgent;
    MeleeRangeTracker meleeRangeTracker;

    Vector3 DirectionToPlayer { get { return (player.position - transform.position).normalized; } }
    Vector3 DirectionFromPlayer { get { return (transform.position - player.position).normalized; } }

    float DistanceToPlayer { get { return Vector3.Distance(transform.position, player.position); } }
    bool InMeleeDistance { get { return meleeRangeTracker.NormalRange || meleeRangeTracker.ComboRange; } }
    bool InRangedDistance { get { return rangedAttacking.Range > DistanceToPlayer; } }
    bool FarRangeIncrement { get { return InRangedDistance && DistanceToPlayer > (rangedAttacking.Range / 3) * 2; } }
    bool MidleRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && DistanceToPlayer > (rangedAttacking.Range / 3) * 1.2f; } }
    bool CloseRangeIncrement { get { return InRangedDistance && !FarRangeIncrement && !MidleRangeIncrement; } }

    int incomingProjectiles;
    public bool InPlayerMeleeRange { get; set; }

    float blockTimer, blockTimeMargin;
    float meleeMargin = 0.4f;
    float rangedMargin = 1.0f;

    public void IncrementColllisions() => incomingProjectiles++;

    public void DecrementCollisions() => incomingProjectiles--;

    public void IncreaseMovementSpeed(float increase)
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        navAgent.acceleration += increase;
        navAgent.speed += increase;
    }

    void Start()
    {
        health = GetComponentInChildren<EnemyHealth>();
        traits = GetComponentInChildren<EnemyTraits>();

        hasMelee = traits.Melee;
        hasRanged = traits.Ranged;
        hasBlock = traits.Block;
        hasDash = traits.Dash;

        if (hasDash)
            dashing = GetComponentInChildren<DashingFeature>();
        controlManager = GetComponent<EnemyControlManager>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();

        playerHasMelee = player.GetComponent<PlayerTraits>().Melee;
        if (playerHasMelee)
            playerMeleeAttacking = player.GetComponentInChildren<MeleeAttackFeature>();

        playerHasBlock = player.GetComponent<PlayerTraits>().Block;
        if (playerHasBlock)
            playerBlocking = player.GetComponentInChildren<BlockingFeature>();

        navAgent = GetComponent<NavMeshAgent>();
        meleeRangeTracker = GetComponentInChildren<MeleeRangeTracker>();

        currentState = EnemyState.Approach;
    }

    void UpdateInput()
    {
        controlManager.VerticalInput = navAgent.velocity.z;
        controlManager.HorizontalInput = navAgent.velocity.x;

        controlManager.NormalAttackInput = false;
        controlManager.ComboAttackInput = false;
        controlManager.FeatureInput = false;
    }

    void UpdateDirectionTowards()
    {
        Vector3 direction = DirectionToPlayer;
        controlManager.VerticalInput = direction.z;
        controlManager.HorizontalInput = direction.x;
        controlManager.DashDirection = direction;
    }

    void UpdateDirectionAway()
    {
        Vector3 direction = DirectionFromPlayer;
        controlManager.VerticalInput = direction.z;
        controlManager.HorizontalInput = direction.x;
        controlManager.DashDirection = direction;
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
                if (hasBlock)
                    Destroy(blocking);
                else if (hasDash)
                    Destroy(dashing);

                navAgent.enabled = false;
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
        if (!navAgent.isStopped)
            navAgent.velocity = Vector3.zero;
        navAgent.isStopped = true;

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
                if (AvoidableAttackIncoming())
                {
                    currentState = EnemyState.Avoid;
                }
                else if (dashing.Ready && (DistanceToPlayer >= dashing.Range))
                {
                    UpdateDirectionTowards();
                    controlManager.FeatureInput = true;
                }
            }
            else if (hasRanged)
            {
                if (AvoidableAttackIncoming())
                {
                    currentState = EnemyState.Avoid;
                }
                else if (dashing.Ready && (DistanceToPlayer >= dashing.Range + rangedAttacking.Range))
                {
                    UpdateDirectionTowards();
                    controlManager.FeatureInput = true;
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

            if (AvoidableAttackIncoming())
                currentState = EnemyState.Avoid;
        }
    }

    void UpdateMeleeAttack()
    {
        UpdateInput();

        if (!controlManager.canMove)
            return;

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
        UpdateInput();

        if (!controlManager.canMove)
            return;

        if (AvoidableAttackIncoming())
        {
            currentState = EnemyState.Avoid;
            return;
        }

        if (!InRangedDistance)
        {
            currentState = EnemyState.Approach;
        }
        else if (FarRangeIncrement/* && !(playerHasBlock && playerBlocking.Blocking)*/)
        {
            controlManager.NormalAttackInput = false;
            controlManager.ComboAttackInput = true;
        }
        else if (MidleRangeIncrement/* && !(playerHasBlock && playerBlocking.Blocking)*/)
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
            currentState = EnemyState.Withdraw;
        }
    }

    bool MeleeAttackIncoming()
    {
        if (playerMeleeAttacking.Attacking)
        {
            if (InPlayerMeleeRange)
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
                    blockTimeMargin = meleeMargin;
                    return true;
                }
            }

            if (incomingProjectiles > 0)
            {
                blockTimeMargin = rangedMargin;
                return true;
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
                if (incomingProjectiles <= 0)
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
            if (dashing.Dashing)
            {
                controlManager.FeatureInput = false;
                currentState = EnemyState.Approach;
                return;
            }

            if (hasMelee)
                UpdateDirectionTowards();
            else
                UpdateDirectionAway();

            controlManager.FeatureInput = true;
        }
    }
}