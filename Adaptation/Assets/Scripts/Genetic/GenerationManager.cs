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
    public static int TotalGenerations { get; private set; } = 10;

    static float spawnFrequency = 8f;
    static float spawnTimer = 0;

    readonly int generationSize = 8;
    readonly int concurrentIndividuals = 3;
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

        CurrentGeneration = 0;
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

            Spawn(generationSize);
        }
        else if (InstantiatedIndividuals >= generationSize
            && DeadIndividuals >= generationSize
            && CurrentGeneration < TotalGenerations)
        {
            individuals = StringsToIndividuals(CreateNextGeneration());
            ResetVariables();

            PlayerReady = false;
            playerSpawn.NewWave();
            return;

            Spawn(generationSize);
        }


        spawnTimer += Time.deltaTime;

        if (InstantiatedIndividuals - DeadIndividuals <= 0)
            spawnTimer = spawnFrequency;

        if (spawnTimer >= spawnFrequency)
        {
            if ((InstantiatedIndividuals - DeadIndividuals < concurrentIndividuals)
                && (InstantiatedIndividuals < generationSize)
                && (CurrentGeneration < TotalGenerations))
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
        for (int i = 0; i < generationSize; i++)
        {
            StringBuilder newIndividual = new StringBuilder();

            for (int j = 0; j < attributes; j++)
            {
                newIndividual.Append(RandomAttribute);
            }
            newIndividual.Append('|');

            newIndividual.Append(RandomAttackFeature);
            newIndividual.Append(RandomUtilityFeature);

            newGeneration.Add(newIndividual.ToString());
        }
        GenFilesManager.SaveGeneration(newGeneration);
        return newGeneration;
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

        List<string> newGeneration = new List<string>();
        for (int i = 0; i < sortedList.Count / 2; i++)
        {
            newGeneration.Add(sortedList[i].Traits);
        }
        /*Logging*/
        GenLogManager.LogParentsToBreed(newGeneration);

        int numberOfParents = newGeneration.Count;
        for (int indexParentA = 0; indexParentA < numberOfParents; indexParentA += 2)
        {
            int indexParentB = indexParentA + 1;

            StringBuilder newChildA = new StringBuilder();
            StringBuilder newChildB = new StringBuilder();
            for (int indexParentTrait = 0; indexParentTrait < newGeneration[indexParentA].Length; indexParentTrait++)
            {
                if (indexParentTrait < newGeneration[indexParentA].Length / 2)
                {
                    newChildA.Append(newGeneration[indexParentA][indexParentTrait]);
                    newChildB.Append(newGeneration[indexParentB][indexParentTrait]);
                }
                else
                {
                    newChildA.Append(newGeneration[indexParentB][indexParentTrait]);
                    newChildB.Append(newGeneration[indexParentA][indexParentTrait]);
                }
            }
            ConsiderMutation(newChildA);
            ConsiderMutation(newChildB);
            newGeneration.Add(newChildA.ToString());
            newGeneration.Add(newChildB.ToString());
        }
        /*Logging*/
        GenLogManager.LogNewGeneration(newGeneration);
        /*Logging*/
        GenLogManager.SaveLog(LogType.Progress);

        individuals.Clear();
        CurrentGeneration++;

        return newGeneration;
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