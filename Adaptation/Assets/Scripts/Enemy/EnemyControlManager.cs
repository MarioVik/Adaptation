﻿using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControlManager : MonoBehaviour
{
    public bool Dead { get; set; }

    [Header("Initialize")]
    public GameObject activeModel;  // defines the current active model.
    public string[] attacks;  // array of normal attacks in string.


    [Header("Inputs")]
    public float vertical;  // stores vertical input.
    public float horizontal; // stores horizontal input.
    public float moveAmount;    //shows the amount of movement from 0 to 1.
    public Vector3 moveDir;     //stores the moving vector value of main character.

    Vector3 veritcalMovement = Vector3.zero;
    Vector3 horizontalMovement = Vector3.zero;

    //[Header("Stats")]
    float moveSpeed = 6f;  //speed of running
    float sprintSpeed = 9f;  //speed of sprinting(double time of running)
    float rotateSpeed = 60f;   //speed of character's turning around

    [Header("FeatureBehaviours")]
    [SerializeField]
    EnemyMeleeAttacking enemyMeleeAttacking;
    [SerializeField]
    EnemyRangedAttacking enemyRangedAttacking;
    bool hasMelee;
    bool hasRanged;

    [SerializeField]
    EnemyBlocking enemyBlocking;
    EnemyDashing enemyDashing;
    bool hasBlock;
    bool hasDash;

    [Header("States")]
    public bool sprint;     //shows you are sprinting or not.

    [HideInInspector]
    public bool normalAttack;   //stores whether you do normal attack or not
    [HideInInspector]
    public bool comboAttack;       //stores whether you combo or not
    public bool canMove;    //shows you can move or not

    float fixedDelta;        //stores Time.fixedDeltaTime
    float delta;
    Animator anim;      //for caching Animator component
    [HideInInspector]
    public Rigidbody rigid;     //for caching Rigidbody component

    bool stopped;

    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    NavMeshAgent navAgent;



    public void IncreaseMovementSpeed(float increase)
    {
        moveSpeed += increase;
        sprintSpeed += increase;
    }

    void Start() // Initiallizing camera, animator, rigidboy
    {
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();

        hasMelee = enemyMeleeAttacking.gameObject.activeSelf;
        hasRanged = enemyRangedAttacking.gameObject.activeSelf;

        hasBlock = enemyBlocking.gameObject.activeSelf;
        enemyDashing = GetComponentInChildren<EnemyDashing>();
        hasDash = enemyDashing.isActiveAndEnabled;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    void SetupAnimator()//Setting up Animator component in the hierarchy.
    {
        if (activeModel == null)
        {
            anim = GetComponentInChildren<Animator>();//Find animator component in the children hierarchy.
            if (anim == null)
            {
                Debug.Log("No model");
            }
            else
            {
                activeModel = anim.gameObject; //save this gameobject as active model.
            }
        }

        if (anim == null)
            anim = activeModel.GetComponent<Animator>();
    }

    void FixedUpdate() //Since this is physics based controller, you have to use FixedUpdate.
    {
        fixedDelta = Time.fixedDeltaTime;    //storing the last frame updated time.             

        FixedTick(fixedDelta);   //update anything related to character moving.
    }

    void Update()
    {
        if (Dead)
            return;

        if (Input.GetKey(KeyCode.O))
            stopped = !stopped;

        if (stopped)
        {
            navAgent.isStopped = true;
            return;
        }

        if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            navAgent.SetDestination(player.position);
        }
        else
        {
            navAgent.enabled = false;
        }

        GetInput();     //getting control input from keyboard or joypad
        UpdateStates();   //Updating anything related to character's actions.         
    }


    void GetInput() //getting various inputs from keyboard or joypad.
    {
        vertical = navAgent.desiredVelocity.z;    //for getting vertical input.
        horizontal = navAgent.desiredVelocity.x;    //for getting horizontal input.
        //sprint = true; /*Input.GetButton("SprintInput");*/      //for getting sprint input.
        //normalAttack = Input.GetButtonDown("NormalAttack"); //for getting normal attack input.
        //comboAttack = Input.GetButtonDown("ComboAttack");    //for getting combo attack input.
    }


    void UpdateStates() //updates character's various actions.
    {
        canMove = anim.GetBool("canMove");   //getting bool value from Animator's parameter named "canMove".          

        if (hasDash)
        {
            if (enemyDashing.DashStart && canMove)
            {
                anim.SetBool("dashing", true);
            }
            else if (enemyDashing.DashStop)
            {
                anim.SetBool("dashing", false);
            }
        }

        if (hasBlock)
        {
            if (enemyBlocking.BlockStart && canMove)
            {
                //anim.CrossFade("Block", 0.0f);
                anim.SetBool("blocking", true);
            }
            else if (enemyBlocking.BlockStop)
            {
                anim.SetBool("blocking", false);
            }
        }

        float targetSpeed = moveSpeed;  //set run speed as target speed.

        if (sprint)
        {
            targetSpeed = sprintSpeed;    //set sprint speed as target speed.            
        }

        if (enemyBlocking.Blocking)
        {
            targetSpeed = 0;
        }

        if (enemyDashing.Dashing)
        {
            targetSpeed = enemyDashing.DashSpeed;
        }


        if (!enemyDashing.Dashing
            || (enemyDashing.DashStart && enemyDashing.Dashing))
        {
            //veritcalMovement = vertical * camManager.transform.forward;
            //horizontalMovement = horizontal * camManager.transform.right;
        }

        //multiplying target speed and move amount.
        moveDir = ((veritcalMovement + horizontalMovement).normalized) * (targetSpeed * moveAmount);

        //This is for isolating y velocity from the character control. 
        moveDir.y = rigid.velocity.y;

        //This is for limiting values from 0 to 1.
        float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        moveAmount = Mathf.Clamp01(m);

        if ((normalAttack || comboAttack) && canMove) // I clicked for normal attack when I can move around.
        {
            if (hasMelee && !hasRanged) enemyMeleeAttacking.Attack(comboAttack);
            if (hasRanged && !hasMelee) enemyRangedAttacking.Attack(comboAttack);
            if (!hasRanged && !hasMelee) throw new System.Exception("No attacks are available");
            if (hasRanged && hasMelee) throw new System.Exception("Error: both attacks are available");

            string targetAnim;

            targetAnim = attacks[comboAttack ? 1 : 0];

            anim.CrossFade(targetAnim, 0.0f); //play the target animation in 0.1 second.                 

            normalAttack = false;
            comboAttack = false;
        }
    }

    void FixedTick(float d)
    {
        if (Dead)
            return;

        float pDelta = d;

        if (canMove)
        {
            rigid.velocity = moveDir;  //This controls the character movement.                  
        }

        //This can control character's rotation.
        if (canMove)
        {
            Vector3 targetDir = moveDir;

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, pDelta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;
        }

        HandleMovementAnimations(); //update character's animations.
    }

    void HandleMovementAnimations()
    {
        anim.SetBool("sprint", sprint);   //syncing sprint bool value to animator's "Sprint" value.           
        if (moveAmount == 0)
        {
            anim.SetBool("sprint", false);
        }

        anim.SetFloat("vertical", moveAmount, 0.2f, fixedDelta); //syncing moveAmount value to animator's "vertical" value.
    }

}
