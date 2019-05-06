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
    public AudioClip deathClip;
    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);

    int startingHealth = 200;
    int currentHealth;

    [SerializeField]
    BlockingFeature playerBlocking;

    Animator anim;
    AudioSource playerAudio;
    PlayerControlManager controlManager;
    bool damaged;

    public void IncreaseHealth(int increase)
    {
        startingHealth += increase;
        currentHealth = startingHealth;
    }

    public void ResetHealth()
    {
        currentHealth = startingHealth;
        healthSlider.maxValue = currentHealth;
        healthSlider.value = currentHealth;
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        controlManager = GetComponentInParent<PlayerControlManager>();
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

    public void TakeDamage(int amount)
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

        controlManager.Dead = true;

        Invoke("RestartLevel", 3.0f);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(1);
    }
}