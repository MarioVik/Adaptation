using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointDetection : MonoBehaviour
{
    static readonly float distanceFromPlayer = 15;

    [SerializeField]
    Vector3 direction;

    public bool Available { get; private set; }
    bool onGround;

    private void Awake()
    {
        Available = true;

        transform.position += direction.normalized * distanceFromPlayer;
    }

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