using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockingFeature : MonoBehaviour
{
    [Header("Only if user is player")]
    [SerializeField]
    bool isPlayer;
    [SerializeField]
    Slider cooldownSlider;

    public bool Ready { get { return coolDownTimer >= coolDown && !Blocking; } }
    public bool BlockStop { get; private set; }
    public bool Blocking { get; private set; }

    // Only if user is enemy
    EnemyHealth enemyHealth;
    //

    float coolDownTimer;
    float coolDown = 5f;

    float activeTimer = 0;
    float activeDuration = 5f;

    public void Activate()
    {
        coolDownTimer = 0;
        Blocking = true;
        Debug.Log("Block Started");
    }

    public void Deactivate()
    {
        Blocking = false;
        BlockStop = true;
        activeTimer = 0;
        Debug.Log("Block Stopped");
    }

    private void Awake()
    {
        coolDownTimer = coolDown;
        if (isPlayer)
        {
            cooldownSlider.maxValue = coolDown;
            cooldownSlider.value = coolDown;
        }
        else
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }
    }

    void Update()
    {
        if (BlockStop) BlockStop = false;

        if (!Blocking) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Blocking)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration || Input.GetButtonUp("FeatureInput"))
            {
                Deactivate();
            }
        }

        if (isPlayer)
        {
            cooldownSlider.value = coolDownTimer;
        }
        else
        {
            enemyHealth.Blocking = Blocking;
        }
    }
}