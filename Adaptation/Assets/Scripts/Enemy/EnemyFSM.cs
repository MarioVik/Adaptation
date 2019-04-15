using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    [SerializeField]
    EnemyMeleeAttacking meleeAttack;
    [SerializeField]
    EnemyRangedAttacking rangedAttack;
    [SerializeField]
    EnemyBlocking block;
    [SerializeField]
    EnemyDashing dash;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
