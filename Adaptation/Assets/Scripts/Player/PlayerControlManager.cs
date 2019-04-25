using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    public bool Dead { get; set; }
    public bool MovementInput { get { return vertical != 0 || horizontal != 0; } }

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
    float rotateSpeed = 30f;   //speed of character's turning around    

    [Header("FeatureBehaviours")]
    [SerializeField]
    PlayerMeleeAttacking playerMeleeAttacking;
    [SerializeField]
    PlayerRangedAttacking playerRangedAttacking;
    bool hasMelee;
    bool hasRanged;
    TargetingHandler targeting;

    [SerializeField]
    PlayerBlocking playerBlocking;
    PlayerDashing playerDashing;
    bool hasBlock;
    bool hasDash;


    [Header("States")]
    public bool sprint;     //shows you are sprinting or not.
                            //[HideInInspector]
                            //public bool jump;       //stores whether you jump or not
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
    CameraManager camManager;   //for caching CameraManager script

    public void IncreaseMovementSpeed(float increase)
    {
        moveSpeed += increase;
        sprintSpeed += increase;
    }

    void Start() // Initiallizing camera, animator, rigidboy
    {
        camManager = CameraManager.singleton;
        camManager.Init(this.transform);
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();

        hasMelee = playerMeleeAttacking.gameObject.activeSelf;
        hasRanged = playerRangedAttacking.gameObject.activeSelf;
        targeting = GetComponent<TargetingHandler>();

        hasBlock = playerBlocking.gameObject.activeSelf;
        playerDashing = GetComponentInChildren<PlayerDashing>();
        hasDash = playerDashing.isActiveAndEnabled;
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
        camManager.FixedTick(fixedDelta);     //update anything related to camera moving.       
    }

    void Update()
    {
        if (Dead)
            return;

        GetInput();     //getting control input from keyboard or joypad
        UpdateStates();   //Updating anything related to character's actions.         
    }


    void GetInput() //getting various inputs from keyboard or joypad.
    {
        vertical = Input.GetAxis("Vertical");    //for getting vertical input.
        horizontal = Input.GetAxis("Horizontal");    //for getting horizontal input.
        sprint = true; /*Input.GetButton("SprintInput");*/      //for getting sprint input.
        normalAttack = Input.GetButtonDown("NormalAttack"); //for getting normal attack input.
        comboAttack = Input.GetButtonDown("ComboAttack");    //for getting combo attack input.
    }


    void UpdateStates() //updates character's various actions.
    {
        canMove = anim.GetBool("canMove");   //getting bool value from Animator's parameter named "canMove".          

        if (hasDash)
        {
            if (playerDashing.DashStart && canMove)
            {
                anim.SetBool("dashing", true);
            }
            else if (playerDashing.DashStop)
            {
                anim.SetBool("dashing", false);
            }
        }

        if (hasBlock)
        {
            if (playerBlocking.BlockStart && canMove)
            {
                //anim.CrossFade("Block", 0.0f);
                anim.SetBool("blocking", true);
            }
            else if (playerBlocking.BlockStop)
            {
                anim.SetBool("blocking", false);
            }
        }

        float targetSpeed = moveSpeed;  //set run speed as target speed.

        if (sprint)
        {
            targetSpeed = sprintSpeed;    //set sprint speed as target speed.            
        }

        if (playerBlocking.Blocking)
        {
            targetSpeed = 0;
        }

        if (playerDashing.Dashing)
        {
            targetSpeed = playerDashing.DashSpeed;
        }


        if (!playerDashing.Dashing
            || (playerDashing.DashStart && playerDashing.Dashing))
        {
            //mixing camera rotation value to the character moving value.
            veritcalMovement = vertical * camManager.transform.forward;
            horizontalMovement = horizontal * camManager.transform.right;
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
            if (hasMelee && !hasRanged) playerMeleeAttacking.Attack(comboAttack);
            if (hasRanged && !hasMelee) playerRangedAttacking.Attack(comboAttack);
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
            Vector3 targetDir;
            if (targeting.ActiveTarget)
            {
                targetDir = targeting.TargetDirection;
            }
            else
            {
                targetDir = moveDir;
            }

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, pDelta/* * moveAmount */* rotateSpeed);
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
