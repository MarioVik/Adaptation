﻿using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControlManager : MonoBehaviour
{
    // All input is controlled by the Finite State Machine
    public float VerticalInput { get; set; }
    public float HorizontalInput { get; set; }
    public bool NormalAttackInput { get; set; }
    public bool ComboAttackInput { get; set; }
    public bool FeatureInput { get; set; } = false;
    public Vector3 DashDirection { get; set; }

    public bool MovementInput { get { return VerticalInput != 0 || HorizontalInput != 0; } }

    Transform playerTransform;

    [Header("Initialize")]
    public GameObject activeModel;  // defines the current active model.
    public string[] attacks;  // array of normal attacks in string.

    public bool canMove;    //shows you can move or not

    public float moveAmount;    //shows the amount of movement from 0 to 1.
    public Vector3 moveDirection;     //stores the moving vector value of main character.

    Vector3 verticalMovement = Vector3.zero;
    Vector3 horizontalMovement = Vector3.zero;

    float moveSpeed = 9f;  //speed of running
    float rotateSpeed = 60f;   //speed of character's turning around    


    EnemyTraits traits;

    [Header("FeatureBehaviours")]
    [SerializeField]
    MeleeAttackFeature meleeAttacking;
    [SerializeField]
    RangedAttackFeature rangedAttacking;
    [SerializeField]
    BlockingFeature blocking;
    DashingFeature dashing;

    bool hasMelee, hasRanged;
    bool hasBlock, hasDash;
    TargetingHandler targeting;

    EnemyHealth health;

    float fixedDelta;        //stores Time.fixedDeltaTime
    Animator anim;      //for caching Animator component
    [HideInInspector]
    public Rigidbody rigid;     //for caching Rigidbody component

    AudioSource blockAudio;

    public void IncreaseMovementSpeed(float increase) => moveSpeed += increase;

    public void GetHit()
    {
        if (hasMelee && meleeAttacking.Attacking)
        {
            meleeAttacking.Disable();
        }
        else if (hasRanged && rangedAttacking.Attacking)
        {
            rangedAttacking.Disable();
        }

        //if (hasBlock)
        //    if (blocking.Blocking)
        //        blockAudio.Play();

        anim.SetTrigger("hit");
    }

    void Start() // Initiallizing camera, animator, rigidboy
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        SetupAnimator();
        rigid = GetComponent<Rigidbody>();

        traits = GetComponentInChildren<EnemyTraits>();

        hasMelee = traits.Melee;
        hasRanged = traits.Ranged;

        hasBlock = traits.Block;
        if (hasBlock)
            blockAudio = GetComponent<AudioSource>();

        hasDash = traits.Dash;
        if (hasDash)
            dashing = GetComponentInChildren<DashingFeature>();

        health = GetComponentInChildren<EnemyHealth>();
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
        if (health.IsDead)
            return;

        UpdateStates();   //Updating anything related to character's actions.         

    }

    void UpdateStates() //updates character's various actions.
    {
        canMove = anim.GetBool("canMove");   //getting bool value from Animator's parameter named "canMove".

        UpdateAttack();

        float targetSpeed = moveSpeed;  //set run speed as target speed.

        //mixing camera rotation value to the character moving value.
        verticalMovement = VerticalInput * transform.forward;
        horizontalMovement = HorizontalInput * transform.right;

        if (hasBlock)
            UpdateBlock(ref targetSpeed);

        if (hasDash)
            UpdateDash(ref targetSpeed);

        //This is for limiting values from 0 to 1.
        float m;

        if (hasDash && dashing.Dashing)
        {
            m = Mathf.Abs(DashDirection.z) + Mathf.Abs(DashDirection.x);
            moveDirection = DashDirection;
        }
        else
        {
            m = Mathf.Abs(HorizontalInput) + Mathf.Abs(VerticalInput);
            moveDirection = verticalMovement + horizontalMovement;
        }

        moveAmount = Mathf.Clamp01(m);

        //multiplying target speed and move amount.
        moveDirection = (moveDirection.normalized) * (targetSpeed * moveAmount);
    }

    void UpdateAttack()
    {
        if ((hasMelee && !meleeAttacking.Attacking) || hasRanged && !rangedAttacking.Attacking)
        {
            if ((NormalAttackInput || ComboAttackInput) && canMove) // I clicked for attack when I can move around.
            {
                if (!hasRanged && !hasMelee) throw new System.Exception("No attacks are available");
                if (hasRanged && hasMelee) throw new System.Exception("Error: both attacks are available");

                if (hasMelee && !hasRanged) meleeAttacking.Activate(ComboAttackInput);
                if (hasRanged && !hasMelee) rangedAttacking.Activate(ComboAttackInput);

                string targetAnim = attacks[ComboAttackInput ? 1 : 0];
                anim.CrossFade(targetAnim, 0.0f); //play the target animation in 0.0 second.

                NormalAttackInput = false;
                ComboAttackInput = false;
            }
        }
    }

    void UpdateBlock(ref float targetSpeed)
    {
        if (blocking.Ready)
        {
            if (FeatureInput && canMove)
            {
                blocking.Activate();
                anim.SetBool("blocking", true);
            }
        }

        if (blocking.Blocking)
        {
            targetSpeed = 0;

            if (!FeatureInput)
            {
                blocking.Deactivate();
            }
        }

        if (blocking.BlockStop)
        {
            anim.SetBool("blocking", false);
        }
    }

    void UpdateDash(ref float targetSpeed)
    {
        if (dashing.Ready)
        {
            if (FeatureInput && canMove)
            {
                dashing.Activate();
                anim.SetBool("dashing", true);
            }
        }

        if (dashing.Dashing)
        {
            targetSpeed = dashing.DashSpeed;
        }


        if (dashing.DashStop)
        {
            targetSpeed = moveSpeed;
            anim.SetBool("dashing", false);
        }
    }

    Vector3 DirectionToPlayer { get { return (playerTransform.position - transform.position).normalized; } }

    void FixedTick(float d)
    {
        if (health.IsDead)
            return;

        float pDelta = d;

        if (hasDash && dashing.Dashing)
        {
            rigid.velocity = moveDirection;  //This controls the character movement.                  
        }
        //if (dashing.DashStop)
        //{
        //    rigid.velocity = Vector3.zero;
        //}

        //This can control character's rotation.
        if (canMove)
        {
            //Vector3 targetDir = moveDir;

            Vector3 targetDir = DirectionToPlayer;

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, pDelta * /*moveAmount **/ rotateSpeed);
            transform.rotation = targetRotation;
        }

        HandleMovementAnimations(); //update character's animations.
    }

    void HandleMovementAnimations()
    {
        anim.SetFloat("vertical", moveAmount, 0.2f, fixedDelta); //syncing moveAmount value to animator's "vertical" value.
    }
}