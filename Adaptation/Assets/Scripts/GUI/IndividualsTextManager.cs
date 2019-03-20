using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IndividualsTextManager : MonoBehaviour
{
    public static int killedIndividuals;
    Text text;



    void Awake()
    {
        text = GetComponent<Text>();
        killedIndividuals = 0;
    }

    void Update()
    {
        text.text = "Individuals killed: " + GenerationManager.DeadIndividuals + "/" + GenerationManager.InstantiatedIndividuals;
    }
}
