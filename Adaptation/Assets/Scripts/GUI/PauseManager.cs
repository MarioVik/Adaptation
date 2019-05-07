using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
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

            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
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