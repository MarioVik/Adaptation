using DM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    public bool MovementInput { get { return verticalInput != 0 || horizontalInput != 0; } }

    public Vector3 TargetDir { get; set; } // This is public so that the BlockingFeature can set the target of rotation when reflecting an attack 

    [Header("Initialize")]
    public GameObject activeModel;  // defines the current active model.
    public string[] attacks;  // array of normal attacks in string.

    float verticalInput;  // stores vertical input.
    float horizontalInput; // stores horizontal input.
    bool normalAttackInput;   //stores whether you do normal attack or not
    bool comboAttackInput;       //stores whether you combo or not
    bool featureInput;
    float dashVertical, dashHorizontal;

    public bool canMove;    //shows you can move or not

    public float moveAmount;    //shows the amount of movement from 0 to 1.
    public Vector3 moveDirection;     //stores the moving vector value of main character.

    Vector3 verticalMovement = Vector3.zero;
    Vector3 horizontalMovement = Vector3.zero;

    float moveSpeed = 9f;  //speed of running
    float rotateSpeed = 35f;   //speed of character's turning around    

    PlayerTraits traits;

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

    PlayerHealth health;

    float fixedDelta;        //stores Time.fixedDeltaTime
    Animator anim;      //for caching Animator component
    [HideInInspector]
    public Rigidbody rigid;     //for caching Rigidbody component
    CameraManager camManager;   //for caching CameraManager script

    AudioSource blockAudio;

    float knockBackDistance;
    readonly float shortKnockBack = 0.2f;
    readonly float longKnockBack = 1.0f;
    readonly float knockBackSpeed = 10f;
    Vector3 posBeforeKnock, knockDirection;
    bool beingKnocked;

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

    public void KnockBack(Vector3 direction, bool isShort)
    {
        beingKnocked = true;
        posBeforeKnock = transform.position;
        knockDirection = direction.normalized;

        if (isShort)
            knockBackDistance = shortKnockBack;
        else
            knockBackDistance = longKnockBack;
    }

    void Start() // Initiallizing camera, animator, rigidboy
    {
        camManager = CameraManager.singleton;
        camManager.Init(transform);
        SetupAnimator();
        rigid = GetComponent<Rigidbody>();

        traits = GetComponentInChildren<PlayerTraits>();

        hasMelee = traits.Melee;
        hasRanged = traits.Ranged;

        hasBlock = traits.Block;
        if (hasBlock)
            blockAudio = GetComponent<AudioSource>();

        hasDash = traits.Dash;
        if (hasDash)
            dashing = GetComponentInChildren<DashingFeature>();

        targeting = GetComponent<TargetingHandler>();

        blockAudio = GetComponent<AudioSource>();

        health = GetComponentInChildren<PlayerHealth>();
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
        if (health.IsDead)
            return;

        GetInput();     //getting control input from keyboard or joypad
        UpdateStates();   //Updating anything related to character's actions.         
    }

    void GetInput() //getting various inputs from keyboard or joypad.
    {
        verticalInput = Input.GetAxis("Vertical");    //for getting vertical input.
        horizontalInput = Input.GetAxis("Horizontal");    //for getting horizontal input.
        normalAttackInput = Input.GetButton("NormalAttack"); //for getting normal attack input.
        comboAttackInput = Input.GetButton("ComboAttack");    //for getting combo attack input.
        featureInput = Input.GetButton("Feature");
    }

    void UpdateStates() //updates character's various actions.
    {
        canMove = anim.GetBool("canMove");   //getting bool value from Animator's parameter named "canMove".
        

        if (Vector3.Distance(posBeforeKnock, transform.position) >= knockBackDistance)
            beingKnocked = false;


        UpdateAttack();

        float targetSpeed = moveSpeed;  //set run speed as target speed.

        //mixing camera rotation value to the character moving value.
        verticalMovement = verticalInput * camManager.transform.forward;
        horizontalMovement = horizontalInput * camManager.transform.right;

        if (hasBlock)
            UpdateBlock(ref targetSpeed);

        if (hasDash)
            UpdateDash(ref targetSpeed);

        if (hasDash && dashing.Dashing)
        {
            horizontalInput = dashHorizontal;
            verticalInput = dashVertical;
        }

        //This is for limiting values from 0 to 1.
        float m = Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput);

        moveAmount = Mathf.Clamp01(m);

        //multiplying target speed and move amount.
        moveDirection = ((verticalMovement + horizontalMovement).normalized) * (targetSpeed * moveAmount);
    }

    void UpdateAttack()
    {
        if ((hasMelee && !meleeAttacking.Attacking) || (hasRanged && !rangedAttacking.Attacking))
        {
            if ((normalAttackInput || comboAttackInput) && canMove) // I clicked for attack when I can move around.
            {
                if (!hasRanged && !hasMelee) throw new System.Exception("No attacks are available");
                if (hasRanged && hasMelee) throw new System.Exception("Error: both attacks are available");

                if (hasMelee && !hasRanged) meleeAttacking.Activate(comboAttackInput);
                if (hasRanged && !hasMelee) rangedAttacking.Activate(comboAttackInput);

                string targetAnim = attacks[comboAttackInput ? 1 : 0];
                anim.CrossFade(targetAnim, 0.0f); //play the target animation in 0.0 second.

                normalAttackInput = false;
                comboAttackInput = false;
            }
        }
    }

    void UpdateBlock(ref float targetSpeed)
    {
        if (blocking.Ready)
        {
            if (featureInput && canMove)
            {
                blocking.Activate();
                anim.SetBool("blocking", true);
            }
        }

        if (blocking.Blocking)
        {
            targetSpeed = 0;

            if (!featureInput)
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
            if (featureInput && canMove)
            {
                if (!MovementInput)
                    verticalInput = 1;

                dashVertical = verticalInput;
                dashHorizontal = horizontalInput;

                dashing.Activate();
                anim.SetBool("dashing", true);
            }
        }

        if (dashing.Dashing)
        {
            targetSpeed = dashing.DashSpeed;
            verticalMovement = dashVertical * camManager.transform.forward;
            horizontalMovement = dashHorizontal * camManager.transform.right;
        }

        if (dashing.DashStop)
        {
            anim.SetBool("dashing", false);
        }
    }

    void FixedTick(float d)
    {
        if (health.IsDead)
            return;

        float pDelta = d;

        if (beingKnocked)
        {
            rigid.velocity = knockDirection * knockBackSpeed;
        }
        else if (canMove || (hasDash && dashing.Dashing))
        {
            rigid.velocity = moveDirection;  //This controls the character movement.                  
        }

        //This can control character's rotation 
        //if (canMove)
        //{
        //Vector3 targetDir;
        if (!(hasBlock && blocking.Blocking))
        {
            if (targeting.ActiveTarget)
            {
                TargetDir = targeting.TargetDirection;
            }
            else
            {
                TargetDir = moveDirection;
            }
        }

        TargetDir = new Vector3(TargetDir.x, 0, TargetDir.z);

        if (TargetDir == Vector3.zero)
            TargetDir = transform.forward;

        Quaternion tr = Quaternion.LookRotation(TargetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, pDelta * rotateSpeed);
        transform.rotation = targetRotation;
        //}

        HandleMovementAnimations(); //update character's animations.
    }

    void HandleMovementAnimations()
    {
        anim.SetFloat("vertical", moveAmount, 0.2f, fixedDelta); //syncing moveAmount value to animator's "vertical" value.
    }
}