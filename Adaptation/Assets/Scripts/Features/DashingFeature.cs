using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DashingFeature : MonoBehaviour
{
    [Header("Only if user is player")]
    [SerializeField]
    bool isPlayer;
    [SerializeField]
    Slider cooldownSlider;

    public bool Ready { get { return coolDownTimer >= coolDown && !Dashing; } }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get; } = 30f;

    [Header("For all users")]
    [SerializeField]
    GameObject effectPrefab;
    [SerializeField]
    Material dashMaterial;
    Material[] normalMaterials;
    //Material[] currentMaterials;

    float coolDown = 2f;
    float coolDownTimer;
    float dashDistance = 10f;

    Vector3 posBefore;
    Vector3 direction;

    Collider collider;
    bool crashed;

    public void Activate()
    {
        coolDownTimer = 0;
        posBefore = transform.position;
        Dashing = true;

        collider.isTrigger = true;

        ChangeAllMaterials(dashMaterial);
        Instantiate(effectPrefab, transform.position, transform.rotation, transform);

        //Debug.Log("Dashing");
    }

    private void Awake()
    {
        coolDownTimer = coolDown;

        if (isPlayer)
        {
            cooldownSlider.maxValue = coolDown;
            cooldownSlider.value = coolDown;
        }

        collider = GetComponent<Collider>();

        //Renderer[] children = GetComponentsInChildren<Renderer>();
        //playerMaterials = new Material[children.Length];
        //for (int i = 0; i < children.Length; i++)
        //    playerMaterials[i] = new Material(children[i].material);
    }

    void Update()
    {
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Dashing)
        {
            if (Vector3.Distance(transform.position, posBefore) >= dashDistance
                || crashed)
            {
                Dashing = false;
                DashStop = true;
                crashed = false;

                collider.isTrigger = false;

                ChangeBackMaterials();
                //Debug.Log("Stopped dashing");
            }
        }

        if (isPlayer)
        {
            cooldownSlider.value = coolDownTimer;
        }

    }

    void ChangeAllMaterials(Material newMat)
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        normalMaterials = new Material[children.Length];

        for (int i = 0; i < children.Length; i++)
        {
            normalMaterials[i] = new Material(children[i].material);
            children[i].material = newMat;
        }
    }

    void ChangeBackMaterials()
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < children.Length; i++)
        {
            children[i].material = new Material(normalMaterials[i]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Environment" && Dashing)
            crashed = true;
    }
}