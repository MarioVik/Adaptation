using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public bool ByPlayer { get { return user.IsPlayer; } }

    [SerializeField]
    GameObject effectPrefab;
    GameObject effectIntance;

    RangedAttackFeature user;

    [SerializeField]
    AudioClip explosionAudio;
    [SerializeField]
    AudioClip sizzleAudio;

    float rotationSpeed = 100;
    float speed, range;
    int damage;

    Vector3 startPos;

    //Only if user is player
    OuterProjectileBehaviour outerBehaviour;

    // Only if user is enemy
    PlayerHealth playerHealth;
    DashingFeature playerDashing;
    BlockingFeature playerBlocking;

    public void Initialize(RangedAttackFeature user, float speed, float range, int damage)
    {
        this.user = user;
        this.speed = speed;
        this.range = range;
        this.damage = damage;

        Vector3 originPosition = user.ShootOrigin.position;
        Quaternion originRotation = user.ShootOrigin.rotation;
        InitializeEffect(originPosition, originRotation);

        if (!user.IsPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player.GetComponent<PlayerHealth>();
            playerDashing = player.GetComponent<DashingFeature>();
            playerBlocking = player.GetComponentInChildren<BlockingFeature>();

            GetComponentInChildren<OuterProjectileBehaviour>().gameObject.SetActive(false);
        }
        else
        {
            outerBehaviour = GetComponentInChildren<OuterProjectileBehaviour>();
        }
    }

    private void InitializeEffect(Vector3 originPosition, Quaternion originRotation)
    {
        transform.SetPositionAndRotation(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z), originRotation);
        transform.position += transform.forward;

        startPos = transform.position;

        effectIntance = Instantiate(effectPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
    }

    private void Update()
    {
        transform.position += transform.forward.normalized * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            if (ByPlayer)
                outerBehaviour.Clear();
            Sizzle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Environment")
        {
            if (ByPlayer)
                outerBehaviour.Clear();
            Sizzle();
            return;
        }

        if (ByPlayer)
        {
            ProjectileBehaviour otherBehaviour = collision.collider.GetComponent<ProjectileBehaviour>();

            if (collision.collider.tag == "Projectile" &&
                !otherBehaviour.ByPlayer)
            {
                outerBehaviour.Clear();
                Explode(collision);
                return;
            }

            if (collision.collider.tag == "Enemy")
            {
                EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
                enemyHealth.GetComponentInParent<EnemyControlManager>().GetHit();

                outerBehaviour.Clear();
                Explode(collision);

                //Debug.Log("Enemy hit");
            }
        }
        else
        {
            if (collision.collider.tag == "Projectile" &&
                collision.collider.GetComponent<ProjectileBehaviour>().ByPlayer)
            {
                Explode(collision);
                return;
            }

            if (collision.collider.tag == "Player")
            {
                Explode(collision);

                if (playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                    return;

                if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
                    return;

                if (playerHealth.currentHealth > 0)
                {
                    playerHealth.TakeDamage(damage);
                    user.GetComponentInParent<EnemyTraits>().DamagedPlayer(damage);
                    playerHealth.GetComponentInParent<PlayerControlManager>().GetHit();
                }

                Debug.Log("Player hit");
            }
        }
    }

    private void Sizzle()
    {
        effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Sizzle(sizzleAudio);

        Destroy(effectIntance.gameObject, 1.0f);
        Destroy(gameObject);
    }

    private void Explode(Collision collision)
    {
        effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Explode(collision, explosionAudio);

        Destroy(effectIntance.gameObject, 1.0f);
        Destroy(gameObject);
    }
}