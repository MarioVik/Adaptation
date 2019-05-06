using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    Text[] infoUI;
    [SerializeField]
    Color validColor;
    [SerializeField]
    Color invalidColor;

    MeshRenderer renderer;

    Transform player;
    bool playerCentered;

    bool firstWave = true;

    public void NewWave()
    {
        renderer.enabled = true;

        if (firstWave)
            infoUI[0].enabled = true;
        else
            infoUI[1].enabled = true;
    }

    void Start()
    {
        renderer = GetComponent<MeshRenderer>();

        player = GameObject.FindGameObjectWithTag("PlayerController").transform;
        player.SetPositionAndRotation(new Vector3(transform.position.x, 0, transform.position.z), transform.rotation);
        playerCentered = true;
    }

    void Update()
    {
        if (!GenerationManager.PlayerReady)
        {
            if (Input.GetButtonDown("Confirm") && playerCentered)
            {
                GenerationManager.PlayerReady = true;

                renderer.enabled = false;

                if (firstWave)
                    infoUI[0].enabled = false;
                else
                    infoUI[1].enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            renderer.material.color = validColor;

            foreach (Text text in infoUI)
            {
                text.color = new Color(validColor.r, validColor.g, validColor.b, 255);
            }

            playerCentered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            renderer.material.color = invalidColor;

            foreach (Text text in infoUI)
            {
                text.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, 255);
            }

            playerCentered = false;
        }
    }
}