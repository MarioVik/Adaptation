using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRangedAttacking : MonoBehaviour
{
    [SerializeField]
    GameObject projectilePrefab;
    [SerializeField]
    GameObject targetPointer;
    public Transform ShootOrigin { get; set; }

    int damage = 40;
    float range = 8f;
    float projectileSpeed = 20f;
    float attackSpeed = 1.0f;

    int shootableMask;

    AudioSource weaponAudio;
    float animationDuration;
    AnimationClip normalClip, comboClip;
    float animationTimer = 0;
    Animator anim;

    bool attacking, combo;

    List<EnemyHealth> enemies;
    int targetIndex = -1;

    public void IncreaseAttackDamage(int increase)
    {
        damage += increase;
    }

    public void IncreaseAttackSpeed(float increase)
    {
        projectileSpeed += increase;
        attackSpeed += (increase / 10);
    }

    //public void IncreaseAttackRate(float increase)
    //{
    //    timeBetweenAttacks -= increase;
    //}

    public void IncreaseAttackRange(float increase)
    {
        range += increase;
    }

    private void Awake()
    {
        GameObject empyGo = new GameObject();
        ShootOrigin = empyGo.transform;

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
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (targetIndex == -1)
                {
                    targetPointer.SetActive(true);
                    UpdateEnemies();
                    TargetClosest();
                }
                else
                {
                    targetIndex++;
                }

            }
        }
        if (targetIndex != -1)
        {
            UpdateTargetPointer();
        }
    }

    private void UpdateTargetPointer()
    {
        Vector3 position = enemies[targetIndex].transform.position;
        position.y += 2;
        Quaternion rotation = enemies[targetIndex].transform.rotation;
        targetPointer.transform.SetPositionAndRotation(position, rotation);
        //targetPointer.transform.position.y += 2;
    }

    private void TargetClosest()
    {
        float closestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (closestDistance > Vector3.Distance(ShootOrigin.position, enemies[i].transform.position))
            {
                closestDistance = Vector3.Distance(ShootOrigin.position, enemies[i].transform.position);
                targetIndex = i;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        if (targetIndex == -1)
        {
            projectile.GetComponent<ProjectileBehaviour>().Initialize(this, projectileSpeed, range, damage);
        }
        else
        {
            projectile.GetComponent<ProjectileBehaviour>().Initialize(this, enemies[targetIndex].transform, projectileSpeed, range, damage);
        }
        weaponAudio.Play();
    }

    public void Attack(bool combo = false)
    {
        this.combo = combo;
        attacking = true;

        Shoot();

        anim.speed = attackSpeed;

        if (combo) animationDuration = comboClip.length;
        else animationDuration = normalClip.length;

        // Scaling to current speed
        animationDuration /= attackSpeed;
        // Cutting the duration time to 60% of the full clip length since clip includes some time margin
        animationDuration *= 0.6f;
    }

    public void UpdateEnemies()
    {
        enemies = new List<EnemyHealth>();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemyObjects)
        {
            enemies.Add(enemyObj.GetComponent<EnemyHealth>());
        }
        targetIndex = -1;
        //targetPointer.SetActive(false);
    }
}