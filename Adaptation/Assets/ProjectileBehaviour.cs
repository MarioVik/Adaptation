using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject effectPrefab;
    GameObject effectIntance;

    MonoBehaviour user;

    AudioSource explosionAudio;

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

        explosionAudio = GetComponent<AudioSource>();
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

        explosionAudio = GetComponent<AudioSource>();
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

        effectIntance = Instantiate(effectPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        explosionAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.position += transform.forward.normalized * speed * Time.deltaTime;

        //effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        //Rigidbody rigid = effectIntance.GetComponentInChildren<Rigidbody>();
        //rigid.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Destroy(effectIntance.gameObject, 1.0f);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Effect")
            return;

        if (user is PlayerRangedAttacking)
        {
            if (collision.collider.tag != "Player" && collision.collider.tag != "GameController")
            {
                EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
                }

                effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Detonate(collision);
                //explosionAudio.Play();

                Destroy(effectIntance.gameObject, 1.0f);

                Destroy(gameObject);

                Debug.Log("Enemy hit");
            }
        }
        else if (user is EnemyRangedAttacking)
        {
            if (collision.collider.tag != "Enemy")
            {

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "Effect")
        //    return;

        //if (user is PlayerRangedAttacking)
        //{
        //    if (other.tag != "Player" && other.tag != "GameController")
        //    {
        //        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        //        if (enemyHealth != null)
        //        {
        //            enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
        //        }
        //        effectIntance.GetComponent<RFX4_PhysicsMotion>().Detonate(other);
        //        Destroy(effectIntance.gameObject, 1.0f);
        //        Destroy(gameObject);
        //        Debug.Log("Enemy hit");
        //    }
        //}
        //else if (user is EnemyRangedAttacking)
        //{
        //    if (other.tag != "Enemy")
        //    {

        //    }
        //}
    }
}