using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyTraits : MonoBehaviour
{
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
                GetComponent<EnemyAttack>().IncreaseAttackDamage(5);
                break;
            case 's':
                GetComponent<EnemyAttack>().IncreaseAttackSpeed(0.1f);
                break;
            case 'r':
                GetComponent<EnemyAttack>().IncreaseAttackRange(0.1f);
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
                //attach meleeattacking script
                break;
            case 'R':
                //attach playershooting script
                break;
            case 'B':
                GetComponent<EnemyHealth>().EnableBlock();
                break;
            case 'D':
                GetComponent<EnemyMovement>().EnableDash();
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