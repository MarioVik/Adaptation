using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitnessTracker : MonoBehaviour
{
    Transform player;

    bool playerHasMelee;
    bool hasMelee;

    int individualNumber;
    float fitnessScore = 0;
    float damageModifier = 0.5f;
    float almostDamageModifier = 0.1f;
    float distanceModifier = 0.5f;
    float timeModifier = 0.5f;

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

    public void DamagedPlayer(float damage)
    {
        totalDamage += damage;
    }

    public void AlmostDamagedPlayer(float almostDamage)
    {
        totalAlmostDamage += almostDamage;
    }

    public void CalculateFitnessScore()
    {
        if (GenerationManager.Tutorial)
        {
            return;
        }

        fitnessScore = 0;

        fitnessScore += (totalDamage * damageModifier);

        fitnessScore += (totalAlmostDamage * almostDamageModifier);

        string weaponType;
        float distanceBonus = AverageDistance;

        if (distanceBonus < 2)
            distanceBonus = 2;

        if (distanceBonus > 8)
            distanceBonus = 8;

        if (hasMelee && !playerHasMelee)
        {
            distanceBonus = (8 - distanceBonus);
            weaponType = "Melee";
        }
        else if (!hasMelee && playerHasMelee)
        {
            distanceBonus = (distanceBonus - 2);
            weaponType = "Ranged";
        }
        else
        {
            distanceBonus = 3;
            weaponType = "Same as player";
        }

        fitnessScore += distanceBonus * distanceModifier;

        fitnessScore += (timeAlive * timeModifier);

        GenerationManager.SetFitnessScore(individualNumber, fitnessScore);

        //*Logging*//
        GenLogManager.LogIndividual(individualNumber, weaponType,
            new float[] { totalDamage, damageModifier },
            new float[] { totalAlmostDamage, almostDamageModifier },
            new float[] { distanceBonus, distanceModifier },
            new float[] { timeAlive, timeModifier },
            fitnessScore);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHasMelee = player.GetComponentInChildren<PlayerTraits>().Melee;

        EnemyTraits traits = GetComponent<EnemyTraits>();
        individualNumber = traits.IndividualNumber;
        hasMelee = traits.Melee;
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
