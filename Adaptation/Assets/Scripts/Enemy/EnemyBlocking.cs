using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBlocking : MonoBehaviour
{
    public bool BlockStart { get; private set; }
    public bool BlockStop { get; private set; }
    public bool Blocking { get; private set; }

    float coolDownTimer;
    float coolDown = 5f;

    float activeTimer = 0;
    float activeDuration = 5f;

    public void Activate()
    {
        if (coolDownTimer >= coolDown)
        {
            coolDownTimer = 0;
            Blocking = true;
            BlockStart = true;
            Debug.Log("Block Started");
        }
    }

    private void Awake()
    {
        coolDownTimer = coolDown;
    }

    void Update()
    {
        if (BlockStart) BlockStart = false;
        if (BlockStop) BlockStop = false;

        if (!Blocking) coolDownTimer += Time.deltaTime;
        if (coolDownTimer > coolDown) coolDownTimer = coolDown;

        if (Blocking)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= activeDuration || Input.GetButtonUp("FeatureInput"))
            {
                Blocking = false;
                BlockStop = true;
                activeTimer = 0;
                Debug.Log("Block Stopped");
            }
        }
    }
}