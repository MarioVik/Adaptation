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
    static public bool GameOver { get; private set; } = false;
    static public bool PlayerReady { get; set; } = true;

    public GameObject enemyPrefab;
    PlayerHealth playerHealth;
    PlayerReadyManager readyManager;

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
    }

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        readyManager = GameObject.FindGameObjectWithTag("ReadyArea").GetComponent<PlayerReadyManager>();

        CurrentGeneration = 1;
        GenLogManager.Initialize();
    }

    void EndGame()
    {
        GameOver = true;
    }

    void ResetWave()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyController");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        foreach (Individual ind in individuals)
        {
            ind.FitnessScore = 0;
        }

        //PlayerReady = false;
        //readyManager.NewWave();

        ResetVariables();

        //GameObject.FindGameObjectWithTag("PlayerController").transform.SetPositionAndRotation(readyManager.transform.position, readyManager.transform.rotation);
    }

    void Update()
    {
        if (CurrentGeneration > TotalGenerations)
            EndGame();

        if (playerHealth.IsDead && PlayerReady)
        {
            ResetWave();
        }
        else if (playerHealth.IsDead && !PlayerReady)
        {
            readyManager.NewWave(dead: true);
            return;
        }
        else if (!PlayerReady)
        {
            return;
        }

        if (!gameStarted)
        {
            individuals = StringsToIndividuals(CreateFirstGeneration());

            ResetVariables();
            gameStarted = true;

            PlayerReady = false;
            readyManager.NewWave();
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
            readyManager.NewWave();
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

    List<string> CreateFirstGeneration()
    {
        string[] permutations = { "MB", "MD", "RB", "RD" };

        List<string> features = new List<string>();
        for (int i = 0; i < (GenerationSize - 1) / permutations.Length; i++)
        {
            features.Add(permutations[0]);
            features.Add(permutations[1]);
            features.Add(permutations[2]);
            features.Add(permutations[3]);
        }

        List<string> newGeneration = new List<string>();
        for (int i = 0; i < (GenerationSize - 1); i++)
        {
            StringBuilder newIndividual = new StringBuilder();

            newIndividual.Append(RandomizeAttributes());
            newIndividual.Append('|');

            int randomIndex = rand.Next(0, features.Count); // Add a random feature permutation and remove it from the selection
            newIndividual.Append(features[randomIndex]);
            features.RemoveAt(randomIndex);

            newGeneration.Add(newIndividual.ToString());
        }

        newGeneration.Add(RandomizeIndividual());

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
        GenLogManager.SaveLog(LogType.ForGraphing);

        List<string> newGeneration = new List<string>();
        List<string> newAttributes = new List<string>();
        List<string> newFeatures = new List<string>();
        for (int i = 0; i < sortedList.Count / 2; i++)
        {
            newGeneration.Add(sortedList[i].Traits);

            string[] traits = sortedList[i].Traits.Split('|');
            newAttributes.Add(traits[0]);
            newFeatures.Add(traits[1]);
        }
        /*Logging*/
        GenLogManager.LogParentsToBreed(newAttributes, newFeatures);

        int numberOfParents = newAttributes.Count;
        for (int indexParentA = 0; indexParentA < numberOfParents; indexParentA += 2)
        {
            int indexParentB = indexParentA + 1;

            StringBuilder newChildA = new StringBuilder();
            StringBuilder newChildB = new StringBuilder();
            int crossoverPoint = rand.Next(1, attributes);

            for (int indexParentTrait = 0; indexParentTrait < attributes; indexParentTrait++)
            {
                if (indexParentTrait < crossoverPoint)
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

            newChildA.Append('|');
            newChildB.Append('|');

            // Copy over the parent's features  to whichever child inherited the bigger part of the parent's genotype
            if (crossoverPoint > attributes / 2)
            {
                newChildA.Append(newFeatures[indexParentA]);
                newChildB.Append(newFeatures[indexParentB]);
            }
            else
            {
                newChildA.Append(newFeatures[indexParentB]);
                newChildB.Append(newFeatures[indexParentA]);
            }

            // If any of the children are identical to any of the parents; redo the crossover
            if (newChildA.ToString() == newGeneration[indexParentA] ||
                newChildA.ToString() == newGeneration[indexParentB])

            {
                GenLogManager.LogReBreed(newChildA.ToString());
                indexParentA -= 2;
                continue;
            }
            else if (newChildB.ToString() == newGeneration[indexParentA] ||
                    newChildB.ToString() == newGeneration[indexParentB])
            {
                GenLogManager.LogReBreed(newChildB.ToString());
                indexParentA -= 2;
                continue;
            }

            newGeneration.Add(newChildA.ToString());
            newGeneration.Add(newChildB.ToString());
        }

        newGeneration.Add(RandomizeIndividual());

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