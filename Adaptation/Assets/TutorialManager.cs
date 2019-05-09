using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum TutorialPhase { First, UI, Melee, Ranged, Block, Dash, Targeting, Last }

public class TutorialManager : MonoBehaviour
{
    [SerializeField] TutorialPhase currentPhase;

    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject enemyPrefab;
    EnemyHealth enemyHealth;

    [SerializeField] GameObject[] generalUI;

    [SerializeField] GameObject[] firstInstructions;

    [SerializeField] GameObject blockInstructions;

    [SerializeField] GameObject dashInstructions;

    [SerializeField] GameObject[] uiInstructions;

    [SerializeField] GameObject meleeInstructions;

    [SerializeField] GameObject[] rangedInstructions;

    [SerializeField] GameObject[] targetingInstructions;

    [SerializeField] GameObject gratzInstructions;

    [SerializeField] GameObject lastInstructions;

    PlayerHealth playerHealth;
    int currentInstruction;

    void Start()
    {
        currentInstruction = 0;

        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GenerationManager.Tutorial = false;
            SceneManager.LoadScene(5);
        }
    }

    void Update()
    {
        if (playerHealth.IsDead)
        {
            playerHealth.ResetHealth();
        }

        UpdateTutorial1();
        UpdateTutorial2();
        UpdateTutorial3();
        UpdateTutorial4();
        UpdateTutorial5();
    }

    void UpdateTutorial1()
    {
        switch (currentPhase)
        {
            case TutorialPhase.First:

                firstInstructions[currentInstruction].SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    firstInstructions[currentInstruction].SetActive(false);
                    currentInstruction++;

                    if (currentInstruction < firstInstructions.Length)
                    {
                        firstInstructions[currentInstruction].SetActive(true);
                    }
                    else
                    {
                        currentInstruction = 0;

                        SceneManager.LoadScene(1);
                    }
                }
                break;
        }
    }

    void UpdateTutorial2()
    {
        switch (currentPhase)
        {
            case TutorialPhase.Block:

                if (currentInstruction == 0)
                    blockInstructions.SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    if (currentInstruction == 0)
                    {
                        currentInstruction++;
                        blockInstructions.SetActive(false);
                    }
                    else
                    {
                        SceneManager.LoadScene(2);
                    }
                }
                break;
        }
    }

    void UpdateTutorial3()
    {
        switch (currentPhase)
        {
            case TutorialPhase.Dash:

                if (currentInstruction == 0)
                    dashInstructions.SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    if (currentInstruction == 0)
                    {
                        currentInstruction++;
                        dashInstructions.SetActive(false);
                    }
                    else
                    {
                        for (int i = 0; i < generalUI.Length; i++)
                        {
                            generalUI[i].SetActive(true);
                        }

                        currentInstruction = 0;
                        currentPhase = TutorialPhase.UI;
                    }
                }
                break;
            case TutorialPhase.UI:

                Time.timeScale = 0;

                uiInstructions[currentInstruction].SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    uiInstructions[currentInstruction].SetActive(false);
                    currentInstruction++;

                    if (currentInstruction == uiInstructions.Length)
                    {
                        Time.timeScale = 1;
                        currentInstruction = 0;
                        currentPhase = TutorialPhase.Melee;
                    }
                    else
                    {
                        uiInstructions[currentInstruction].SetActive(true);
                    }
                }
                break;

            case TutorialPhase.Melee:

                if (currentInstruction == 0)
                    meleeInstructions.SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    if (currentInstruction == 0)
                    {
                        currentInstruction++;
                        meleeInstructions.SetActive(false);
                    }
                    else
                    {
                        SceneManager.LoadScene(3);
                    }
                }

                break;
        }
    }

    void UpdateTutorial4()
    {
        switch (currentPhase)
        {
            case TutorialPhase.Ranged:

                if (currentInstruction < rangedInstructions.Length)
                    rangedInstructions[currentInstruction].SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    if (currentInstruction < rangedInstructions.Length)
                    {
                        rangedInstructions[currentInstruction].SetActive(false);
                    }

                    currentInstruction++;

                    if (currentInstruction == rangedInstructions.Length)
                    {
                        for (int i = 0; i < rangedInstructions.Length; i++)
                        {
                            rangedInstructions[i].SetActive(false);
                        }
                    }
                    else if (currentInstruction > rangedInstructions.Length)
                    {
                        currentInstruction = 0;
                        currentPhase = TutorialPhase.Targeting;
                    }
                }

                break;
            case TutorialPhase.Targeting:

                if (currentInstruction < targetingInstructions.Length)
                    targetingInstructions[currentInstruction].SetActive(true);

                if (Input.GetButtonDown("Confirm"))
                {
                    if (currentInstruction < targetingInstructions.Length)
                    {
                        targetingInstructions[currentInstruction].SetActive(false);
                    }

                    currentInstruction++;

                    if (currentInstruction == targetingInstructions.Length)
                    {
                        for (int i = 0; i < rangedInstructions.Length; i++)
                        {
                            targetingInstructions[i].SetActive(false);
                        }
                    }
                    else if (currentInstruction > targetingInstructions.Length && enemyHealth == null)
                    {
                        enemyHealth = Spawn().GetComponentInChildren<EnemyHealth>();
                    }
                }

                if (enemyHealth != null && enemyHealth.IsDead)
                {
                    gratzInstructions.SetActive(true);

                    if (Input.GetButtonDown("Confirm"))
                    {
                        gratzInstructions.SetActive(false);
                        currentInstruction = 0;

                        SceneManager.LoadScene(4);
                    }
                }
                break;
        }
    }

    void UpdateTutorial5()
    {
        switch (currentPhase)
        {
            case TutorialPhase.Last:

                if (currentInstruction == 0)
                {
                    enemyHealth = Spawn().GetComponentInChildren<EnemyHealth>();
                    currentInstruction++;
                }

                if (enemyHealth != null && enemyHealth.IsDead)
                {
                    lastInstructions.SetActive(true);

                    if (Input.GetButtonDown("Confirm"))
                    {
                        GenerationManager.Tutorial = false;
                        SceneManager.LoadScene(5);
                    }
                }

                break;
        }
    }

    GameObject Spawn(int amount = 1)
    {
        return Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}