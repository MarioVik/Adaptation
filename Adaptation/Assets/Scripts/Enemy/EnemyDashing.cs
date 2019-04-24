using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDashing : MonoBehaviour
{
    public bool DashStart { get; private set; }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get; } = 30f;

    Rigidbody rigid;

    [SerializeField]
    GameObject effectPrefab;

    float coolDown = 2f;
    float coolDownTimer;
    float dashDistance = 10f;

    Vector3 posBefore;
    Vector3 direction;

    Vector3 lastFramePos;

    public void Activate()
    {
        if (coolDownTimer >= coolDown && !Dashing)
        {
            coolDownTimer = 0;
            posBefore = transform.position;
            Dashing = true;
            DashStart = true;

            Instantiate(effectPrefab, rigid.position, rigid.rotation, rigid.transform);

            Debug.Log("Dashing");
        }
    }

    private void Awake()
    {
        coolDownTimer = coolDown;

        rigid = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        if (DashStart) DashStart = false;
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Dashing)
        {
            if (Vector3.Distance(transform.position, posBefore) >= dashDistance
                || lastFramePos == transform.position)
            {
                Dashing = false;
                DashStop = true;

                Debug.Log("Stopped dashing");
            }
        }
    }

    private void FixedUpdate()
    {
        if (Dashing)
        {
            lastFramePos = transform.position;
            transform.position += direction.normalized * DashSpeed * Time.deltaTime;
        }
    }
}
