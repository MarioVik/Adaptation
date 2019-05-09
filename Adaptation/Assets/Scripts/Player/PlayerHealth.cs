using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public bool AlreadyHit { get; set; }

    public Slider healthSlider;
    public Image damageImage;
    [SerializeField]
    AudioClip deathClip;
    [SerializeField]
    AudioClip hurtClip;
    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);

    public float StartingHealth { get; private set; } = 1000;
    float currentHealth;

    Vector3 startPos;

    [SerializeField]
    BlockingFeature playerBlocking;

    Animator anim;
    AudioSource playerAudio;
    PlayerControlManager controlManager;
    bool damaged;

    public void IncreaseHealth()
    {
        StartingHealth += (StartingHealth * 0.1f);
        currentHealth = StartingHealth;
    }

    public void ResetHealth()
    {
        currentHealth = StartingHealth;
        healthSlider.maxValue = currentHealth;
        healthSlider.value = currentHealth;
        IsDead = false;
        anim.SetBool("dead", false);
        playerAudio.clip = hurtClip;

        transform.parent.SetPositionAndRotation(startPos, transform.rotation);
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        controlManager = GetComponentInParent<PlayerControlManager>();
        startPos = transform.parent.position;

        ResetHealth();
    }

    void Update()
    {
        if (damaged)
        {
            damageImage.color = flashColour;
        }
        else
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        damaged = false;
    }

    public void TakeDamage(float amount)
    {
        if (playerBlocking == null || !playerBlocking.Blocking)
        {
            damaged = true;
            currentHealth -= amount;
            healthSlider.value = currentHealth;
            playerAudio.Play();
            if (currentHealth <= 0 && !IsDead)
            {
                Death();
            }
        }
    }

    void Death()
    {
        anim.SetBool("dead", true);

        IsDead = true;

        playerAudio.clip = deathClip;
        playerAudio.Play();

        GenerationManager.PlayerReady = false;
    }
}