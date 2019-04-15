using System.Collections;
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

    bool ranged, melee;

    string[] traits;
    int individualNumber;
    float totalDamage = 0;
    float timeAlive = 0;
    float DistanceToPlayer { get { return Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position); } }
    float fitnessScore = 0;

    public void DamagedPlayer(int damage)
    {
        totalDamage += damage;
    }

    public void CalculateFitnessScore()
    {
        fitnessScore += totalDamage;
        fitnessScore += (0.5f * timeAlive);
        fitnessScore -= (1.5f * DistanceToPlayer);
        GenLogManager.LogIndividual(individualNumber, totalDamage, timeAlive, DistanceToPlayer, fitnessScore);
        GenerationManager.SetFitnessScore(individualNumber, fitnessScore);
    }

    void Start()
    {
        traits = GenerationManager.FetchTraits(out individualNumber).Split('|');

        foreach (char tempChar in traits[0])
            ApplyAttribute(tempChar);

        //foreach (char tempChar in traits[1])
        //    ApplyFeature(tempChar);
    }

    void ApplyAttribute(char attrChar)
    {
        switch (attrChar)
        {
            case 'h':
                GetComponent<EnemyHealth>().IncreaseHealth(10);
                break;
            case 'd':
                if (melee)
                    GetComponentInChildren<EnemyMeleeAttacking>().IncreaseAttackDamage(5);
                if (ranged)
                    GetComponentInChildren<EnemyRangedAttacking>().IncreaseAttackDamage(5);
                break;
            case 's':
                if (melee)
                    GetComponentInChildren<EnemyMeleeAttacking>().IncreaseAttackSpeed(0.1f);
                if (ranged)
                    GetComponentInChildren<EnemyRangedAttacking>().IncreaseAttackSpeed(10f);
                break;
            case 'o':
                if (melee)
                    GetComponentInChildren<EnemyMeleeAttacking>().IncreaseAttackRate(0.1f);
                if (ranged)
                    GetComponentInChildren<EnemyRangedAttacking>().IncreaseAttackRate(0.1f);
                break;
            case 'r':
                if (melee)
                    GetComponentInChildren<EnemyMeleeAttacking>().IncreaseAttackRange(0.1f);
                if (ranged)
                    GetComponentInChildren<EnemyRangedAttacking>().IncreaseAttackRange(1f);
                break;
            case 'm':
                GetComponent<EnemyMovement>().IncreaseSpeed(1.5f);
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
                GetComponent<EnemyDashing>().enabled = true;
                break;
            default:
                throw new System.Exception("Enemy has unrecognized feature");
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
    }
}