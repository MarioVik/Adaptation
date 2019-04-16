using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    MonoBehaviour user;

    float speed, range;
    int damage;

    Vector3 startPos;

    public void Initialize(EnemyRangedAttacking user, Transform originTransform, float speed, float range, int damage)
    {
        this.user = user;
        transform.SetPositionAndRotation(originTransform.position, originTransform.rotation);
        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;
    }

    public void Initialize(PlayerRangedAttacking player, float speed, float range, int damage)
    {
        user = player;
        transform.SetPositionAndRotation(player.ShootOrigin.position, player.ShootOrigin.rotation);
        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;
    }

    public void Initialize(PlayerRangedAttacking player, Transform target, float speed, float range, int damage)
    {
        user = player;

        Vector3 direction = target.position - player.ShootOrigin.position;
        direction.Normalize();
        transform.SetPositionAndRotation(player.ShootOrigin.position, Quaternion.LookRotation(direction));

        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;
    }

    void Update()
    {
        transform.position += transform.forward.normalized * speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (user is PlayerRangedAttacking)
        {
            if (other.tag != "Player" && other.tag != "GameController")
            {
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
                    if (enemyHealth.IsDead)
                    {
                        (user as PlayerRangedAttacking).UpdateEnemies();
                    }
                }
                Destroy(gameObject);
                Debug.Log("Enemy hit");
            }
        }
        else if (user is EnemyRangedAttacking)
        {
            if (other.tag != "Enemy")
            {

            }
        }
    }
}