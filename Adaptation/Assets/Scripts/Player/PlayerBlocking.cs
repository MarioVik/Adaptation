using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlocking : MonoBehaviour
{
    public bool Blocking { get; private set; }

    float coolDownTimer;
    float coolDown = 5f;

    float activeTimer = 0;
    float activeDuration = 5f;

    private void Awake()
    {
        coolDownTimer = coolDown;
    }

    void Update()
    {
        coolDownTimer += Time.deltaTime;

        if (Input.GetButtonDown("Block") && coolDownTimer >= coolDown)
        {
            Blocking = true;
            coolDownTimer = 0;
            Debug.Log("Block Started");
        }

        if (Blocking)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration)
            {
                Blocking = false;
                activeTimer = 0;
                Debug.Log("Block Stopped");
            }
        }
    }
}