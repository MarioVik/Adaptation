using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

static public class GenFilesManager
{
    //static public string DirectoryPath { get { return @"C:\Users\vikto\OneDrive\År3\Examensarbete\Adaptation\Adaptation\Assets\Generations\"; } }
    static public string DirectoryPath { get { return GenerationManager.RandomMode ? @"TestDataB\" : @"TestDataA\"; } }
    static public string PlayerDirectory { get { return @"PlayerData\"; } }
    static public string PlayerFilename { get { return "PlayerTraits.txt"; } }
    static public string PlayerFilepath { get { return PlayerDirectory + PlayerFilename; } }
    static public string GenerationFilename { get { return "Generation_" + GenerationManager.CurrentGeneration + ".txt"; } }
    static public string GenerationFilepath { get { return DirectoryPath + GenerationFilename; } }

    static public void SavePlayer(string playerTraits)
    {
        StreamWriter writer = new StreamWriter(PlayerFilepath);
        writer.WriteLine(playerTraits);
        writer.Close();
        //AssetDatabase.ImportAsset(PlayerFilepath);
    }

    static public void SaveGeneration(List<string> genToSave)
    {
        StreamWriter writer = new StreamWriter(GenerationFilepath, false);
        foreach (string tempString in genToSave)
        {
            writer.WriteLine(tempString);
        }
        writer.Close();
        //AssetDatabase.ImportAsset(EnemyFilepath);
        //TextAsset asset = (TextAsset)Resources.Load(Filename);
    }

    static public string LoadPlayer()
    {
        StreamReader reader = new StreamReader(PlayerFilepath);
        string playerTraits = reader.ReadLine();
        reader.Close();
        return playerTraits;
    }

    static public string LoadTutorial(int tutorial)
    {
        StreamReader reader = new StreamReader(PlayerDirectory + "Tutorial" + tutorial + ".txt");
        string playerTraits = reader.ReadLine();
        reader.Close();
        return playerTraits;
    }

    static public List<Individual> LoadGeneration()
    {
        StreamReader reader = new StreamReader(GenerationFilepath);
        List<Individual> individuals = new List<Individual>();

        while (!reader.EndOfStream)
        {
            individuals.Add(new Individual(reader.ReadLine()));
        }
        reader.Close();
        return individuals;
    }
}