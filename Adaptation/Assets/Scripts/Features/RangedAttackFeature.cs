using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttackFeature : MonoBehaviour
{
    public bool Attacking { get; private set; }

    public bool IsPlayer { get { return isPlayer; } }

    [SerializeField]
    bool isPlayer;

    [SerializeField]
    GameObject projectilePrefab;
    [SerializeField]
    Transform shootOrigin;

    public Transform ShootOrigin { get { return shootOrigin; } }

    public float Range { get; private set; } = 8f;
    int damage = 40;
    float projectileSpeed = 10f;
    float attackSpeed, baseAttackSpeed = 1.0f;
    bool combo;

    int shootableMask;

    AudioSource weaponAudio;
    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;
    Animator anim;

    public void IncreaseAttackDamage(int increase) => damage += increase;

    public void IncreaseAttackSpeed(float increase) => attackSpeed += increase;

    public void IncreaseAttackRange(float increase) => Range += increase;

    public void Activate(bool combo = false)
    {
        this.combo = combo;
        Attacking = true;

        anim.speed = attackSpeed;

        if (combo) animationDuration = comboClip.length;
        else animationDuration = normalClip.length;

        // Scaling to current speed
        animationDuration /= attackSpeed;

        float delay = animationDuration * 0.3f;
        Invoke("Shoot", delay);

        anim.SetBool("attacking", Attacking);
        anim.SetBool("combo", combo);
    }

    public void Disable()
    {
        animationTimer = 0;
        Attacking = false;
        combo = false;
        anim.speed = baseAttackSpeed;

        anim.SetBool("attacking", Attacking);
        anim.SetBool("combo", combo);
    }

    private void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        weaponAudio = GetComponent<AudioSource>();

        anim = GetComponentInParent<Animator>();

        baseAttackSpeed = anim.speed;
        attackSpeed = baseAttackSpeed;

        normalClip = GetAnimationTime("NormalAttack01_SwordShield");
        comboClip = GetAnimationTime("NormalAttack02_SwordShield");
    }

    private AnimationClip GetAnimationTime(string name)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
            if (clip.name == name)
                return clip;
        throw new System.ArgumentNullException("No matching animation clip found for attack");
    }

    private void Update()
    {
        if (Attacking)
        {
            animationTimer += Time.deltaTime;
            if (combo && animationTimer >= (animationDuration * 0.45f))
            {
                combo = false;
                Shoot();
            }
            if (animationTimer >= animationDuration)
            {
                Disable();
            }
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.GetComponent<ProjectileBehaviour>().Initialize(this, projectileSpeed, Range, damage);
        weaponAudio.Play();
    }
}