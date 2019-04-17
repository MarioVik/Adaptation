using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBlocking : MonoBehaviour
{
    public bool BlockStart { get; private set; }
    public bool BlockStop { get; private set; }
    public bool Blocking { get; private set; }

    [SerializeField]
    Slider cooldownSlider;

    float coolDownTimer;
    float coolDown = 5f;

    float activeTimer = 0;
    float activeDuration = 5f;

    private void Awake()
    {
        coolDownTimer = coolDown;
        cooldownSlider.maxValue = coolDown;
        cooldownSlider.value = coolDown;
    }

    void Update()
    {
        if (BlockStart) BlockStart = false;
        if (BlockStop) BlockStop = false;

        if (!Blocking) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Input.GetButtonDown("FeatureInput") && coolDownTimer >= coolDown)
        {
            coolDownTimer = 0;
            Blocking = true;
            BlockStart = true;
            Debug.Log("Block Started");
        }

        if (Blocking)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration || Input.GetButtonUp("FeatureInput"))
            {
                Blocking = false;
                BlockStop = true;
                activeTimer = 0;
                Debug.Log("Block Stopped");
            }
        }

        cooldownSlider.value = coolDownTimer;
    }
}