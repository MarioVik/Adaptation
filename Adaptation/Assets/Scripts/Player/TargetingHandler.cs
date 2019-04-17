﻿using System.Collections;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
        position.y += 2;
        Quaternion rotation = enemies[targetIndex].transform.rotation;
        targetPointer.transform.SetPositionAndRotation(position, rotation);
    }

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
        // the function was called by dying enemy WHILE targeting was active
        // OR function was called by input activation
        if (enemies.Count > 0 &&
            ((calledByEnemy && ActiveTarget) || !calledByEnemy))
        {
            TargetClosest();
        }
        else
        {
            targetIndex = -1;
            targetPointer.SetActive(false);
        }
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
