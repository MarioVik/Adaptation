﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangedAttacking : MonoBehaviour
{
    [SerializeField]
    GameObject projectilePrefab;
    [SerializeField]
    Transform shootOrigin;

    public Transform ShootOrigin { get { return shootOrigin; } }

    public float Range { get; private set; } = 8f;
    int damage = 40;
    float projectileSpeed = 10f;
    float attackSpeed = 1.0f;

    int shootableMask;

    AudioSource weaponAudio;
    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;
    Animator anim;

    bool attacking, combo;

    public void IncreaseAttackDamage(int increase)
    {
        damage += increase;
    }

    public void IncreaseAttackSpeed(float increase)
    {
        projectileSpeed += increase;
        attackSpeed += (increase / 10);
    }

    public void IncreaseAttackRange(float increase)
    {
        Range += increase;
    }

    private void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        weaponAudio = GetComponent<AudioSource>();

        anim = GetComponentInParent<Animator>();
        normalClip = GetAnimationTime("NormalAttack01_SwordShield");
        comboClip = GetAnimationTime("NormalAttack02_SwordShield");
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
            if (combo && animationTimer >= animationDuration / 2)
            {
                Shoot();
                combo = false;
            }
            if (animationTimer >= animationDuration)
            {
                animationTimer = 0;
                attacking = false;
                anim.speed = 1.0f;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.GetComponent<ProjectileBehaviour>().Initialize(this, projectileSpeed, Range, damage);
        weaponAudio.Play();
    }

    public void Attack(bool combo = false)
    {
        this.combo = combo;
        attacking = true;

        float delay = 0.2f / attackSpeed;
        Invoke("Shoot", delay);

        anim.speed = attackSpeed;

        if (combo) animationDuration = comboClip.length;
        else animationDuration = normalClip.length;

        // Scaling to current speed
        animationDuration /= attackSpeed;
        // Cutting the duration time to 60% of the full clip length since clip includes some time margin
        animationDuration *= 0.6f;
    }
}
