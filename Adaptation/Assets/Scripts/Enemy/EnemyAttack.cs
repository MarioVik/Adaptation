using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    PlayerBlocking playerBlocking;

    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;

    Animator anim;
    GameObject player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;

    public void IncreaseAttackSpeed(float increase)
    {
        timeBetweenAttacks -= increase;
    }

    public void IncreaseAttackDamage(int increase)
    {
        attackDamage += increase;
    }

    public void IncreaseAttackRange(float increase)
    {
        transform.localScale = new Vector3(transform.localScale.x + increase, transform.localScale.y, transform.localScale.z + increase);
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
        {
            Attack();
        }

        if (playerHealth.currentHealth <= 0)
        {
            anim.SetTrigger("PlayerDead");
        }
    }

    void Attack()
    {
        timer = 0f;

        if (playerBlocking.isActiveAndEnabled && playerBlocking.Blocking)
            return;

        if (playerHealth.currentHealth > 0)
        {
            playerHealth.TakeDamage(attackDamage);
            GetComponent<EnemyTraits>().DamagedPlayer(attackDamage);
        }
    }
}