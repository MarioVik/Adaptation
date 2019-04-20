using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject effectPrefab;

    MonoBehaviour user;

    float speed, range;
    int damage;

    Vector3 startPos;

    public void Initialize(EnemyRangedAttacking user, Transform originTransform, float speed, float range, int damage)
    {
        this.user = user;

        transform.SetPositionAndRotation(new Vector3(originTransform.position.x, originTransform.position.y + 1, originTransform.position.z), originTransform.rotation);
        transform.position += transform.forward;

        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;
    }

    public void Initialize(PlayerRangedAttacking player, float speed, float range, int damage)
    {
        user = player;

        transform.SetPositionAndRotation(new Vector3(player.ShootOrigin.position.x, player.ShootOrigin.position.y + 1, player.ShootOrigin.position.z), player.ShootOrigin.rotation);
        transform.position += transform.forward;

        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;
    }

    public void Initialize(PlayerRangedAttacking player, Vector3 position, Quaternion rotation, float speed, float range, int damage)
    {
        user = player;

        transform.SetPositionAndRotation(new Vector3(position.x, position.y + 1, position.z), rotation);
        transform.position += transform.forward;

        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;

        Instantiate(effectPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
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