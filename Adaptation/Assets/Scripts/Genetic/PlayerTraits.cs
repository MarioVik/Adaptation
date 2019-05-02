using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTraits : MonoBehaviour
{
    [SerializeField] GameObject meleeObject;
    [SerializeField] GameObject rangedObject;
    [SerializeField] GameObject blockObject;

    [SerializeField]
    Image featureImage;
    [SerializeField]
    Sprite blockImage;
    [SerializeField]
    Sprite dashImage;

    public bool Ranged { get; private set; }
    public bool Melee { get; private set; }

    void Start()
    {
        string[] traits = GenFilesManager.LoadPlayer().Split('|');

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
                GetComponent<PlayerHealth>().IncreaseHealth(10);
                break;
            case 'd':
                if (Melee)
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackDamage(5);
                if (Ranged)
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackDamage(5);
                break;
            case 's':
                if (Melee)
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackSpeed(0.2f);
                if (Ranged)
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackSpeed(10f);
                break;
            case 'o':
            //if (melee)
            //    GetComponentInChildren<PlayerMeleeAttacking>().IncreaseAttackRate(0.1f);
            //if (ranged)
            //    GetComponentInChildren<PlayerRangedAttacking>().IncreaseAttackRate(0.1f);
            //break;
            case 'r':
                if (Melee)
                {
                    GetComponentInChildren<MeleeAttackFeature>().IncreaseAttackRange(0.2f);
                    GetComponentInChildren<MeleeRangeTracker>().IncreaseRange(0.1f);
                }
                if (Ranged)
                {
                    GetComponentInChildren<RangedAttackFeature>().IncreaseAttackRange(1f);
                }
                break;
            case 'm':
                GetComponentInParent<PlayerControlManager>().IncreaseMovementSpeed(1.5f);
                break;
            default:
                throw new System.Exception("Player has unrecognized attribute");
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
                featureImage.sprite = blockImage;
                break;
            case 'D':
                GetComponent<DashingFeature>().enabled = true;
                featureImage.sprite = dashImage;
                break;
            default:
                throw new System.Exception("Player has unrecognized feature");
        }
    }

    void GetChildObject(Transform parent, string _tag, ref GameObject targetChild)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag)
            {
                targetChild = child.gameObject;
            }
            if (child.childCount > 0)
            {
                GetChildObject(child, _tag, ref targetChild);
            }
        }
    }
}