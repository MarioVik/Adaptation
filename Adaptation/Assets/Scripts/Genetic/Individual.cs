using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual
{
    public float FitnessScore { get; set; }
    public string Traits { get; private set; }
    //int number;

    public Individual(/*int number,*/ string traits)
    {
        //this.number = number;
        Traits = traits;
        FitnessScore = 0;
    }
}