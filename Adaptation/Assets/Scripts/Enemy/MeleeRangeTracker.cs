using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRangeTracker : MonoBehaviour
{
    public bool NormalRange { get { return collidersInNormalRange > 0; } }
    public bool ComboRange { get { return collidersInComboRange > 0; } }

    int collidersInNormalRange = 0;
    int collidersInComboRange = 0;

    public void IncrementNormal() => collidersInNormalRange++;
    public void DecrementNormal() => collidersInNormalRange--;
    public void IncrementCombo() => collidersInComboRange++;
    public void DecrementCombo() => collidersInComboRange--;
}