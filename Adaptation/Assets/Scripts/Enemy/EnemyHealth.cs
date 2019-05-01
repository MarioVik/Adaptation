using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public bool AlreadyHit { get; set; }
    public bool Dashing { private get; set; }
    public bool Blocking { private get; set; }

    TargetingHandler targeting;

    public int startingHealth = 80;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public AudioClip deathClip;

    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    public bool IsDead { get; set; }
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
        if (IsDead || Dashing || Blocking)
            return;

        enemyAudio.Play();
        anim.SetTrigger("hit");

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
        IsDead = true;
        targeting.UpdateEnemies(calledByEnemy: true);

        capsuleCollider.isTrigger = true;

        anim.SetTrigger("die");
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