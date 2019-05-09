using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingHandler : MonoBehaviour
{
    public bool ActiveTarget { get { return targetIndex != -1; } }
    public Vector3 TargetDirection { get; private set; }

    [SerializeField]
    GameObject targetPointer;

    List<EnemyHealth> enemies;
    int targetIndex = -1;

    float rotationSpeed = 50;

    public void UpdateEnemies(bool calledByEnemy = false)
    {
        enemies = new List<EnemyHealth>();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemyObjects)
        {
            if (!enemyObj.GetComponent<EnemyHealth>().IsDead)
            {
                enemies.Add(enemyObj.GetComponent<EnemyHealth>());
            }
        }

        // Only retarget if there are enemies left AND 
        // if function was called by input activation       OR      the function was called by dying enemy WHILE targeting was active 
        if (enemies.Count > 0 &&
            ((calledByEnemy && ActiveTarget) || !calledByEnemy))
        {
            TargetClosest();
        }
        else
        {
            Untarget();
        }
    }

    public void AddEnemy(EnemyHealth newEnemy)
    {
        if (enemies == null || enemies.Count == 0)
        {
            UpdateEnemies(true);
        }
        else
        {
            enemies.Add(newEnemy);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Target"))
        {
            if (!ActiveTarget)
            {
                targetPointer.SetActive(true);
                UpdateEnemies();
            }
            else
            {
                IterateTarget();
            }
        }
        else if (Input.GetButtonDown("Untarget"))
        {
            Untarget();
        }

        if (ActiveTarget)
        {
            UpdateTargetPointer();

            Vector3 direction = enemies[targetIndex].transform.position - transform.position;
            TargetDirection = direction.normalized;
        }
    }

    private void IterateTarget()
    {
        targetIndex++;
        if (targetIndex >= enemies.Count)
            targetIndex = 0;
    }

    private void UpdateTargetPointer()
    {
        Vector3 position = enemies[targetIndex].transform.position;
        position.y += 2f;

        targetPointer.transform.position = new Vector3(position.x, position.y, position.z);
        targetPointer.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void Untarget()
    {
        targetIndex = -1;
        targetPointer.SetActive(false);
    }

    private void TargetClosest()
    {
        float closestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (closestDistance > Vector3.Distance(transform.position, enemies[i].transform.position))
            {
                closestDistance = Vector3.Distance(transform.position, enemies[i].transform.position);
                targetIndex = i;
            }
        }
    }
}