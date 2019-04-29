using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public enum LogType { Progress, Individual, ForGraphing }

static public class GenLogManager
{
    static StringBuilder progressLog, individualDetailsLog, logForGraphing;

    static string ProgressLogPath { get { return GenFilesManager.DirectoryPath + "ProgressLog.txt"; } }
    static string IndividualDetailsLogPath { get { return GenFilesManager.DirectoryPath + "IndividualDetailsLog_" + GenFilesManager.EnemyFilename; } }
    static string LogForGraphingPath { get { return GenFilesManager.DirectoryPath + "LogForGraphing.txt"; } }

    static public void Initialize()
    {
        progressLog = new StringBuilder();
        individualDetailsLog = new StringBuilder();
        logForGraphing = new StringBuilder();
    }

    static public void LogForGraphing(List<Individual> sortedList)
    {
        float worstValue, averageValue, bestValue;
        float sumValue = 0;
        foreach (Individual tempInd in sortedList)
        {
            sumValue += tempInd.FitnessScore;
        }
        averageValue = sumValue / sortedList.Count;
        bestValue = sortedList[0].FitnessScore;
        worstValue = sortedList[sortedList.Count - 1].FitnessScore;

        logForGraphing.AppendLine("");
        logForGraphing.AppendLine("-- Generation: " + GenerationManager.CurrentGeneration + " --");
        logForGraphing.AppendLine("");
        logForGraphing.AppendLine(" Worst value: " + worstValue);
        logForGraphing.AppendLine(" Average value: " + averageValue);
        logForGraphing.AppendLine(" Best value: " + bestValue);
        logForGraphing.AppendLine("");
    }

    static public void LogAfterSort(List<Individual> sortedList)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("------- Generation: " + GenerationManager.CurrentGeneration + " -------");
        progressLog.AppendLine("");
        progressLog.AppendLine("Individuals (sorted by fitness value): ");
        foreach (Individual tempInd in sortedList)
        {
            progressLog.AppendLine("\tScore: " + tempInd.FitnessScore + "\t\t\t Traits: " + tempInd.Traits);
        }
    }

    static public void LogParentsToBreed(List<string> parents)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("Best individuals (parents for next gen): ");
        foreach (string tempStr in parents)
        {
            progressLog.AppendLine("\tTraits: " + tempStr);
        }
    }

    static public void LogMutatatedIndividual(string child, bool afterMutation)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("Individual " + (afterMutation ? "after" : "before") + " mutation: " + child);
    }

    static public void LogNewGeneration(List<string> newGeneration)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("New generation: ");
        foreach (string tempStr in newGeneration)
        {
            progressLog.AppendLine("\tTraits: " + tempStr);
        }
        progressLog.AppendLine("");
    }

    static public void LogIndividual(int individualNumber, float totalDamage, float timeAlive, float DistanceToPlayer, float fitnessScore)
    {
        individualDetailsLog.AppendLine("");
        individualDetailsLog.AppendLine("Individual number: " + individualNumber);
        individualDetailsLog.AppendLine("Damage done to player: " + totalDamage);
        individualDetailsLog.AppendLine("Time spent alive: " + timeAlive);
        individualDetailsLog.AppendLine("Distance to player: " + DistanceToPlayer);
        individualDetailsLog.AppendLine("Fitness score: " + fitnessScore);
        individualDetailsLog.AppendLine("");
    }

    static public void SaveLog(LogType logType)
    {
        string path = "";
        string log = "";
        switch (logType)
        {
            case LogType.Progress:
                path = ProgressLogPath;
                log = progressLog.ToString();
                break;
            case LogType.Individual:
                path = IndividualDetailsLogPath;
                log = individualDetailsLog.ToString();
                break;
            case LogType.ForGraphing:
                path = LogForGraphingPath;
                log = logForGraphing.ToString();
                break;
        }
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(log);
        writer.Close();
        //AssetDatabase.ImportAsset(path);
        if (logType == LogType.Individual) individualDetailsLog = new StringBuilder();
    }
}