using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockingFeature : MonoBehaviour
{
    [SerializeField]
    Transform characterTransform;

    [SerializeField]
    Slider sliderPreafab;
    Slider cooldownSlider;
    Image fillArea;
    float uiOffset = 0.2f;
    float uiScale = 0.005f;

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

        fillArea.color = Color.yellow;
        cooldownSlider.maxValue = activeDuration;
        cooldownSlider.value = activeTimer;
        //Debug.Log("Block Started");
    }

    public void Deactivate()
    {
        Blocking = false;
        BlockStop = true;
        activeTimer = 0;

        fillArea.color = Color.white;
        cooldownSlider.maxValue = coolDown;
        cooldownSlider.value = coolDown;
        //Debug.Log("Block Stopped");
    }

    private void Start()
    {
        coolDownTimer = coolDown;

        cooldownSlider = Instantiate(sliderPreafab, characterTransform.position, characterTransform.rotation);
        cooldownSlider.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        cooldownSlider.maxValue = coolDown;
        cooldownSlider.value = coolDown;
        fillArea = cooldownSlider.GetComponentsInChildren<Image>()[1];
        cooldownSlider.transform.localScale *= uiScale;
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

            cooldownSlider.value = activeTimer;
        }
        else
        {
            cooldownSlider.value = coolDownTimer;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        Transform cameraTransform = CameraManager.singleton.camTrans;
        Plane plane = new Plane(cameraTransform.forward, cameraTransform.position);

        cooldownSlider.transform.rotation = Quaternion.LookRotation(plane.normal, Vector3.up);
        cooldownSlider.transform.position = new Vector3(characterTransform.position.x, characterTransform.position.y - uiOffset, characterTransform.position.z);

        Vector3 dirToCamera = (cameraTransform.position - characterTransform.position).normalized;
        cooldownSlider.transform.position += dirToCamera * 4;
    }
}