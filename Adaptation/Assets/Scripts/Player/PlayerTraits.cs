using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerTraits : MonoBehaviour
{
    [SerializeField]
    GameObject meleeObject;
    [SerializeField]
    GameObject rangedObject;
    [SerializeField]
    GameObject blockObject;
    //[SerializeField]
    //GameObject dashObject;

    bool ranged, melee;

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
                if (melee)
                    GetComponentInChildren<PlayerMeleeAttacking>().IncreaseAttackDamage(5);
                if (ranged)
                    GetComponentInChildren<PlayerShooting>().IncreaseAttackDamage(5);
                break;
            case 's':
                if (melee)
                    GetComponentInChildren<PlayerMeleeAttacking>().IncreaseAttackSpeed(0.1f);
                if (ranged)
                    GetComponentInChildren<PlayerShooting>().IncreaseAttackSpeed(0.1f);
                break;
            case 'r':
                if (melee)
                    GetComponentInChildren<PlayerMeleeAttacking>().IncreaseAttackRange(0.1f);
                if (ranged)
                    GetComponentInChildren<PlayerShooting>().IncreaseAttackRange(10f);
                break;
            case 'm':
                GetComponent<PlayerMovement>().IncreaseSpeed(1.5f);
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
                melee = true;
                break;
            case 'R':
                rangedObject.SetActive(true);
                ranged = true;
                break;
            case 'B':
                blockObject.SetActive(true);
                break;
            case 'D':
                GetComponent<PlayerDashing>().enabled = true;
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