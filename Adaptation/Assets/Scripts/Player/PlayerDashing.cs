﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDashing : MonoBehaviour
{
    public bool DashStart { get; private set; }
    public bool DashStop { get; private set; }
    public bool Dashing { get; private set; }
    public float DashSpeed { get { return dashSpeed; } }

    [SerializeField]
    GameObject effectPrefab;
    GameObject effect;

    [SerializeField]
    Slider cooldownSlider;

    float coolDown = 2f;
    float coolDownTimer;

    float dashSpeed = 50f;
    float dashDistance = 10f;

    Vector3 posBefore;
    Vector3 direction;

    Vector3 lastFramePos;

    private void Awake()
    {
        coolDownTimer = coolDown;
        cooldownSlider.maxValue = coolDown;
        cooldownSlider.value = coolDown;
    }

    void Update()
    {
        if (DashStart) DashStart = false;
        if (DashStop) DashStop = false;

        if (!Dashing) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Input.GetButtonDown("FeatureInput") && coolDownTimer >= coolDown && !Dashing)
        {
            coolDownTimer = 0;
            posBefore = transform.position;
            Dashing = true;
            DashStart = true;

            effect = Instantiate(effectPrefab, transform.position, transform.rotation);

            Debug.Log("Dashing");
        }

        if (Dashing)
        {
            if (Vector3.Distance(transform.position, posBefore) >= dashDistance
                || lastFramePos == transform.position
                /*|| Input.GetButtonUp("FeatureInput")*/)
            {
                Dashing = false;
                DashStop = true;
                Debug.Log("Stopped dashing");
            }
        }

        cooldownSlider.value = coolDownTimer;
    }

    private void FixedUpdate()
    {
        if (Dashing)
        {
            lastFramePos = transform.position;
            transform.position += direction.normalized * dashSpeed * Time.deltaTime;
        }
    }

    //private void ActivateDash()
    //{
    //    posBefore = transform.position;

    //    float horizontal = Input.GetAxisRaw("Horizontal");
    //    float vertical = Input.GetAxisRaw("Vertical");

    //    if (horizontal == 0 && vertical == 0)
    //        direction = transform.forward;
    //    else
    //        direction.Set(horizontal, 0f, vertical);

    //    Dashing = true;
    //}
}
