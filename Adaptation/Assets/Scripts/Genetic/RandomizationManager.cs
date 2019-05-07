using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class RandomizationManager : MonoBehaviour
{
    static public bool GameOver { get; private set; } = false;
    static public bool PlayerReady { get; set; } = true;

    public GameObject enemyPrefab;
    PlayerHealth playerHealth;
    PlayerSpawnManager playerSpawn;

    [SerializeField]
    SpawnPointDetection[] spawnPoints;

    static List<Individual> individuals;
    public static int InstantiatedIndividuals { get; private set; }
    public static int DeadIndividuals { get; private set; }
    public static int CurrentGeneration { get; private set; }
    public static int TotalGenerations { get; } = 10;
    public static int GenerationSize { get; } = 9;
    public static int ConcurrentIndividuals { get; } = 3;

    static float spawnFrequency = 8f;
    static float spawnTimer = 0;

    readonly int attributes = 12;

    System.Random rand = new System.Random();
    char[] possibleAttributes = new char[] { 'h', 'd',/* 's',*/ 'r', 'm' };
    char[] possibleAttackFeatures = new char[] { 'M', 'R' };
    char[] possbleUtilityFeatures = new char[] { 'B', 'D' };

    char RandomAttribute { get { return possibleAttributes[rand.Next(0, possibleAttributes.Length)]; } }
    char RandomAttackFeature { get { return possibleAttackFeatures[rand.Next(0, possibleAttackFeatures.Length)]; } }
    char RandomUtilityFeature { get { return possbleUtilityFeatures[rand.Next(0, possbleUtilityFeatures.Length)]; } }

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
    }

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        playerSpawn = GameObject.FindGameObjectWithTag("PlayerSpawn").GetComponent<PlayerSpawnManager>();

        CurrentGeneration = 1;
        GenLogManager.Initialize();
    }

    void EndGame()
    {
        GameOver = true;
    }

    void Update()
    {
        if (CurrentGeneration > TotalGenerations)
            EndGame();

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
            // If any spawn point is available; spawn an enemy. Otherwise leave the timer on max so an enemy is spawned as soon as possible
            if (GetSpawnPoint(out Transform spawnPoint))
            {
                Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                spawnTimer = spawnFrequency;
                return;
            }
        }
    }

    bool GetSpawnPoint(out Transform spawnPoint)
    {
        bool noAvailableSpawn = true;
        foreach (SpawnPointDetection tempSpawn in spawnPoints)
        {
            if (tempSpawn.Available)
            {
                noAvailableSpawn = false;
                break;
            }
        }

        if (noAvailableSpawn)
        {
            spawnPoint = null;
            return false;
        }

        SpawnPointDetection potentialSpawn;
        do
        {
            potentialSpawn = spawnPoints[rand.Next(0, spawnPoints.Length)];
        }
        while (!potentialSpawn.Available);

        spawnPoint = potentialSpawn.transform;
        return true;
    }

    List<string> RandomizeGeneration()
    {
        List<string> newGeneration = new List<string>();
        for (int i = 0; i < GenerationSize; i++)
        {
            string newIndividual = RandomizeIndividual();
            newGeneration.Add(newIndividual.ToString());
        }

        GenFilesManager.SaveGeneration(newGeneration);
        return newGeneration;
    }

    string RandomizeIndividual()
    {
        StringBuilder newIndividual = new StringBuilder();

        newIndividual.Append(RandomizeAttributes());
        newIndividual.Append('|');
        newIndividual.Append(RandomizeFeatures());

        return newIndividual.ToString();
    }

    string RandomizeAttributes()
    {
        StringBuilder newAttributes = new StringBuilder();

        for (int j = 0; j < attributes; j++)
        {
            newAttributes.Append(RandomAttribute);
        }

        return newAttributes.ToString();
    }

    string RandomizeFeatures()
    {
        StringBuilder newFeatures = new StringBuilder();

        newFeatures.Append(RandomAttackFeature);
        newFeatures.Append(RandomUtilityFeature);

        return newFeatures.ToString();
    }

    /// <summary>
    /// Breeds a new generation and saves it as a .txt file 
    /// </summary>
    List<string> CreateNextGeneration()
    {
        /*Logging details about last generation's individuals*/
        GenLogManager.SaveLog(LogType.Individual);

        List<string> newGeneration = BreedNewGeneration();
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
        GenLogManager.SaveLog(LogType.ForGraphing);

        List<string> newGeneration = RandomizeGeneration();

        /*Logging*/
        GenLogManager.LogNewGeneration(newGeneration);
        /*Logging*/
        GenLogManager.SaveLog(LogType.Progress);

        individuals.Clear();
        CurrentGeneration++;

        return newGeneration;
    }
}