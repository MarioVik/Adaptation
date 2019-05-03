using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public bool AlreadyHit { get; set; }

    TargetingHandler targeting;

    public int startingHealth = 80;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public AudioClip deathClip;

    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    bool isSinking;

    public void IncreaseHealth(int increase)
    {
        startingHealth += increase;
        currentHealth = startingHealth;
    }

    void Awake()
    {
        targeting = GameObject.FindGameObjectWithTag("PlayerController").GetComponent<TargetingHandler>();

        anim = GetComponent<Animator>();
        enemyAudio = GetComponent<AudioSource>();
        hitParticles = GetComponentInChildren<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        currentHealth = startingHealth;
        AlreadyHit = false;
    }

    void Update()
    {
        if (isSinking)
        {
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (IsDead)
            return;

        enemyAudio.Play();

        currentHealth -= amount;

        hitParticles.transform.position = hitPoint;
        hitParticles.Play();

        if (currentHealth <= 0)
        {
            Death();
        }
    }


    void Death()
    {
        anim.SetBool("dead", true);

        IsDead = true;
        targeting.UpdateEnemies(calledByEnemy: true);

        capsuleCollider.isTrigger = true;

        enemyAudio.clip = deathClip;
        enemyAudio.Play();

        GetComponent<EnemyTraits>().CalculateFitnessScore();

        Invoke("StartSinking", 5.0f);
    }

    public void StartSinking()
    {
        isSinking = true;
        IndividualsTextManager.killedIndividuals += 1;

        Rigidbody rigid = GetComponentInParent<Rigidbody>();
        rigid.isKinematic = true;
        rigid.constraints = RigidbodyConstraints.None;
        Destroy(rigid.gameObject, 2f);
    }
}