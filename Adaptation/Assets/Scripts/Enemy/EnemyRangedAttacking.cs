using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangedAttacking : MonoBehaviour
{
    bool activated;

    [SerializeField]
    GameObject projectilePrefab;
    [SerializeField]
    Transform shootOrigin;

    int damagePerAttack = 40;
    float timeBetweenAttacks = 0.8f;
    float range = 8f;
    float speed = 20f;

    float attackTimer;
    int shootableMask;
    AudioSource weaponAudio;
    float animationDuration, animationTimer;
    Animator anim;

    public void IncreaseAttackDamage(int increase)
    {
        damagePerAttack += increase;
    }

    public void IncreaseAttackSpeed(float increase)
    {
        //Scale animation time                                  <<<<---- TODO
        speed += increase;
    }

    public void IncreaseAttackRate(float increase)
    {
        timeBetweenAttacks -= increase;
    }

    public void IncreaseAttackRange(float increase)
    {
        range += increase;
    }

    private void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        weaponAudio = GetComponent<AudioSource>();
        anim = GetComponentInParent<Animator>();
        attackTimer = timeBetweenAttacks;
        animationTimer = 0;
        animationDuration = GetAnimationTime();
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
            if (activated)
                Attack();
        }
        else
        {
            animationTimer += Time.deltaTime;
            if (animationTimer >= animationDuration)
            {
                animationTimer = 0;
                anim.SetBool("IsAttacking", false);
            }
        }
    }

    void Attack()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.GetComponent<ProjectileBehaviour>().Initialize(shootOrigin, speed, range, damagePerAttack);

        attackTimer = 0f;
        weaponAudio.Play();
        anim.SetBool("IsAttacking", true);
    }
}
