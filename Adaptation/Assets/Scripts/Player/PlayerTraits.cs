using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerTraits : MonoBehaviour
{
    [SerializeField]
    GameObject meleePrefab;
    [SerializeField]
    GameObject rangedPrefab;
    [SerializeField]
    GameObject blockPrefab;

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
                ApplyMelee();
                break;
            case 'R':
                ApplyRanged();
                break;
            case 'B':
                ApplyBlock();
                break;
            case 'D':
                GetComponent<PlayerMovement>().EnableDash();
                break;
            default:
                throw new System.Exception("Player has unrecognized feature");
        }
    }

    void ApplyMelee()
    {
        GameObject rightHand = new GameObject();
        GetChildObject(transform, "RightHand", ref rightHand);
        GameObject meleeObj = Instantiate(meleePrefab);
        meleeObj.transform.parent = rightHand.transform;
        melee = true;
    }

    void ApplyRanged()
    {
        GameObject rightHand = new GameObject();
        GetChildObject(transform, "RightHand", ref rightHand);
        GameObject rangedObj = Instantiate(rangedPrefab);
        rangedObj.transform.parent = rightHand.transform;
        ranged = true;
    }

    void ApplyBlock()
    {
        GameObject leftHand = new GameObject();
        GetChildObject(transform, "LeftHand", ref leftHand);
        GameObject blockObj = Instantiate(blockPrefab);
        blockObj.transform.parent = leftHand.transform;

        GetComponent<PlayerHealth>().EnableBlock();
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