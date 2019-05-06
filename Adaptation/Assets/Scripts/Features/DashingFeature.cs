using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DashingFeature : MonoBehaviour
{
    [SerializeField]
    Transform characterTransform;

    [SerializeField]
    Slider sliderPreafab;
    Slider cooldownSlider;
    Image fillArea;
    float uiOffset = 0.2f;
    float uiScale = 0.005f;

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

        fillArea.color = Color.yellow;
        cooldownSlider.maxValue = Range;
        cooldownSlider.value = 0f;
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

        collider = GetComponent<Collider>();
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

                fillArea.color = Color.white;
                cooldownSlider.maxValue = coolDown;
                cooldownSlider.value = coolDown;

                //Debug.Log("Stopped dashing");
            }

            cooldownSlider.value = DistanceTraversed;
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

    private void OnDestroy()
    {
        if (cooldownSlider != null)
            Destroy(cooldownSlider.gameObject);
    }
}