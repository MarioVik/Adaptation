﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttacking : MonoBehaviour
{
    Collider weaponcollider;

    int damagePerAttack = 40;
    float timeBetweenAttacks = 0.8f;

    float attackTimer;
    int shootableMask;
    AudioSource weaponAudio;
    float animationDuration, animationTimer;
    Animator anim;

    List<EnemyHealth> hitEnemies;

    public void IncreaseAttackDamage(int increase)
    {
        damagePerAttack += increase;
    }

    public void IncreaseAttackSpeed(float increase)
    {
        //Scale animation time                                  <<<<---- TODO        
    }

    public void IncreaseAttackRate(float increase)
    {
        timeBetweenAttacks -= increase;
    }

    public void IncreaseAttackRange(float increase)
    {
        weaponcollider.transform.localScale = new Vector3(
            weaponcollider.transform.localScale.x,
            weaponcollider.transform.localScale.y + increase,
            weaponcollider.transform.localScale.z);
    }

    private void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        weaponAudio = GetComponent<AudioSource>();
        anim = GetComponentInParent<Animator>();
        weaponcollider = GetComponent<CapsuleCollider>();
        weaponcollider.enabled = false;
        attackTimer = timeBetweenAttacks;
        animationTimer = 0;
        animationDuration = GetAnimationTime();
        hitEnemies = new List<EnemyHealth>();
    }

    public float GetAnimationTime()
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
            if (clip.name == "NormalAttack01_SwordShield")
                return clip.length;
        throw new System.ArgumentNullException("No matching animation clip found for attack");
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= timeBetweenAttacks && Time.timeScale != 0)
        {
            if (Input.GetButton("Attack"))
                Attack();
        }
        else
        {
            animationTimer += Time.deltaTime;
            if (animationTimer >= animationDuration)
            {
                animationTimer = 0;
                weaponcollider.enabled = false;
                anim.SetBool("IsAttacking", false);
                foreach (EnemyHealth tempEnemy in hitEnemies)
                    tempEnemy.AlreadyHit = false;
            }
        }
    }

    void Attack()
    {
        attackTimer = 0f;
        weaponcollider.enabled = true;
        weaponAudio.Play();
        anim.SetBool("IsAttacking", true);
    }

    public void HitEnemy(EnemyHealth enemyHealth, Vector3 hitPoint)
    {
        if (!enemyHealth.AlreadyHit)
        {
            enemyHealth.TakeDamage(damagePerAttack, hitPoint);
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