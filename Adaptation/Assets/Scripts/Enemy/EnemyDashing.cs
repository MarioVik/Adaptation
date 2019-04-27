using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDashing : MonoBehaviour
{
    public bool DashStart { get; private set; }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get; } = 30f;
    public float DashDistance { get; } = 10f;

    Rigidbody rigid;

    [SerializeField]
    GameObject effectPrefab;

    [SerializeField]
    Material dashMaterial;
    Material[] enemyMaterials;

    float coolDown = 2f;
    float coolDownTimer;

    Vector3 posBefore;
    Vector3 direction;

    Collider collider;
    bool crashed;

    public void Activate()
    {
        if (coolDownTimer >= coolDown && !Dashing)
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
    }

    private void Awake()
    {
        coolDownTimer = coolDown;

        rigid = GetComponentInParent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    void Update()
    {
        if (DashStart) DashStart = false;
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Dashing)
        {
            if (Vector3.Distance(transform.position, posBefore) >= DashDistance
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
    }

    void ChangeAllMaterials(Material newMat)
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        enemyMaterials = new Material[children.Length];

        for (int i = 0; i < children.Length; i++)
        {
            enemyMaterials[i] = new Material(children[i].material);
            children[i].material = newMat;
        }
    }

    void ChangeBackMaterials()
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < children.Length; i++)
        {
            children[i].material = new Material(enemyMaterials[i]);
        }
    }

    private void FixedUpdate()
    {
        if (Dashing)
            transform.position += direction.normalized * DashSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Environment" && Dashing)
            crashed = true;
    }
}