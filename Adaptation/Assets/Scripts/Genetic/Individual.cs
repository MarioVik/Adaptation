using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public float FitnessScore { get; set; }
    public string Traits { get; private set; }
    //int generation;

    public Individual(string traits/*, int generation*/)
    {
        Traits = traits;
        //this.generation = generation;
        FitnessScore = 0;
    }
}