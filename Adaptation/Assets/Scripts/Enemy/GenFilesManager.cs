﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

static public class GenFilesManager
{
    //static public string DirectoryPath { get { return @"C:\Users\vikto\OneDrive\År3\Examensarbete\Adaptation\Adaptation\Assets\Generations\"; } }
    static public string DirectoryPath { get { return @"Assets\Generations\"; } }
    static public string Filename { get { return "generation_" + GenerationManager.CurrentGeneration + ".txt"; } }
    static public string Filepath { get { return DirectoryPath + Filename; } }

    static public void SaveGeneration(List<string> genToSave)
    {
        StreamWriter writer = new StreamWriter(Filepath, false);
        foreach (string tempString in genToSave)
        {
            writer.WriteLine(tempString);
        }
        writer.Close();
        AssetDatabase.ImportAsset(Filepath);
        //TextAsset asset = (TextAsset)Resources.Load(Filename);
    }

    static public List<Individual> LoadGeneration()
    {
        StreamReader reader = new StreamReader(Filepath);
        List<Individual> individuals = new List<Individual>();

        while (!reader.EndOfStream)
        {
            individuals.Add(new Individual(reader.ReadLine()));
        }
        reader.Close();
        return individuals;
    }
}