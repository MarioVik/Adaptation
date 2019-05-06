using DM;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    Transform characterTransform;

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

    [SerializeField]
    Slider sliderPreafab;
    Slider healthSlider;
    Image fillArea;
    float uiOffset = 0.05f/*-1.6f*/;
    float uiScale = 0.005f;

    public void IncreaseHealth(int increase)
    {
        startingHealth += increase;
        currentHealth = startingHealth;
        healthSlider.maxValue = currentHealth;
        healthSlider.value = currentHealth;
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

        healthSlider = Instantiate(sliderPreafab, GetSliderPosition(), characterTransform.rotation);
        healthSlider.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        healthSlider.maxValue = currentHealth;
        healthSlider.value = currentHealth;
        //healthSlider.GetComponentsInChildren<Image>()[0].color = new Color(0, 0, 0, 1.0f);
        healthSlider.GetComponentsInChildren<Image>()[1].color = Color.red;
        healthSlider.transform.localScale *= uiScale;
    }

    Vector3 GetSliderPosition()
    {
        Vector3 worldPoint = new Vector3(characterTransform.position.x, characterTransform.position.y - uiOffset, characterTransform.position.z);
        return Camera.main.WorldToScreenPoint(worldPoint);
    }

    void Update()
    {
        if (isSinking)
        {
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }

        if (!IsDead)
            UpdateUI();
    }

    void UpdateUI()
    {
        Transform cameraTransform = CameraManager.singleton.camTrans;
        Plane plane = new Plane(cameraTransform.forward, cameraTransform.position);

        healthSlider.transform.rotation = Quaternion.LookRotation(plane.normal, Vector3.up);
        healthSlider.transform.position = new Vector3(characterTransform.position.x, characterTransform.position.y - uiOffset, characterTransform.position.z);

        Vector3 dirToCamera = (cameraTransform.position - characterTransform.position).normalized;
        healthSlider.transform.position += dirToCamera * 4;
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (IsDead)
            return;

        enemyAudio.Play();

        currentHealth -= amount;

        healthSlider.value = currentHealth;

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

        Destroy(healthSlider.gameObject);

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