using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyManager : MonoBehaviour
{
    [SerializeField]
    Text[] infoUI;
    [SerializeField]
    Color validColor;
    [SerializeField]
    Color invalidColor;

    MeshRenderer renderer;

    bool playerCentered;
    bool firstWave = true;

    public void NewWave(bool dead = false)
    {
        if (dead)
        {
            infoUI[2].enabled = true;
            playerCentered = true;
        }
        else
        {
            renderer.enabled = true;

            if (firstWave)
                infoUI[0].enabled = true;
            else
                infoUI[1].enabled = true;
        }
    }

    public void End()
    {
        foreach (Text uiElement in infoUI)
        {
            uiElement.enabled = false;
        }
    }

    void Start()
    {
        renderer = GetComponent<MeshRenderer>();

        renderer.material.color = invalidColor;

        for (int i = 0; i < infoUI.Length - 1; i++)
        {
            infoUI[i].color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, 255);
        }

        playerCentered = false;
    }

    void Update()
    {
        if (!GenerationManager.PlayerReady)
        {
            if (Input.GetButtonDown("Confirm") && playerCentered)
            {
                GenerationManager.PlayerReady = true;

                renderer.enabled = false;

                foreach (Text uiElement in infoUI)
                {
                    uiElement.enabled = false;
                }

                if (firstWave)
                    firstWave = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            renderer.material.color = validColor;

            for (int i = 0; i < infoUI.Length - 1; i++)
            {
                infoUI[i].color = new Color(validColor.r, validColor.g, validColor.b, 255);
            }

            playerCentered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            renderer.material.color = invalidColor;

            for (int i = 0; i < infoUI.Length - 1; i++)
            {
                infoUI[i].color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, 255);
            }

            playerCentered = false;
        }
    }
}