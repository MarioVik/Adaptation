using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitnessTracker : MonoBehaviour
{
    Transform player;
    bool playerHasMelee, hasMelee;


    int individualNumber;
    float fitnessScore = 0;
    float damageModifier = 1.0f;
    float almostDamageModifier = 0.2f;
    float distanceModifier = 0.5f;
    float timeModifier = 0.1f;

    float totalDamage = 0;
    float totalAlmostDamage = 0;

    float timeAlive = 0;

    bool relevantDistance;
    double totalDistance = 0;
    long distRecordings = 0;
    float recordingFrequency = 0.5f;
    float distTimer = 0;

    float DistanceToPlayer { get { return Vector3.Distance(transform.position, player.position); } }
    float AverageDistance { get { return (float)(totalDistance / distRecordings); } }


    public void DamagedPlayer(int damage)
    {
        totalDamage += damage;
    }

    public void AlmostDamagedPlayer(int almostDamage)
    {
        totalAlmostDamage += almostDamage;
    }

    public void CalculateFitnessScore()
    {
        fitnessScore = 0;

        fitnessScore += (totalDamage * damageModifier);

        fitnessScore += (totalAlmostDamage * almostDamageModifier);

        if (hasMelee && !playerHasMelee)
        {
            fitnessScore -= (AverageDistance * distanceModifier);
        }
        else if (!hasMelee && playerHasMelee)
        {
            fitnessScore += (AverageDistance * distanceModifier);
        }

        fitnessScore += (timeAlive * timeModifier);

        GenLogManager.LogIndividual(individualNumber, totalDamage, totalAlmostDamage, AverageDistance, timeAlive, fitnessScore);
        GenerationManager.SetFitnessScore(individualNumber, fitnessScore);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        timeAlive += Time.deltaTime;
        distTimer += Time.deltaTime;

        if (distTimer >= recordingFrequency)
        {
            distTimer = 0;
            RecordDistance();
        }
    }

    void RecordDistance()
    {
        totalDistance += DistanceToPlayer;
        distRecordings++;
    }
}
