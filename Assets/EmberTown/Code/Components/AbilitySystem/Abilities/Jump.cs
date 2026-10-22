using UnityEngine;

using System.Collections.Generic;

public class Jump : Ability
{

    // Jump Editor Data
    [Space(10)]
    [Header("Jump Settings")]

    [SerializeField]
    private float jumpTimeUp = 1.5f;

    [SerializeField]
    private float jumpTimeDown = 1.5f;


    [SerializeField]
    private float jumpDistance = 2.5f;

    [SerializeField]
    private float doubleJumpDistance = 3f;

    [SerializeField]
    private float tripleJumpDistance = 4f;


    [SerializeField]
    private float jumpComboWindow = 0.15f;

    [SerializeField]
    private float jumpSpeedBoost = 1f;

    // Jump Data
    private float jumpGravityUp;
    private float jumpGravityDown;
    private float jumpImpulse;
    private float doubleJumpGravityUp;
    private float doubleJumpImpulse;
    private float tripleJumpGravityUp;
    private float tripleJumpImpulse;
    private float currentJumpGravityUp;
    private bool wasGrounded = true;
    private bool useDashJump;
    private Timer jumpComboTimer;

    protected override void InitializeConditions()
    {
        if (Conditions == null)
        {
            Conditions = new List<AbilityCondition>();
        }

        Conditions.Add(new GroundedCondition());

        if (!InterruptorAbilities.Contains(typeof(WallSlide).Name))
        {
            InterruptorAbilities.Add(typeof(WallSlide).Name);
        }


        if (!InterruptorAbilities.Contains(typeof(Dash).Name))
        {
            InterruptorAbilities.Add(typeof(Dash).Name);
        }

    }

    public override bool CheckCanUse()
    {
        if (!abilityController) abilityController = GetComponent<AbilityController>();
        if (abilityController && abilityController.ActiveAbility is Dash)
        {
            return true;
        }

        return base.CheckCanUse();
    }


    private void Start()
    {
        // Jump Gravity Setup
        jumpGravityUp = 2 * jumpDistance / Mathf.Pow(jumpTimeUp, 2);

        jumpGravityDown = 2 * jumpDistance / Mathf.Pow(jumpTimeDown, 2);

        jumpImpulse = Mathf.Abs(jumpGravityUp) * jumpTimeUp;
        currentJumpGravityUp = jumpGravityUp;

        // Double Jump
        doubleJumpGravityUp = 2 * doubleJumpDistance / Mathf.Pow(jumpTimeUp, 2);
        doubleJumpImpulse = Mathf.Abs(doubleJumpGravityUp) * jumpTimeUp;

        // Triple Jump
        tripleJumpGravityUp = 2 * tripleJumpDistance / Mathf.Pow(jumpTimeUp, 2);
        tripleJumpImpulse = Mathf.Abs(tripleJumpGravityUp) * jumpTimeUp;


        // Jump Timer
        jumpComboTimer = new Timer("MultiJumpTimer", jumpComboWindow, WhenJumpTimerEnds);
    }

    private void WhenJumpTimerEnds()
    {
        if (rData != null)
        {
            rData.JumpCounter = 0;
        }
    }

    protected override void UseAbility()
    {
        if (useDashJump)
        {
            Effects.Add(new Effect(EffectsDB.BlockMovement, true));

            if (rData != null)
            {
                rData.JumpCounter = 1;
            }
        }

        wasGrounded = true;

        animator.SetBool("SingleJump", false);
        animator.SetBool("DoubleJump", false);
        animator.SetBool("TripleJump", false);
        animator.SetBool("Airborne", false);

        ComboJump();

        useDashJump = false;
    }

    private void Update()
    {
        jumpComboTimer.Tick(Time.deltaTime);

        bool dashIsActive = abilityController.ActiveAbility is Dash;

        bool wallJumpIsActive = abilityController.ActiveAbility is WallJump;

        if (!dashIsActive && !wallJumpIsActive)
        {
            JumpAction();
        }

        if (!input.JumpPressed) { return; }

        if (rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.Ground &&
            rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.WallPush &&
            !dashIsActive) { return; }

        useDashJump = dashIsActive;

        if (!abilityController.TryUseAbility(this))
        {
            useDashJump = false;
        }
    }


    private void JumpAction()
    {
        Debug.Log("Jump Method");

        /* 
        Distance Formula
        float jumpDistance = data.JumpTargetDistance = 0.5f * data.JumpMaxAcceleration * Mathf.Pow(data.JumpDuration, 2);

        Time Formula
        float jumpTime = Mathf.Sqrt(2 * Mathf.Abs(data.JumpTargetDistance) / data.JumpMaxAcceleration); 
        */

        bool isGrounded = rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.Ground ||
                          rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.WallPush;
        if (isGrounded && !wasGrounded && rData.VerticalDirection <= 0)
        {
            if (rData.JumpCounter > 0)
            {
                jumpComboTimer.ResetTime();
                jumpComboTimer.StartTimer();
            }

            if (AbilityState == State.Active)
            {
                StopAbility();
            }
        }

        // When player is grounded
        if (isGrounded)
        {
            animator.SetBool("SingleJump", false);
            animator.SetBool("DoubleJump", false);
            animator.SetBool("TripleJump", false);
            animator.SetBool("Airborne", false);

            if (rData.VerticalDirection < 0)
            {
                rData.VerticalDirection = data.Gravity;
            }
        }

        // When player is in air
        else
        {
            if (rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.WallSlide)
            {
                return;
            }
            // animator.SetBool("SingleJump", true);
            // Jump Gravity
            if (rData.VerticalDirection > 0)
            {
                rData.VerticalDirection -= currentJumpGravityUp * Time.deltaTime;
            }
            // Fall Gravity
            else
            {
                animator.SetBool("Airborne", true);
                rData.VerticalDirection -= jumpGravityDown * Time.deltaTime;
            }
        }


        wasGrounded = isGrounded;

    }

    private void ComboJump()
    {

        rData.JumpCounter++;

        switch (rData.JumpCounter)
        {

            // Normal Jump
            case 1:
                currentJumpGravityUp = jumpGravityUp;
                rData.VerticalDirection = jumpImpulse;
                animator.SetBool("SingleJump", true);

                Debug.Log("Single Jump");
                break;

            // Double Jump
            case 2:
                currentJumpGravityUp = doubleJumpGravityUp;
                rData.VerticalDirection = doubleJumpImpulse;
                animator.SetBool("DoubleJump", true);
                Debug.Log("Double Jump");
                break;

            // Triple Jump
            case 3:
                currentJumpGravityUp = tripleJumpGravityUp;
                rData.VerticalDirection = tripleJumpImpulse;

                Effects.Add(new Effect(EffectsDB.IncreaseSpeed, jumpSpeedBoost));

                animator.SetBool("TripleJump", true);
                Debug.Log("Triple Jump");
                rData.JumpCounter = 0;
                break;

            default:
                rData.JumpCounter = 1;
                currentJumpGravityUp = jumpGravityUp;
                rData.VerticalDirection = jumpImpulse;
                break;
        }

        jumpComboTimer.ResetTime();
    }

    protected override void WhenInterrupted()
    {
        if (interrupter is not WallSlide && rData != null)
        {
            rData.JumpCounter = 0;
        }

        animator.SetBool("SingleJump", false);
        animator.SetBool("DoubleJump", false);
        animator.SetBool("TripleJump", false);
    }


}
