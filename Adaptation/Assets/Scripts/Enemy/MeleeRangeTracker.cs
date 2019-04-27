using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRangeTracker : MonoBehaviour
{
    public bool NormalRange { get { return CollidersInNormalRange > 0; } }
    public bool ComboRange { get { return CollidersInComboRange > 0; } }

    public int CollidersInNormalRange { get; set; }
    public int CollidersInComboRange { get; set; }

    private void Awake()
    {
        CollidersInNormalRange = 0;
        CollidersInComboRange = 0;
    }
}