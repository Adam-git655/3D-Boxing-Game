using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    public GameObject Enemy;
    public GameObject MainCamera;
    public bool IsAttacking = false;
    private enum AttackHands
    {
        LeftHand = 0,
        RightHand,
        BothHands
    }
    private AttackHands currentAttackingHand;

    public SphereCollider LeftPunchCollider;
    public SphereCollider RightPunchCollider;

    public GameObject FloatingTextPrefab;
    private GameObject floatingText;

    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float RotationFactorPerFrame = 15.0f;

    //auto generated C# file for input system
    private InputSystem_Actions playerInput;

    private Animator animator;

    private Vector2 currentMovementInput;
    private bool isMovementPressed;

    private Rigidbody rb;

    private bool JabPressed = false;
    private float TimeSinceJab = 0f;

    private bool CrossPressed = false;
    private float TimeSinceCross = 0f;

    private bool HookPressed = false;
    private float TimeSinceHook = 0f;

    private bool DodgeRightPressed = false;
    private bool DodgeLeftPressed = false;
    private bool BlockPressed = false;

    [SerializeField] private float ComboWindow = 0.4f;


    private int IsWalkingHash;

    private int TriggerJabHash;
    private int TriggerCrossHash;
    private int TriggerHookHash;
    private int TriggerDodgeRightHash;
    private int TriggerDodgeLeftHash;
    private int TriggerBlockHash;

    private int StateJabCrossHash;
    private int StateCrossJabHash;
    private int StateHookHash;
    private int StateCrossHookHash;
    private int StateLeadJabHash;
    private int StateCrossPunchHash;

    void Awake()
    {
        playerInput = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        //Register move inputs
        playerInput.CharacterControls.Move.started += OnMovementInput;
        playerInput.CharacterControls.Move.canceled += OnMovementInput;
        playerInput.CharacterControls.Move.performed += OnMovementInput;

        //Increase performance by setting the string names of triggers, bools and states to hashes
        IsWalkingHash = Animator.StringToHash("IsWalking");

        TriggerJabHash = Animator.StringToHash("Trigger_Jab");
        TriggerCrossHash = Animator.StringToHash("Trigger_Cross");
        TriggerHookHash = Animator.StringToHash("Trigger_Hook");
        TriggerDodgeRightHash = Animator.StringToHash("Trigger_DodgeRight");
        TriggerDodgeLeftHash = Animator.StringToHash("Trigger_DodgeLeft");
        TriggerBlockHash = Animator.StringToHash("Trigger_Block");

        StateJabCrossHash = Animator.StringToHash("Jab Cross");
        StateCrossJabHash = Animator.StringToHash("Cross Jab");
        StateHookHash = Animator.StringToHash("Hook");
        StateCrossHookHash = Animator.StringToHash("Cross Hook");
        StateLeadJabHash = Animator.StringToHash("Lead Jab");
        StateCrossPunchHash = Animator.StringToHash("Cross Punch");
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        //Read Vector2 values of the move inputs, and assign them to a Vec2 variable
        currentMovementInput = context.ReadValue<Vector2>();

        //If vector2 values are 0,0 then no movement otherwise yes movement
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    void FixedUpdate()
    {
        if (MainCamera.GetComponent<CameraMain>().CanStartGame)
        {
            HandleRotation();

            //Move the player based on the current movement vector
            Vector3 currentMovement = new(currentMovementInput.x, 0, currentMovementInput.y);
            Vector3 targetPos = rb.position + currentMovement * MoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
        }
    }
    void Update()
    {
        if (MainCamera.GetComponent<CameraMain>().CanStartGame) 
            HandleAnimations();


        //Check the hand which is curently being used to attack and enable that hands colliders while disabling the other
        if (currentAttackingHand == AttackHands.LeftHand)
        {
            LeftPunchCollider.enabled = true;
            RightPunchCollider.enabled = false;
        }
        if (currentAttackingHand == AttackHands.RightHand)
        {
            LeftPunchCollider.enabled = false;
            RightPunchCollider.enabled = true;
        }
        if (currentAttackingHand == AttackHands.BothHands)
        {
            LeftPunchCollider.enabled = true;
            RightPunchCollider.enabled = true;
        }
    }

    void HandleRotation()
    {
        //Always look towards the enemy
        Vector3 positionToLookAt = Enemy.transform.position;
        Vector3 direction = positionToLookAt - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, RotationFactorPerFrame * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }

    void HandleAnimations()
    {
        //Play walking animation when moving
        animator.SetBool(IsWalkingHash, isMovementPressed);

        //SINGLE_MOVES

        //Play Jab animation on jab (LMB)
        SingleAttackMove(ref JabPressed, ref TimeSinceJab, playerInput.CharacterControls.Jab, TriggerJabHash, AttackHands.LeftHand, "Jab");

        //Play Cross animation on cross (RMB)
        SingleAttackMove(ref CrossPressed, ref TimeSinceCross, playerInput.CharacterControls.Cross, TriggerCrossHash, AttackHands.RightHand, "Cross");

        //Play Hook animation on hook (MMB)
        SingleAttackMove(ref HookPressed, ref TimeSinceHook, playerInput.CharacterControls.Hook, TriggerHookHash, AttackHands.LeftHand, "Hook");


        //Play Dodge Right animation on DodgeRight (E)
        DodgeMove(ref DodgeRightPressed, ref DodgeLeftPressed, playerInput.CharacterControls.DodgeRight, TriggerDodgeRightHash, "Dodge Right");

        //Play Dodge Left aniamtion on DodgeLeft (Q)
        DodgeMove(ref DodgeLeftPressed, ref DodgeRightPressed, playerInput.CharacterControls.DodgeLeft, TriggerDodgeLeftHash, "Dodge Left");

        //Play Block animation on Block (Spacebar)
        if (playerInput.CharacterControls.Block.WasPressedThisFrame())
        {
            if (!BlockPressed)
            {
                BlockPressed = true;
                animator.SetTrigger(TriggerBlockHash);
                InstantiateFloatingText("Block");
            }
        }


        //COMBO_MOVES

        //Do Jab-Cross or Jab-Hook combos if cross or hook is pressed after jab within the combo window
        ComboMove(ref JabPressed, ref TimeSinceJab, playerInput.CharacterControls.Cross, playerInput.CharacterControls.Hook, StateJabCrossHash, StateHookHash, AttackHands.BothHands, AttackHands.LeftHand, "Jab Cross", "Jab Hook");

        //Do Cross-Hook or Cross-Jab combos if hook or jab is pressed after cross within the combo window
        ComboMove(ref CrossPressed, ref TimeSinceCross, playerInput.CharacterControls.Hook, playerInput.CharacterControls.Jab, StateCrossHookHash, StateCrossJabHash, AttackHands.BothHands, AttackHands.BothHands, "Cross Hook", "Cross Jab");

        //Do Hook-Jab or Hook-Cross combos if jab or cross is pressed after hook within the combo window
        ComboMove(ref HookPressed, ref TimeSinceHook, playerInput.CharacterControls.Jab, playerInput.CharacterControls.Cross, StateLeadJabHash, StateCrossPunchHash, AttackHands.LeftHand, AttackHands.BothHands, "Hook Jab", "Hook Cross");
   
    }

    void InstantiateFloatingText(string moveName)
    {
        //Instantiate floating text showing MOVENAMES
        Vector3 positionToInstantiateText = new(transform.position.x, transform.position.y + 2f, transform.position.z);
        floatingText = Instantiate(FloatingTextPrefab, positionToInstantiateText, Quaternion.identity);
        floatingText.GetComponent<TextMeshPro>().text = moveName;
    }

    void SingleAttackMove(ref bool pressedFlag, ref float timer, InputAction input, int triggerHash, AttackHands attackHand, string MoveName)
    {
        if (input.WasPressedThisFrame() && !JabPressed && !HookPressed && !CrossPressed && !IsAttacking)
        {
            pressedFlag = true; //Set The move Pressed to true (Eg:-JabPressed = true)
            IsAttacking = true; 
            currentAttackingHand = attackHand; //Set the attacking hand for the move
            timer = 0f; //Set the timer to check for combos after the intial move to 0 (Eg:- TimeSinceJab = 0)
            animator.SetTrigger(triggerHash); //Set the trigger for the animation (Eg:- Trigger_Jab)
            InstantiateFloatingText(MoveName);
        }
    }

    void DodgeMove(ref bool currentDodgePressedFlag, ref bool otherDodgePressedFlag,  InputAction input, int triggerHash, string MoveName)
    {
        if (input.WasPressedThisFrame() && !otherDodgePressedFlag)
        {
            currentDodgePressedFlag = true; // Set the current dodge pressed to true (Eg:- DodgeRight = true)
            animator.SetTrigger(triggerHash); //Set the trigger for the animation(eg:- Trigger_DodgeRight)
            InstantiateFloatingText(MoveName);
        }
    }

    void ComboMove(ref bool comboStartPressedFlag, ref float timer, InputAction comboInput1, InputAction comboInput2, int comboState1Hash, int comboState2Hash, AttackHands attackHandCombo1, AttackHands attackHandCombo2, string Combo1Name, string Combo2Name)
    {
        if (comboStartPressedFlag)
        {
            timer += Time.deltaTime; //Increase the timer in which you can check for combos by deltaTime (Eg:- TimeSinceJab += Time.deltaTime)

            if (comboInput1.WasPressedThisFrame())
            {
                if (timer <= ComboWindow) // If the second input was pressed before the timer reaches 0.4s, then trigger the combo (Eg:- Jab-Cross)
                {
                    currentAttackingHand = attackHandCombo1; //Set the attacking Hand(s) for the combo
                    animator.CrossFade(comboState1Hash, 0.1f); //CrossFade to the combo animation
                    InstantiateFloatingText(Combo1Name);
                    comboStartPressedFlag = false; //Exit by turning the flag off
                }
            }

            if (comboInput2.WasPressedThisFrame())
            {
                if (timer <= ComboWindow)
                {
                    currentAttackingHand = attackHandCombo2;
                    animator.CrossFade(comboState2Hash, 0.1f);
                    InstantiateFloatingText(Combo2Name);
                    comboStartPressedFlag = false;
                }
            }

            if (timer > ComboWindow) //If unable to hit second move within 0.4s, then exit by turning the flag off
            {
                comboStartPressedFlag = false;
            }
        }
    }

    public void OnAttackEnd()
    {
        IsAttacking = false;
    }

    public void OnDodgeRightAnimEnd()
    {
        DodgeRightPressed = false;
    }
    public void OnDodgeLeftAnimEnd()
    {
        DodgeLeftPressed = false;
    }

    public void OnBlockAnimEnd()
    {
        BlockPressed = false;
    }

    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
