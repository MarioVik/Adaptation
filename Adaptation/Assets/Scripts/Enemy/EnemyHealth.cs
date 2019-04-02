using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public bool AlreadyHit { get; set; }

    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public AudioClip deathClip;

    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    bool isDead;
    bool isSinking;

    bool blockEnabled;

    public void IncreaseHealth(int increase)
    {
        startingHealth += increase;
        currentHealth = startingHealth;
    }

    void Awake()
    {
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
        if (isDead)
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
        isDead = true;

        capsuleCollider.isTrigger = true;

        anim.SetTrigger("Dead");

        enemyAudio.clip = deathClip;
        enemyAudio.Play();

        GetComponent<EnemyTraits>().CalculateFitnessScore();
    }

    public void StartSinking()
    {
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        isSinking = true;
        IndividualsTextManager.killedIndividuals += 1;
        Destroy(gameObject, 2f);
    }

    public void EnableBlock()
    {
        blockEnabled = false;
    }

    private void Block()
    {
        if (blockEnabled)
        {
            //Disable/reduce damage taken for certain duration
        }
    }
}
