using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackFeature : MonoBehaviour
{
    public bool Attacking { get; private set; }

    [SerializeField]
    bool isPlayer;

    // Only if user is player
    List<EnemyHealth> hitEnemies;
    //

    // Only if user is enemy
    PlayerHealth playerHealth;
    DashingFeature playerDashing;
    BlockingFeature playerBlocking;
    //

    Collider weaponcollider;

    int damage = 40;
    float speed, baseSpeed;
    bool combo;

    AudioSource weaponAudio;

    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;
    Animator anim;

    public void IncreaseAttackDamage(int increase) => damage += increase;

    public void IncreaseAttackSpeed(float increase) => speed += increase;

    public void IncreaseAttackRange(float increase)
    {
        weaponcollider.transform.localScale = new Vector3(
            weaponcollider.transform.localScale.x,
            weaponcollider.transform.localScale.y + increase,
            weaponcollider.transform.localScale.z);
    }

    public void Activate(bool combo = false)
    {
        this.combo = combo;
        Attacking = true;

        weaponcollider.enabled = true;

        weaponAudio.Play();

        anim.speed = speed;

        if (combo) animationDuration = comboClip.length;
        else animationDuration = normalClip.length;

        // Scaling to current speed
        animationDuration /= speed;
        // Cutting the duration time to 60% of the full clip length since clip includes some time margin
        //animationDuration *= 0.6f;

        anim.SetBool("attacking", Attacking);
        anim.SetBool("combo", combo);
        //Debug.Log("Attack Enabled");
    }

    public void Disable()
    {
        animationTimer = 0;
        anim.speed = baseSpeed;
        Attacking = false;
        combo = false;
        weaponcollider.enabled = false;

        if (isPlayer)
        {
            foreach (EnemyHealth tempEnemy in hitEnemies)
                tempEnemy.AlreadyHit = false;
            hitEnemies.Clear();
        }
        else
        {
            playerHealth.AlreadyHit = false;
        }

        anim.SetBool("attacking", Attacking);
        anim.SetBool("combo", combo);
    }

    private void Awake()
    {
        weaponAudio = GetComponent<AudioSource>();

        weaponcollider = GetComponent<CapsuleCollider>();
        weaponcollider.enabled = false;

        anim = GetComponentInParent<Animator>();
        baseSpeed = anim.speed;
        speed = baseSpeed;

        normalClip = GetAnimationTime("NormalAttack01_SwordShield");
        comboClip = GetAnimationTime("NormalAttack02_SwordShield");

        if (isPlayer)
        {
            hitEnemies = new List<EnemyHealth>();
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player.GetComponent<PlayerHealth>();
            playerDashing = player.GetComponent<DashingFeature>();
            playerBlocking = player.GetComponentInChildren<BlockingFeature>();
        }
    }

    public AnimationClip GetAnimationTime(string name)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
            if (clip.name == name)
                return clip;
        throw new System.ArgumentNullException("No matching animation clip found for attack");
    }

    void Update()
    {
        if (Attacking)
        {
            animationTimer += Time.deltaTime;
            if (combo && animationTimer >= (animationDuration * 0.3f))
            {
                combo = false;
                weaponAudio.Play();

                if (isPlayer)
                {
                    foreach (EnemyHealth tempEnemy in hitEnemies)
                        tempEnemy.AlreadyHit = false;
                }
                else
                {
                    playerHealth.AlreadyHit = false;
                }

                //Debug.Log("Collider hits reset");
            }
            if (animationTimer >= animationDuration)
            {
                Disable();

                //Debug.Log("Attack Disabled");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayer)
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                HitEnemy(enemyHealth, enemyHealth.transform.position);
            }
        }
        else
        {
            if (other.tag == "Player")
            {
                HitPlayer();
            }
        }
    }

    private void HitEnemy(EnemyHealth enemyHealth, Vector3 hitPoint)
    {
        if (!enemyHealth.AlreadyHit && !enemyHealth.IsDead)
        {
            DashingFeature enemyDashing = enemyHealth.GetComponent<DashingFeature>();
            if (enemyDashing != null && enemyDashing.isActiveAndEnabled && enemyDashing.Dashing)
                return;

            enemyHealth.AlreadyHit = true;
            enemyHealth.GetComponentInParent<EnemyControlManager>().GetHit();

            BlockingFeature enemyBlocking = enemyHealth.GetComponentInChildren<BlockingFeature>();
            if (enemyBlocking != null && enemyBlocking.isActiveAndEnabled && enemyBlocking.Blocking)
                return;

            enemyHealth.TakeDamage(damage, hitPoint);
            hitEnemies.Add(enemyHealth);
            Debug.Log("Enemy hit");
        }
    }

    private void HitPlayer()
    {
        if (!playerHealth.AlreadyHit && !playerHealth.IsDead)
        {
            if (playerDashing != null && playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                return;

            playerHealth.AlreadyHit = true;
            playerHealth.GetComponentInParent<PlayerControlManager>().GetHit();

            if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
                return;

            playerHealth.TakeDamage(damage);
            GetComponentInParent<FitnessTracker>().DamagedPlayer(damage);
            //Debug.Log("Player hit");
        }
    }
}