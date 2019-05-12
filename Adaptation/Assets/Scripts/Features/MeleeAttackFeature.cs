using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MeleeAttackFeature : MonoBehaviour
{
    public bool Attacking { get; private set; }

    [SerializeField]
    bool isPlayer;

    // Only if user is player
    List<EnemyHealth> hitEnemies;
    //

    // Only if user is enemy
    bool inflictedDamage, outerHit;
    PlayerHealth playerHealth;
    DashingFeature playerDashing;
    BlockingFeature playerBlocking;
    //

    Collider weaponcollider;
    [SerializeField] Collider outerCollider;

    float baseDamage = 70;
    float damage;
    float speed, baseSpeed;
    bool combo;

    [SerializeField]
    AudioClip[] audioClips;
    AudioSource weaponAudio;
    Random rand;

    Animator anim;
    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;

    public void IncreaseAttackDamage() => damage += (baseDamage * 0.125f);

    public void IncreaseAttackSpeed(float increase) => speed += increase;

    public void IncreaseAttackRange(float increase)
    {
        weaponcollider.transform.localScale = new Vector3(
            weaponcollider.transform.localScale.x,
            weaponcollider.transform.localScale.y + increase,
            weaponcollider.transform.localScale.z);

        outerCollider.transform.localScale = new Vector3(
                    outerCollider.transform.localScale.x,
                    outerCollider.transform.localScale.y + increase,
                    outerCollider.transform.localScale.z);
    }

    public void Activate(bool combo = false)
    {
        this.combo = combo;
        Attacking = true;

        weaponcollider.enabled = true;
        outerCollider.enabled = true;

        weaponAudio.clip = audioClips[rand.Next(0, audioClips.Length)];
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
        outerCollider.enabled = false;

        if (isPlayer)
        {
            foreach (EnemyHealth tempEnemy in hitEnemies)
                tempEnemy.AlreadyHit = false;
            hitEnemies.Clear();
        }
        else
        {
            if (!inflictedDamage && outerHit)
            {
                GetComponentInParent<FitnessTracker>().AlmostDamagedPlayer(damage);
            }

            inflictedDamage = false;
            outerHit = false;
            playerHealth.AlreadyHit = false;
        }

        anim.SetBool("attacking", Attacking);
        anim.SetBool("combo", combo);
    }

    private void Awake()
    {
        damage = baseDamage;

        weaponAudio = GetComponent<AudioSource>();

        weaponcollider = GetComponent<CapsuleCollider>();
        weaponcollider.enabled = false;

        outerCollider.enabled = false;

        anim = GetComponentInParent<Animator>();
        baseSpeed = anim.speed;
        speed = baseSpeed;

        rand = new Random();

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
        throw new ArgumentNullException("No matching animation clip found for attack");
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
            if (other.tag == "Enemy")
            {
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                HitEnemy(enemyHealth, enemyHealth.transform.position);
            }
        }
        else
        {
            if (other.tag == "Player")
            {
                HitPlayer();
            }
            else if (other.tag == "OuterCharacter")
            {
                if (playerDashing != null && playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                    return;
                if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
                    return;

                outerHit = true;
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
            hitEnemies.Add(enemyHealth);

            EnemyControlManager enemyControl = enemyHealth.GetComponentInParent<EnemyControlManager>();
            enemyControl.GetHit();

            BlockingFeature enemyBlocking = enemyHealth.GetComponentInChildren<BlockingFeature>();
            if (enemyBlocking != null && enemyBlocking.isActiveAndEnabled && enemyBlocking.Blocking)
            {
                Disable();
                enemyBlocking.BlockHit(isPlayer, enemyBlocking.CharacterTransform.position - transform.position);
                GetComponentInParent<PlayerControlManager>().KnockBack(transform.position - enemyHealth.transform.position, isShort: true);
                anim.SetTrigger("recoil");
                return;
            }

            enemyControl.KnockBack(enemyHealth.transform.position - transform.position, isShort: false);
            enemyHealth.TakeDamage(damage, hitPoint);
            //Debug.Log("Enemy hit");
        }
    }

    private void HitPlayer()
    {
        if (!playerHealth.AlreadyHit && !playerHealth.IsDead)
        {
            if (playerDashing != null && playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                return;

            playerHealth.AlreadyHit = true;
            PlayerControlManager playerControl = playerHealth.GetComponentInParent<PlayerControlManager>();
            playerControl.GetHit();

            if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
            {
                Disable();
                anim.SetTrigger("recoil");
                GetComponentInParent<EnemyControlManager>().KnockBack(transform.position - playerHealth.transform.position, isShort: true);
                playerBlocking.BlockHit(isPlayer, playerBlocking.CharacterTransform.position - transform.position);
                return;
            }

            playerControl.KnockBack(playerHealth.transform.position - transform.position, isShort: false);
            playerHealth.TakeDamage(damage);
            GetComponentInParent<FitnessTracker>().DamagedPlayer(damage);
            inflictedDamage = true;
            //Debug.Log("Player hit");
        }
    }
}