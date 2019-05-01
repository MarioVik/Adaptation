using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterProjectileBehaviour : MonoBehaviour
{
    List<FiniteStateMachine> enemiesInContact;

    public void Clear()
    {
        foreach (FiniteStateMachine fsm in enemiesInContact)
            fsm.DecrementCollisions();
        enemiesInContact.Clear();
    }

    void Start()
    {
        enemiesInContact = new List<FiniteStateMachine>();
    }

    private bool InContact(FiniteStateMachine fsm)
    {
        for (int i = 0; i < enemiesInContact.Count; i++)
        {
            if (fsm.transform == enemiesInContact[i].transform)
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemyController")
        {
            FiniteStateMachine fsm = other.GetComponent<FiniteStateMachine>();

            if (!InContact(fsm))
            {
                fsm.IncrementColllisions();
                enemiesInContact.Add(fsm);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemyController")
        {
            FiniteStateMachine fsm = other.GetComponent<FiniteStateMachine>();

            for (int i = 0; i < enemiesInContact.Count; i++)
            {
                if (fsm.transform == enemiesInContact[i].transform)
                {
                    enemiesInContact[i].DecrementCollisions();
                    enemiesInContact.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
