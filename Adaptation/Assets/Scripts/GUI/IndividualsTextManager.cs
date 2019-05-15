using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IndividualsTextManager : MonoBehaviour
{
    Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "Enemies killed: " + GenerationManager.DeadIndividuals + "/" + GenerationManager.GenerationSize
            /*+ "\nCurrently spawned: " + (GenerationManager.InstantiatedIndividuals - GenerationManager.DeadIndividuals) + "/" + GenerationManager.ConcurrentIndividuals*/;
    }
}