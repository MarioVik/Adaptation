using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDashing : MonoBehaviour
{
    public bool DashStart { get; private set; }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get; } = 30f;

    Rigidbody rigid;

    [SerializeField]
    GameObject effectPrefab;

    [SerializeField]
    Material dashMaterial;
    Material[] playerMaterials;
    //Material[] currentMaterials;

    [SerializeField]
    Slider cooldownSlider;

    float coolDown = 2f;
    float coolDownTimer;
    float dashDistance = 10f;

    Vector3 posBefore;
    Vector3 direction;

    Collider collider;
    bool crashed;

    private void Awake()
    {
        coolDownTimer = coolDown;
        cooldownSlider.maxValue = coolDown;
        cooldownSlider.value = coolDown;

        rigid = GetComponentInParent<Rigidbody>();
        collider = GetComponent<Collider>();

        //Renderer[] children = GetComponentsInChildren<Renderer>();
        //playerMaterials = new Material[children.Length];
        //for (int i = 0; i < children.Length; i++)
        //    playerMaterials[i] = new Material(children[i].material);
    }

    void Update()
    {
        if (DashStart) DashStart = false;
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Input.GetButtonDown("FeatureInput") && 
            GetComponentInParent<PlayerControlManager>().MovementInput && 
            coolDownTimer >= coolDown && !Dashing)
        {
            coolDownTimer = 0;
            posBefore = transform.position;
            Dashing = true;
            DashStart = true;

            collider.isTrigger = true;

            ChangeAllMaterials(dashMaterial);
            Instantiate(effectPrefab, rigid.position, rigid.rotation, rigid.transform);

            //Debug.Log("Dashing");
        }

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

        cooldownSlider.value = coolDownTimer;
    }

    void ChangeAllMaterials(Material newMat)
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        playerMaterials = new Material[children.Length];

        for (int i = 0; i < children.Length; i++)
        {
            playerMaterials[i] = new Material(children[i].material);
            children[i].material = newMat;
        }
    }

    void ChangeBackMaterials()
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < children.Length; i++)
        {
            children[i].material = new Material(playerMaterials[i]);
        }
    }

    //private void FixedUpdate()
    //{
    //    if (Dashing)
    //        transform.position += direction.normalized * DashSpeed * Time.deltaTime;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Environment" && Dashing)
            crashed = true;
    }
}