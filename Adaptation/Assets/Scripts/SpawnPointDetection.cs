using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointDetection : MonoBehaviour
{
    public bool Available { get; private set; }
    bool onGround;

    private void Awake()
    {
        Available = true;
    }

    //private void Update()
    //{
    //    Debug.Log(gameObject.name + " Available: " + Available);
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground")
            onGround = true;

        if (other.tag == "Environment" || other.tag == "Enemy")
            Available = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
            onGround = false;

        if (onGround)
            Available = true;
    }
}