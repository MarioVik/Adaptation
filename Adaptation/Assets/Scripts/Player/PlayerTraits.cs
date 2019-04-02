using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTraits : MonoBehaviour
{
    void Start()
    {
        string[] traits = GenFilesManager.LoadPlayer().Split('|');

        foreach (char tempChar in traits[0])
            ApplyAttribute(tempChar);

        foreach (char tempChar in traits[1])
            ApplyFeature(tempChar);
    }

    void ApplyAttribute(char attrChar)
    {
        switch (attrChar)
        {
            case 'h':
                GetComponent<PlayerHealth>().IncreaseHealth(10);
                break;
            case 'd':
                GetComponent<PlayerMeleeAttacking>().IncreaseAttackDamage(5);
                break;
            case 's':
                GetComponent<PlayerMeleeAttacking>().IncreaseAttackSpeed(0.1f);
                break;
            case 'r':
                GetComponent<PlayerMeleeAttacking>().IncreaseAttackRange(0.1f);
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
                //attach meleeattacking script
                break;
            case 'R':
                //attach playershooting script
                break;
            case 'B':
                GetComponent<PlayerHealth>().EnableBlock();
                break;
            case 'D':
                GetComponent<PlayerMovement>().EnableDash();
                break;
            default:
                throw new System.Exception("Player has unrecognized feature");
        }
    }
}
