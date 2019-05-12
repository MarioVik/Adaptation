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
    [SerializeField]
    bool tutorial;
    public static bool Tutorial;

    [SerializeField]
    bool randomMode;

    static public bool PlayerReady { get; set; } = true;

    public GameObject enemyPrefab;
    static PlayerHealth playerHealth;
    PlayerReadyManager readyManager;

    [SerializeField]
    SpawnPointDetection[] spawnPoints;
    [SerializeField]
    GameObject endText;

    static List<Individual> individuals;
    public static int InstantiatedIndividuals { get; private set; }
    public static int DeadIndividuals { get; private set; }
    public static int CurrentGeneration { get; private set; }
    public static int TotalGenerations { get; } = 10;
    public static int GenerationSize { get; } = 9; // (GenerationSize - 1) must be dividable by permutations.Length. Aside for the one random individual, the first generation has an equal amount of every permutation.
    public static int ConcurrentIndividuals { get; } = 1;

    static readonly float spawnFrequency = 0f;
    static float spawnTimer = 0;

    public static int Attributes { get; } = 12;

    System.Random rand = new System.Random();
    char[] attributes = new char[] { 'h', 'd', 's', 'r', 'm' };
    char[] offensiveFeatures = new char[] { 'M', 'R' };
    char[] defensiveFeatures = new char[] { 'B', 'D' };
    string[] featurePermutations = { "MB"/*, "MD", "RB", "RD" */};

    char RandomAttribute { get { return attributes[rand.Next(0, attributes.Length)]; } }
    char RandomOffensiveFeature { get { return offensiveFeatures[rand.Next(0, offensiveFeatures.Length)]; } }
    char RandomDefensiveFeature { get { return defensiveFeatures[rand.Next(0, defensiveFeatures.Length)]; } }

    int mutationChance = 8;

    bool gameStarted = false;
    bool gameEnd = false;

    static public string FetchTraits(out int individualNumber)
    {
        if (Tutorial)
        {
            individualNumber = 0;
            return "hdrsm|MB";
        }

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
        if (tutorial)
        {
            Tutorial = tutorial;
            return;
        }
        else
        {
            Tutorial = tutorial;
        }

        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        readyManager = GameObject.FindGameObjectWithTag("ReadyArea").GetComponent<PlayerReadyManager>();

        CurrentGeneration = 1;
        GenLogManager.Initialize();
    }

    static public void ResetWave()
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
        if (tutorial)
            return;

        if (CurrentGeneration > TotalGenerations)
        {
            GenLogManager.LogForGraphing(individuals);
            GenLogManager.SaveLog(LogType.Individual);
            endText.SetActive(true);
            gameEnd = true;
        }

        if (gameEnd)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            return;
        }

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
            if (randomMode)
            {
                individuals = StringsToIndividuals(RandomizeGeneration());
            }
            else
            {
                individuals = StringsToIndividuals(CreateFirstGeneration());
            }

            ResetVariables();
            gameStarted = true;

            PlayerReady = false;
            readyManager.NewWave();
            return;
        }
        else if (InstantiatedIndividuals >= GenerationSize
            && DeadIndividuals >= GenerationSize
            && CurrentGeneration <= TotalGenerations)
        {
            if (randomMode)
            {
                individuals = StringsToIndividuals(RandomizeNextGeneration());
            }
            else
            {
                individuals = StringsToIndividuals(CreateNextGeneration());
            }

            ResetVariables();

            PlayerReady = false;
            readyManager.NewWave();
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

    static void ResetVariables()
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
        List<string> features = new List<string>();
        for (int i = 0; i < (GenerationSize - 1) / featurePermutations.Length; i++)
        {
            foreach (string permutation in featurePermutations)
            {
                features.Add(permutation);
            }
            //features.Add(permutations[0]);
            //features.Add(permutations[1]);
            //features.Add(permutations[2]);
            //features.Add(permutations[3]);
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

        for (int j = 0; j < Attributes; j++)
        {
            newAttributes.Append(RandomAttribute);
        }

        return newAttributes.ToString();
    }

    string RandomizeFeatures()
    {
        StringBuilder newFeatures = new StringBuilder();

        newFeatures.Append(RandomOffensiveFeature);
        newFeatures.Append(RandomDefensiveFeature);

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

            // Uniform crossover
            for (int indexParentTrait = 0; indexParentTrait < Attributes; indexParentTrait++)
            {
                int coinFlip = rand.Next(0, 2);
                if (coinFlip == 0)
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

            // Single point crossover
            //int crossoverPoint = rand.Next(1, attributes);
            //for (int indexParentTrait = 0; indexParentTrait < attributes; indexParentTrait++)
            //{
            //    if (indexParentTrait < crossoverPoint)
            //    {
            //        newChildA.Append(newAttributes[indexParentA][indexParentTrait]);
            //        newChildB.Append(newAttributes[indexParentB][indexParentTrait]);
            //    }
            //    else
            //    {
            //        newChildA.Append(newAttributes[indexParentB][indexParentTrait]);
            //        newChildB.Append(newAttributes[indexParentA][indexParentTrait]);
            //    }
            //}

            // Copy over the parent's features  to whichever child inherited the bigger part of the parent's genotype
            //if (crossoverPoint > Attributes / 2)
            //{
            //    newChildA.Append(newFeatures[indexParentA]);
            //    newChildB.Append(newFeatures[indexParentB]);
            //}
            //else
            //{
            //    newChildA.Append(newFeatures[indexParentB]);
            //    newChildB.Append(newFeatures[indexParentA]);
            //}

            ConsiderMutation(newChildA);
            ConsiderMutation(newChildB);

            newChildA.Append('|');
            newChildB.Append('|');

            int fifyfifty = rand.Next(0, 2);
            // Copy over the parent's features to a random child
            if (fifyfifty > 0)
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

        List<string> shuffledGeneration = newGeneration.OrderBy(x => UnityEngine.Random.value).ToList();

        /*Logging*/
        GenLogManager.LogNewGeneration(shuffledGeneration);
        /*Logging*/
        GenLogManager.SaveLog(LogType.Progress);

        individuals.Clear();
        CurrentGeneration++;

        return shuffledGeneration;
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

    List<string> RandomizeNextGeneration()
    {
        /*Logging details about last generation's individuals*/
        GenLogManager.SaveLog(LogType.Individual);

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