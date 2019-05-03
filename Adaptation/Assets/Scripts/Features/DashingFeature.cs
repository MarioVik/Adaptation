using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DashingFeature : MonoBehaviour
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

    public bool Ready { get { return coolDownTimer >= coolDown && !Dashing; } }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get; } = 30f;
    public float Range { get; } = 10f;

    [Header("For all users")]
    [SerializeField]
    GameObject effectPrefab;
    [SerializeField]
    Material dashMaterial;
    Material[] normalMaterials;
    Renderer[] renderers;

    float coolDown = 5f;
    float coolDownTimer;

    Vector3 posBefore;
    Vector3 direction;

    Collider collider;
    bool crashed;

    float DistanceTraversed { get { return Vector3.Distance(characterTransform.position, posBefore); } }

    public void Activate()
    {
        coolDownTimer = 0;
        posBefore = characterTransform.position;
        Dashing = true;

        collider.isTrigger = true;

        ChangeAllMaterials(dashMaterial);
        //Instantiate(effectPrefab, transform.position, transform.rotation, transform);
        Instantiate(effectPrefab, characterTransform.position, characterTransform.rotation, characterTransform);

        if (isPlayer)
        {
            fillArea.color = Color.yellow;
            cooldownSlider.maxValue = Range;
            cooldownSlider.value = 0f;
        }
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

        collider = GetComponent<Collider>();
    }

    Vector3 GetSliderPosition()
    {
        Vector3 worldPoint = new Vector3(characterTransform.position.x, characterTransform.position.y - uiOffset, characterTransform.position.z);
        return Camera.main.WorldToScreenPoint(worldPoint);
    }

    void Update()
    {
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Dashing)
        {
            if (DistanceTraversed >= Range || crashed)
            {
                Dashing = false;
                DashStop = true;
                crashed = false;

                collider.isTrigger = false;

                ChangeBackMaterials();

                if (isPlayer)
                {
                    fillArea.color = Color.white;
                    cooldownSlider.maxValue = coolDown;
                    cooldownSlider.value = coolDown;
                }
                //Debug.Log("Stopped dashing");
            }

            if (isPlayer)
            {
                cooldownSlider.value = DistanceTraversed;
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

    void ChangeAllMaterials(Material newMat)
    {
        renderers = GetComponentsInChildren<Renderer>();

        normalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            normalMaterials[i] = new Material(renderers[i].material);
            renderers[i].material = newMat;
        }
    }

    void ChangeBackMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = new Material(normalMaterials[i]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Environment" && Dashing)
            crashed = true;
    }
}