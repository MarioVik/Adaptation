using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IndividualsTextManager : MonoBehaviour
{
    Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = "Enemies killed: " + GenerationManager.DeadIndividuals + "/" + GenerationManager.InstantiatedIndividuals;
    }
}
