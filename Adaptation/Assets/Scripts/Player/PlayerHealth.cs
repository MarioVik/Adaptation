using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public bool AlreadyHit { get; set; }

    public int startingHealth = 80;
    public int currentHealth;
    public Slider healthSlider;
    public Image damageImage;
    public AudioClip deathClip;
    public float flashSpeed = 5f;
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);

    [SerializeField]
    BlockingFeature playerBlocking;

    Animator anim;
    AudioSource playerAudio;
    PlayerControlManager controlManager;
    bool isDead;
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
            anim.SetTrigger("hit");
            if (currentHealth <= 0 && !isDead)
            {
                Death();
            }
        }
    }

    void Death()
    {
        isDead = true;

        anim.SetTrigger("die");

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