using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttacking : MonoBehaviour
{
    [SerializeField]
    Collider weaponcollider;

    public int damagePerAttack = 20;
    public float timeBetweenAttacks = 0.3f;
    public float range = 100f;

    float timer;
    int shootableMask;
    AudioSource weaponAudio;
    float effectsDisplayTime = 0.2f;

    Animator anim;


    void Awake()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        weaponAudio = GetComponent<AudioSource>();
        anim = GetComponentInParent<Animator>();
        weaponcollider.enabled = false;
    }


    void Update()
    {
        timer += Time.deltaTime;

        if (Input.GetButton("Fire1") && timer >= timeBetweenAttacks && Time.timeScale != 0)
        {
            Attack();
        }
        else if (anim.GetBool("IsAttacking"))
        {
            anim.SetBool("IsAttacking", false);
        }
    }

    void Attack()
    {
        timer = 0f;

        weaponcollider.enabled = true;
        //weaponAudio.Play();
        anim.SetBool("IsAttacking", true);

        //if (Enemy in collider)
        //{
        //    if (enemyHealth != null)
        //    {
        //        enemyHealth.TakeDamage(damagePerShot, shootHit.point);
        //    }
        //    gunLine.SetPosition(1, shootHit.point);
        //}
        //else
        //{
        //    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        //}
    }

    public void HitEnemy(EnemyHealth enemyHealth)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            enemyHealth.TakeDamage(damagePerAttack, hit.point);
            Debug.Log("Enemy hit");
        }
    }

    //public void HitEnemy()
    //{
    //    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
    //}
}
