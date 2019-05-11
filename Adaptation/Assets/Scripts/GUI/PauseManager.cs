using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField]
    GameObject pauseUI;

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

        if (active)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GenerationManager.ResetWave();

                active = false;
                Toggle();

                Time.timeScale = 1;
            }
        }
    }

    void Toggle()
    {
        pauseUI.SetActive(active);
    }
}