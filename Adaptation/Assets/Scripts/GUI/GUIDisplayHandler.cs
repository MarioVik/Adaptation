using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIDisplayHandler : MonoBehaviour
{
    [SerializeField]
    GameObject[] canvasElements;

    bool active;

    void Start()
    {
        active = false;
        Toggle();
    }

    void Update()
    {
        if (Input.GetButtonDown("GUI"))
        {
            active = !active;
            Toggle();
        }
        //else if (Input.GetButtonUp("GUI"))
        //{
        //    Toggle();
        //}
    }

    void Toggle()
    {
        foreach (GameObject element in canvasElements)
        {
            element.SetActive(active);
        }
    }
}