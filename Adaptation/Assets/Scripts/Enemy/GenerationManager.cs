using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using UnityEditor;

public class GenerationManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public PlayerHealth playerHealth;

    static List<Individual> individuals;
    public static int InstantiatedIndividuals { get; private set; }
    public static int DeadIndividuals { get; private set; }
    public static int CurrentGeneration { get; private set; }
    public static int TotalGenerations { get; private set; }

    readonly int generationSize = 8;
    readonly int individualSize = 4;

    System.Random rand = new System.Random();
    char[] possibleTraits = new char[] { 'h', 'm', 'd', 's' };
    char RandomTrait { get { return possibleTraits[rand.Next(0, possibleTraits.Length)]; } }

    bool gameStarted = false;

    static public string FetchTraits(out int individualNumber)
    {
        if (InstantiatedIndividuals >= individuals.Count)
            throw new System.Exception("exceeded generation amount");

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
    }

    void Start()
    {
        CurrentGeneration = 0;
        TotalGenerations = 10;
        GenLogManager.Initialize();
    }

    void Update()
    {
        if (playerHealth.currentHealth <= 0)
            return;

        if (!gameStarted)
        {
            RandomizeGeneration();
            individuals = GenFilesManager.LoadGeneration();
            ResetVariables();
            Spawn(generationSize);
            gameStarted = true;
        }
        else if (DeadIndividuals >= InstantiatedIndividuals
            && CurrentGeneration < TotalGenerations)
        {

            CreateNextGeneration();
            individuals = GenFilesManager.LoadGeneration();
            ResetVariables();
            Spawn(generationSize);
        }
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
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void RandomizeGeneration()
    {
        List<string> newGeneration = new List<string>();
        for (int i = 0; i < generationSize; i++)
        {
            StringBuilder newIndividual = new StringBuilder();
            for (int j = 0; j < individualSize; j++)
            {
                newIndividual.Append(RandomTrait);
            }
            newGeneration.Add(newIndividual.ToString());
        }
        GenFilesManager.SaveGeneration(newGeneration);
    }

    /// <summary>
    /// Breeds a new generation and saves it as a txt-file 
    /// </summary>
    void CreateNextGeneration()
    {
        GenFilesManager.SaveGeneration(BreedNewGeneration());
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
    /// Every child is sent here and considered for mutation, they have an 8% chance
    /// of getting a random trait replaced with a new random one
    /// </summary>
    /// <param name="child">A newly bred child for the next generation</param>
    void ConsiderMutation(StringBuilder child)
    {
        if (rand.Next(0, 101) <= 8)
        {
            /*Logging*/
            GenLogManager.LogMutatatedIndividual(child.ToString(), afterMutation: false);

            int placeToChange = rand.Next(0, child.Length);
            char newTrait;
            do { newTrait = RandomTrait; } while (newTrait == child[placeToChange]);
            child[placeToChange] = newTrait;

            /*Logging*/
            GenLogManager.LogMutatatedIndividual(child.ToString(), afterMutation: true);
        }
    }
}