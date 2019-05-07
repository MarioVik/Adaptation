using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    static public bool PlayerReady { get; set; } = true;

    public GameObject enemyPrefab;
    PlayerHealth playerHealth;
    PlayerSpawnManager playerSpawn;

    [SerializeField]
    Transform[] spawnPoints;

    static List<Individual> individuals;
    public static int InstantiatedIndividuals { get; private set; }
    public static int DeadIndividuals { get; private set; }
    public static int CurrentGeneration { get; private set; }
    public static int TotalGenerations { get; } = 10;
    public static int GenerationSize { get; } = 9;
    public static int ConcurrentIndividuals { get; } = 3;

    static float spawnFrequency = 8f;
    static float spawnTimer = 0;

    readonly int attributes = 8;
    readonly int features = 2;

    System.Random rand = new System.Random();
    char[] possibleAttributes = new char[] { 'h', 'd',/* 's',*/ 'r', 'm' };
    char[] possibleAttackFeatures = new char[] { 'M', 'R' };
    char[] possbleUtilityFeatures = new char[] { 'B', 'D' };

    char RandomAttribute { get { return possibleAttributes[rand.Next(0, possibleAttributes.Length)]; } }
    char RandomAttackFeature { get { return possibleAttackFeatures[rand.Next(0, possibleAttackFeatures.Length)]; } }
    char RandomUtilityFeature { get { return possbleUtilityFeatures[rand.Next(0, possbleUtilityFeatures.Length)]; } }

    int mutationChance = 8;

    bool gameStarted = false;

    static public string FetchTraits(out int individualNumber)
    {
        if (InstantiatedIndividuals >= individuals.Count)
            throw new Exception("exceeded generation amount");

        string individualTraits = individuals[InstantiatedIndividuals].Traits;
        individualNumber = InstantiatedIndividuals;
        InstantiatedIndividuals++;
        return individualTraits;
    }

    static public void SetFitnessScore(int individualNumber, float fitnessScore)
    {
        individuals[individualNumber].FitnessScore = fitnessScore;
        DeadIndividuals++;

        if (DeadIndividuals >= InstantiatedIndividuals)
            GenLogManager.SaveLog(LogType.Individual);

        spawnTimer = 0;

    }

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        playerSpawn = GameObject.FindGameObjectWithTag("PlayerSpawn").GetComponent<PlayerSpawnManager>();

        CurrentGeneration = 1;
        GenLogManager.Initialize();
    }

    void Update()
    {
        if (playerHealth.IsDead || !PlayerReady)
            return;

        if (!gameStarted)
        {
            individuals = StringsToIndividuals(RandomizeGeneration());

            ResetVariables();
            gameStarted = true;

            PlayerReady = false;
            playerSpawn.NewWave();
            return;

            Spawn(GenerationSize);
        }
        else if (InstantiatedIndividuals >= GenerationSize
            && DeadIndividuals >= GenerationSize
            && CurrentGeneration <= TotalGenerations)
        {
            individuals = StringsToIndividuals(CreateNextGeneration());
            ResetVariables();

            PlayerReady = false;
            playerSpawn.NewWave();
            return;

            Spawn(GenerationSize);
        }


        spawnTimer += Time.deltaTime;

        if (InstantiatedIndividuals - DeadIndividuals <= 0)
            spawnTimer = spawnFrequency;

        if (spawnTimer >= spawnFrequency)
        {
            if ((InstantiatedIndividuals - DeadIndividuals < ConcurrentIndividuals)
                && (InstantiatedIndividuals < GenerationSize)
                && (CurrentGeneration <= TotalGenerations))
            {
                spawnTimer = 0;
                Spawn();
            }
        }
    }

    public List<Individual> StringsToIndividuals(List<string> stringList)
    {
        List<Individual> individualList = new List<Individual>();

        foreach (string tempString in stringList)
        {
            individualList.Add(new Individual(tempString));
        }

        return individualList;
    }

    void ResetVariables()
    {
        InstantiatedIndividuals = 0;
        DeadIndividuals = 0;
        playerHealth.ResetHealth();
    }

    void Spawn(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform spawnPoint = GetSpawnPoint();
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    Transform GetSpawnPoint()
    {
        Transform potentialSpawn;
        do
        {
            potentialSpawn = spawnPoints[rand.Next(0, spawnPoints.Length)];
        }
        while (!potentialSpawn.GetComponent<SpawnPointDetection>().Available);

        return potentialSpawn;
    }

    List<string> RandomizeGeneration()
    {
        List<string> newGeneration = new List<string>();
        for (int i = 0; i < GenerationSize; i++)
        {
            newGeneration.Add(RandomizeIndividual());
        }
        GenFilesManager.SaveGeneration(newGeneration);
        return newGeneration;
    }

    string RandomizeIndividual()
    {
        StringBuilder newIndividual = new StringBuilder();

        for (int j = 0; j < attributes; j++)
        {
            newIndividual.Append(RandomAttribute);
        }
        newIndividual.Append('|');

        newIndividual.Append(RandomAttackFeature);
        newIndividual.Append(RandomUtilityFeature);

        return newIndividual.ToString();
    }

    /// <summary>
    /// Breeds a new generation and saves it as a .txt file 
    /// </summary>
    List<string> CreateNextGeneration()
    {
        List<string> newGeneration = BreedNewGeneration();
        GenFilesManager.SaveGeneration(newGeneration);
        return newGeneration;
    }

    /// <summary>
    /// Looks at the top half of the fitness-scores from the last generation and uses
    /// a single point crossover to create a new generation
    /// </summary>
    List<string> BreedNewGeneration()
    {
        List<Individual> sortedList = individuals.OrderByDescending(i => i.FitnessScore).ToList();
        /*Logging*/
        GenLogManager.LogAfterSort(sortedList);
        GenLogManager.LogForGraphing(sortedList);

        List<string> newAttributes = new List<string>();
        for (int i = 0; i < sortedList.Count / 2; i++)
        {
            newAttributes.Add(sortedList[i].Traits);
        }
        /*Logging*/
        GenLogManager.LogParentsToBreed(newAttributes);

        int numberOfParents = newAttributes.Count;
        for (int indexParentA = 0; indexParentA < numberOfParents; indexParentA += 2)
        {
            int indexParentB = indexParentA + 1;

            StringBuilder newChildA = new StringBuilder();
            StringBuilder newChildB = new StringBuilder();
            for (int indexParentTrait = 0; indexParentTrait < newAttributes[indexParentA].Length; indexParentTrait++)
            {
                if (indexParentTrait < newAttributes[indexParentA].Length / 2)
                {
                    newChildA.Append(newAttributes[indexParentA][indexParentTrait]);
                    newChildB.Append(newAttributes[indexParentB][indexParentTrait]);
                }
                else
                {
                    newChildA.Append(newAttributes[indexParentB][indexParentTrait]);
                    newChildB.Append(newAttributes[indexParentA][indexParentTrait]);
                }
            }
            ConsiderMutation(newChildA);
            ConsiderMutation(newChildB);
            newAttributes.Add(newChildA.ToString());
            newAttributes.Add(newChildB.ToString());
        }

        newAttributes.Add(RandomizeIndividual());

        /*Logging*/
        GenLogManager.LogNewGeneration(newAttributes);
        /*Logging*/
        GenLogManager.SaveLog(LogType.Progress);

        individuals.Clear();
        CurrentGeneration++;

        return newAttributes;
    }

    /// <summary>
    /// Every child is sent here and considered for mutation, they have an x% chance
    /// of getting a random trait replaced with a new random one
    /// </summary>
    /// <param name="child">A newly bred child for the next generation</param>
    void ConsiderMutation(StringBuilder child)
    {
        if (rand.Next(0, 101) <= mutationChance)
        {
            /*Logging*/
            GenLogManager.LogMutatatedIndividual(child.ToString(), afterMutation: false);

            int placeToChange = rand.Next(0, child.Length);
            char newTrait;
            do { newTrait = RandomAttribute; } while (newTrait == child[placeToChange]);
            child[placeToChange] = newTrait;

            /*Logging*/
            GenLogManager.LogMutatatedIndividual(child.ToString(), afterMutation: true);
        }
    }
}