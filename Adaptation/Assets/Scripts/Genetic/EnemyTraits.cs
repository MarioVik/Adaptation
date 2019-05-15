﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyTraits : MonoBehaviour
{
    [SerializeField]
    GameObject meleeObject;
    [SerializeField]
    GameObject rangedObject;
    [SerializeField]
    GameObject blockObject;

    public bool Melee { get; private set; }
    public bool Ranged { get; private set; }
    public bool Block { get; private set; }
    public bool Dash { get; private set; }
    public int IndividualNumber { get { return individualNumber; } }

    string[] traits;
    int individualNumber;

    void Start()
    {
        traits = GenerationManager.FetchTraits(out individualNumber).Split('|');

        foreach (char tempChar in traits[1])
            ApplyFeature(tempChar);

        foreach (char tempChar in traits[0])
            ApplyAttribute(tempChar);
    }

    void ApplyAttribute(char attrChar)
    {
        switch (attrChar)
        {
            case 'h':
                GetComponent<EnemyHealth>().IncreaseHealth();
                break;
            case 'd':
                if (Melee)
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackDamage();
                if (Ranged || Block)
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackDamage();
                break;
            case 's':
                if (Melee)
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackSpeed(0.1f);
                if (Ranged || Block)
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackSpeed(0.1f);
                break;
            case 'r':
                if (Melee)
                {
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackRange(0.15f);
                    GetComponentInChildren<MeleeRangeTracker>().IncreaseRange(0.15f);
                }
                if (Ranged || Block)
                {
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackRange(1.5f);
                }
                break;
            case 'm':
                GetComponentInParent<EnemyControlManager>().IncreaseMovementSpeed(2.5f);
                GetComponentInParent<FiniteStateMachine>().IncreaseMovementSpeed(2.5f);
                break;
            default:
                throw new System.Exception("Enemy has unrecognized attribute");
        }
    }

    void ApplyFeature(char featChar)
    {
        switch (featChar)
        {
            case 'M':
                meleeObject.SetActive(true);
                Melee = true;
                break;
            case 'R':
                rangedObject.SetActive(true);
                Ranged = true;
                break;
            case 'B':
                blockObject.SetActive(true);
                if (!Ranged)
                {
                    rangedObject.SetActive(true);
                    rangedObject.GetComponent<RangedAttackFeature>().OnlyForBlock = true;
                    rangedObject.GetComponent<MeshRenderer>().enabled = false;
                }
                Block = true;
                break;
            case 'D':
                GetComponent<DashingFeature>().enabled = true;
                Dash = true;
                break;
            default:
                throw new System.Exception("Enemy has unrecognized feature");
        }
    }
}