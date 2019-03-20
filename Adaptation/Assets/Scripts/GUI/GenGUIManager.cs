using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenGUIManager : MonoBehaviour
{
    Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "Generation: " + GenerationManager.CurrentGeneration + " / " + GenerationManager.TotalGenerations;
    }
}