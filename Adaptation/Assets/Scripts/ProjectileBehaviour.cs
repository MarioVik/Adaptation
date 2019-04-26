using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject effectPrefab;
    GameObject effectIntance;

    MonoBehaviour user;

    [SerializeField]
    AudioClip explosionAudio;
    [SerializeField]
    AudioClip sizzleAudio;

    float rotationSpeed = 100;
    float movementSpeed, range;
    int damage;

    Vector3 startPos;

    public void Initialize(EnemyRangedAttacking user, float speed, float range, int damage)
    {
        this.user = user;

        Vector3 originPosition = user.ShootOrigin.position;
        Quaternion originRotation = user.ShootOrigin.rotation;

        Initialize(originPosition, originRotation, speed, range, damage);
    }

    public void Initialize(PlayerRangedAttacking player, float speed, float range, int damage)
    {
        user = player;

        Vector3 originPosition = player.ShootOrigin.position;
        Quaternion originRotation = player.ShootOrigin.rotation;

        Initialize(originPosition, originRotation, speed, range, damage);
    }

    private void Initialize(Vector3 originPosition, Quaternion originRotation, float speed, float range, int damage)
    {
        transform.SetPositionAndRotation(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z), originRotation);
        transform.position += transform.forward;

        movementSpeed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;

        effectIntance = Instantiate(effectPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
    }

    void Update()
    {
        transform.position += transform.forward.normalized * movementSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Sizzle();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Environment")
        {
            Sizzle();
            return;
        }

        if (user is PlayerRangedAttacking)
        {
            if (collision.collider.tag == "Enemy")
            {
                EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damage, enemyHealth.transform.position);

                Explode(collision);

                //Debug.Log("Enemy hit");
            }
        }
        else if (user is EnemyRangedAttacking)
        {
            if (collision.collider.tag == "Player")
            {
                PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
                playerHealth.TakeDamage(damage);

                Explode(collision);

                Debug.Log("Player hit");
            }
        }
    }

    void Sizzle()
    {
        effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Sizzle(sizzleAudio);

        Destroy(effectIntance.gameObject, 1.0f);
        Destroy(gameObject);
    }

    void Explode(Collision collision)
    {
        effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Explode(collision, explosionAudio);

        Destroy(effectIntance.gameObject, 1.0f);
        Destroy(gameObject);
    }
}