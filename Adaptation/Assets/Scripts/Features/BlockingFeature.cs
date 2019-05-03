using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockingFeature : MonoBehaviour
{
    [SerializeField]
    Transform characterTransform;

    [Header("Only if user is player")]
    [SerializeField]
    bool isPlayer;

    [SerializeField]
    Slider sliderPreafab;
    Slider cooldownSlider;
    Image fillArea;
    float uiOffset = 0.5f;

    public bool Ready { get { return coolDownTimer >= coolDown && !Blocking; } }
    public bool BlockStop { get; private set; }
    public bool Blocking { get; private set; }
    
    float coolDown = 3f;
    float coolDownTimer;

    float activeTimer = 0;
    float activeDuration = 5f;

    public void Activate()
    {
        coolDownTimer = 0;
        Blocking = true;

        if (isPlayer)
        {
            fillArea.color = Color.yellow;
            cooldownSlider.maxValue = activeDuration;
            cooldownSlider.value = activeTimer;
        }
        //Debug.Log("Block Started");
    }

    public void Deactivate()
    {
        Blocking = false;
        BlockStop = true;
        activeTimer = 0;

        if (isPlayer)
        {
            fillArea.color = Color.white;
            cooldownSlider.maxValue = coolDown;
            cooldownSlider.value = coolDown;
        }
        //Debug.Log("Block Stopped");
    }

    private void Awake()
    {
        coolDownTimer = coolDown;
        if (isPlayer)
        {
            cooldownSlider = Instantiate(sliderPreafab, GetSliderPosition(), characterTransform.rotation);
            cooldownSlider.transform.parent = GameObject.FindGameObjectWithTag("Canvas").transform;
            cooldownSlider.maxValue = coolDown;
            cooldownSlider.value = coolDown;
            fillArea = cooldownSlider.GetComponentsInChildren<Image>()[1];
        }
    }

    Vector3 GetSliderPosition()
    {
        Vector3 worldPoint = new Vector3(characterTransform.position.x, characterTransform.position.y - uiOffset, characterTransform.position.z);
        return Camera.main.WorldToScreenPoint(worldPoint);
    }

    void Update()
    {
        if (BlockStop) BlockStop = false;

        if (!Blocking) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Blocking)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration)
            {
                Deactivate();
            }

            if (isPlayer)
            {
                cooldownSlider.value = activeTimer;
            }
        }
        else if (isPlayer)
        {
            cooldownSlider.value = coolDownTimer;
        }

        if (isPlayer)
        {
            cooldownSlider.transform.position = GetSliderPosition();
        }
    }
}