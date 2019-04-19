using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttacking : MonoBehaviour
{
    Collider weaponcollider;

    int damage = 40;
    float speed = 1.0f;

    AudioSource weaponAudio;
    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;
    Animator anim;

    List<EnemyHealth> hitEnemies;

    bool attacking, combo;

    public void IncreaseAttackDamage(int increase)
    {
        damage += increase;
    }

    public void IncreaseAttackSpeed(float increase)
    {
        speed += increase;
    }

    //public void IncreaseAttackRate(float increase)
    //{
    //    timeBetweenAttacks -= increase;
    //}

    public void IncreaseAttackRange(float increase)
    {
        weaponcollider.transform.localScale = new Vector3(
            weaponcollider.transform.localScale.x,
            weaponcollider.transform.localScale.y + increase,
            weaponcollider.transform.localScale.z);
    }

    private void Awake()
    {
        weaponAudio = GetComponent<AudioSource>();

        weaponcollider = GetComponent<CapsuleCollider>();
        weaponcollider.enabled = false;

        anim = GetComponentInParent<Animator>();

        normalClip = GetAnimationTime("NormalAttack01_SwordShield");
        comboClip = GetAnimationTime("NormalAttack02_SwordShield");

        hitEnemies = new List<EnemyHealth>();
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
        if (attacking)
        {
            animationTimer += Time.deltaTime;
            if (combo && animationTimer >= (animationDuration * 0.4f))
            {
                foreach (EnemyHealth tempEnemy in hitEnemies)
                    tempEnemy.AlreadyHit = false;
                combo = false;
                weaponAudio.Play();

                Debug.Log("Collider hits reset");
            }
            if (animationTimer >= animationDuration)
            {
                animationTimer = 0;
                attacking = false;

                anim.speed = 1.0f;

                weaponcollider.enabled = false;

                foreach (EnemyHealth tempEnemy in hitEnemies)
                    tempEnemy.AlreadyHit = false;

                Debug.Log("Attack Disabled");
            }
        }
    }

    public void Attack(bool combo = false)
    {
        this.combo = combo;
        attacking = true;

        weaponcollider.enabled = true;

        weaponAudio.Play();

        anim.speed = speed;

        if (combo) animationDuration = comboClip.length;
        else animationDuration = normalClip.length;

        // Scaling to current speed
        animationDuration /= speed;
        // Cutting the duration time to 60% of the full clip length since clip includes some time margin
        animationDuration *= 0.6f;

        Debug.Log("Attack Enabled");
    }

    public void HitEnemy(EnemyHealth enemyHealth, Vector3 hitPoint)
    {
        if (!enemyHealth.AlreadyHit)
        {
            enemyHealth.TakeDamage(damage, hitPoint);
            hitEnemies.Add(enemyHealth);
        }
        enemyHealth.AlreadyHit = true;
        Debug.Log("Enemy hit");
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            HitEnemy(enemyHealth, enemyHealth.transform.position);
        }
    }
}