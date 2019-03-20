using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyTraits : MonoBehaviour
{
    string traits;
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
        traits = GenerationManager.FetchTraits(out individualNumber);
        foreach (char tempChar in traits)
        {
            ApplyTrait(tempChar);
        }
    }

    void ApplyTrait(char traitChar)
    {
        switch (traitChar)
        {
            case 'h':
                GetComponent<EnemyHealth>().IncreaseHealth(10);
                break;
            case 'm':
                GetComponent<EnemyMovement>().IncreaseSpeed(1.5f);
                break;
            case 'd':
                GetComponent<EnemyAttack>().IncreaseAttackDamage(5);
                break;
            case 's':
                GetComponent<EnemyAttack>().IncreaseAttackSpeed(0.1f);
                break;
            default:
                throw new System.Exception("Enemy has unrecognized trait");
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
    }
}