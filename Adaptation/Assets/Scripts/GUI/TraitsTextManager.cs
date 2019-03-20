using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TraitsTextManager : MonoBehaviour
{
    Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        StringBuilder textFileText = new StringBuilder();
        textFileText.AppendLine("Current generation:");

        StreamReader reader = new StreamReader(GenFilesManager.Filepath);
        while (!reader.EndOfStream)
        {
            textFileText.AppendLine(reader.ReadLine());
        }
        reader.Close();

        text.text = textFileText.ToString();
    }
}
