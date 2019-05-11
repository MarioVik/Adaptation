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
    static string IndividualDetailsLogPath { get { return GenFilesManager.DirectoryPath + "IndividualDetailsLog_" + GenFilesManager.GenerationFilename; } }
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
        logForGraphing.AppendLine(" Worst value: \t" + worstValue);
        logForGraphing.AppendLine(" Average value: \t" + averageValue);
        logForGraphing.AppendLine(" Best value: \t" + bestValue);
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

    static public void LogParentsToBreed(List<string> parentAttributes, List<string> parentFeatures)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("Best individuals (parents for next gen): ");
        for (int i = 0; i < parentAttributes.Count; i++)
        {
            progressLog.AppendLine("\tTraits: " + parentAttributes[i] + "|" + parentFeatures[i]);
        }
    }

    static public void LogReBreed(string child)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("Child is indentical to parent, REDO:\t\t" + child);
    }

    static public void LogMutatatedIndividual(string child, bool afterMutation)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("Individual " + (afterMutation ? "after" : "before") + " mutation: " + child);
    }

    static public void LogNewGeneration(List<string> newGeneration, bool randomized = false)
    {
        progressLog.AppendLine("");
        progressLog.AppendLine("New generation " + (randomized ? "SHUFFLED: " : ":"));
        foreach (string tempStr in newGeneration)
        {
            progressLog.AppendLine("\tTraits: " + tempStr);
        }
        progressLog.AppendLine("");
    }

    static public void LogIndividual(int individualNumber, string weaponType, float[] damage, float[] almostDamage, float[] distance, float[] time, float fitnessScore)
    {
        individualDetailsLog.AppendLine("");
        individualDetailsLog.AppendLine("Individual number: " + individualNumber);
        individualDetailsLog.AppendLine("Weapon type: " + weaponType);
        individualDetailsLog.AppendLine("Damage done to player: " + damage[0] + "\t\tModifier: " + damage[1] + "\t\tResult: " + damage[0] * damage[1]);
        individualDetailsLog.AppendLine("Almost-damage done to player: " + almostDamage[0] + "\t\tModifier: " + almostDamage[1] + "\t\tResult: " + almostDamage[0] * almostDamage[1]);
        individualDetailsLog.AppendLine("Average distance to player: " + distance[0] + "\t\tModifier: " + distance[1] + "\t\tResult: " + distance[0] * distance[1]);
        individualDetailsLog.AppendLine("Time spent alive: " + time[0] + "\t\tModifier: " + time[1] + "\t\tResult: " + time[0] * time[1]);
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
        if (logType == LogType.Individual) individualDetailsLog.Clear();
    }
}