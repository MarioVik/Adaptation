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
    float damage;

    Vector3 startPos;

    //Only if user is player
    OuterProjectileBehaviour outerBehaviour;

    // Only if user is enemy
    bool inflictedDamage, outerHit;
    PlayerHealth playerHealth;
    DashingFeature playerDashing;
    BlockingFeature playerBlocking;

    public void Initialize(RangedAttackFeature user, float speed, float range, float damage, Vector3 direction = new Vector3())
    {
        this.user = user;
        this.speed = speed;
        this.range = range;
        this.damage = damage;

        Vector3 originPosition = user.ShootOrigin.position;
        Quaternion originRotation;

        if (direction == new Vector3())
            originRotation = user.ShootOrigin.rotation;
        else
            originRotation = Quaternion.Euler(direction.x, 0, direction.z);

        InitializeEffect(originPosition, originRotation);

        if (!user.IsPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player.GetComponent<PlayerHealth>();
            playerDashing = player.GetComponent<DashingFeature>();
            playerBlocking = player.GetComponentInChildren<BlockingFeature>();
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
        if (user == null)
            Destroy(gameObject);

        transform.position += transform.forward.normalized * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Sizzle();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Environment")
        {
            Sizzle();
            return;
        }

        if (ByPlayer)
        {
            //if (collision.collider.tag == "Projectile" &&
            //    !collision.collider.GetComponent<ProjectileBehaviour>().ByPlayer)
            //{
            //    outerBehaviour.Clear();
            //    Explode(collision);
            //    return;
            //}

            EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
            if (collision.collider.tag == "Enemy" && !enemyHealth.IsDead)
            {
                outerBehaviour.Clear();
                Explode(collision);

                DashingFeature enemyDashing = collision.collider.GetComponent<DashingFeature>();
                if (enemyDashing != null && enemyDashing.isActiveAndEnabled && enemyDashing.Dashing)
                    return;

                enemyHealth.GetComponentInParent<EnemyControlManager>().GetHit();

                BlockingFeature enemyBlocking = collision.collider.GetComponentInChildren<BlockingFeature>();
                if (enemyBlocking != null && enemyBlocking.isActiveAndEnabled && enemyBlocking.Blocking)
                {
                    enemyBlocking.Reflect(transform.forward);
                    return;
                }

                enemyHealth.TakeDamage(damage, enemyHealth.transform.position);

                //Debug.Log("Enemy hit");
            }
        }
        else
        {
            //if (collision.collider.tag == "Projectile" &&
            //    collision.collider.GetComponent<ProjectileBehaviour>().ByPlayer)
            //{
            //    Explode(collision);
            //    return;
            //}

            if (collision.collider.tag == "Enemy")
            {
                Sizzle();
            }
            else if (collision.collider.tag == "Player" && !playerHealth.IsDead)
            {
                if (playerDashing != null && playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                {
                    Sizzle();
                    return;
                }

                playerHealth.GetComponentInParent<PlayerControlManager>().GetHit();

                if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
                {
                    Sizzle();
                    playerBlocking.Reflect(transform.forward);
                    return;
                }

                Explode(collision);
                playerHealth.TakeDamage(damage);
                user.GetComponentInParent<FitnessTracker>().DamagedPlayer(damage);
                inflictedDamage = true;

                //Debug.Log("Player hit");
            }
            else if (collision.collider.tag == "OuterCharacter")
            {
                outerHit = true;
            }
        }
    }

    public void Sizzle()
    {
        if (ByPlayer)
        {
            outerBehaviour.Clear();
        }
        else
        {
            if (!inflictedDamage && outerHit)
            {
                // If the player is not blocking AND not dashing, register the "almost" damage
                if (playerDashing != null && playerDashing.isActiveAndEnabled && playerDashing.Dashing)
                { }
                else if (playerBlocking != null && playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
                { }
                else
                {
                    user.GetComponentInParent<FitnessTracker>().AlmostDamagedPlayer(damage);
                }
            }
        }

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