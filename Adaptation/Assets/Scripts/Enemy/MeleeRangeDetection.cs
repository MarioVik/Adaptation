using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRangeDetection : MonoBehaviour
{
    [SerializeField]
    bool IsPlayer;
    [SerializeField]
    bool combo;

    MeleeRangeTracker trackerInParent;

    public void IncreaseRange(float increase)
    {
        GetComponent<CapsuleCollider>().height += increase;
    }

    private void Start()
    {
        trackerInParent = GetComponentInParent<MeleeRangeTracker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((IsPlayer && other.tag == "Enemy") || (!IsPlayer && other.tag == "Player"))
        {
            if (combo)
                trackerInParent.IncrementCombo();
            else
                trackerInParent.IncrementNormal(); ;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((IsPlayer && other.tag == "Enemy") || (!IsPlayer && other.tag == "Player"))
        {
            if (combo)
                trackerInParent.DecrementCombo();
            else
                trackerInParent.DecrementNormal();
        }
    }
}