using System.Collections.Generic;
using UnityEngine;

public class WallJump : Ability
{
    [Space(10)]
    [Header("Wall Jump Settings")]

    [Header("Vertical Settings")]
    [SerializeField]
    private float wallJumpHeight = 3.5f;

    [SerializeField]
    private float wallJumpTimeUp = 0.45f;

    [SerializeField]
    private float wallJumpTimeDown = 0.4f;

    [Header("Horizontal Settings")]
    [SerializeField]
    private float wallJumpHorizontalDistance = 4f;

    [SerializeField]
    private float wallJumpHorizontalTime = 0.45f;

    // Kinematics Data
    private float wallJumpGravityUp;
    private float wallJumpGravityDown;
    private float wallJumpImpulse;
    private float wallJumpHorizontalSpeed;

    private float currentVerticalVelocity;
    private Vector3 wallJumpDirection;

    private Timer wallJumpTimer;

    protected override void InitializeConditions()
    {
        if (Conditions == null)
        {
            Conditions = new List<AbilityCondition>();
        }

        Conditions.Add(new WallJumpCondition());

        if (!InterruptorAbilities.Contains(typeof(WallSlide).Name))
        {
            InterruptorAbilities.Add(typeof(WallSlide).Name);
        }

        if (!InterruptorAbilities.Contains(typeof(Dash).Name))
        {
            InterruptorAbilities.Add(typeof(Dash).Name);
        }
    }

    private void Start()
    {
        CalculateJumpPhysics();
        wallJumpTimer = new Timer("WallJumpTimer", wallJumpHorizontalTime, WhenWallJumpEnds);
    }

    private void CalculateJumpPhysics()
    {
        wallJumpGravityUp = 2 * wallJumpHeight / Mathf.Pow(wallJumpTimeUp, 2);
        wallJumpGravityDown = 2 * wallJumpHeight / Mathf.Pow(wallJumpTimeDown, 2);
        wallJumpImpulse = Mathf.Abs(wallJumpGravityUp) * wallJumpTimeUp;

        wallJumpHorizontalSpeed = wallJumpHorizontalDistance / wallJumpHorizontalTime;
    }

    private void Update()
    {
        if (wallJumpTimer != null)
        {
            wallJumpTimer.Tick(Time.deltaTime);
        }

        if (input.JumpPressed)
        {
            abilityController.TryUseAbility(this);
        }

        if (AbilityState == State.Active)
        {
            WallJumpAction();
        }
    }

    private void WallJumpAction()
    {
        if (controller.isGrounded)
        {
            rData.VerticalDirection = 0f;
            WhenWallJumpEnds();
            return;
        }

        // Apply upward gravity to decelerate vertical velocity
        currentVerticalVelocity -= wallJumpGravityUp * Time.deltaTime;
        rData.VerticalDirection = currentVerticalVelocity;

        // Maintain horizontal trajectory towards destination
        rData.HorizontalDirection = wallJumpDirection * wallJumpHorizontalSpeed;
    }

    protected override void UseAbility()
    {
        CalculateJumpPhysics();

        WallSlide wallSlide = GetComponent<WallSlide>();
        Vector3 wallNormal = Vector3.zero;

        if (wallSlide != null && wallSlide.WallNormal.sqrMagnitude > 0.001f)
        {
            wallNormal = wallSlide.WallNormal;
        }
        else
        {
            WallCheck wallCheck = GetComponent<WallCheck>();
            if (wallCheck != null && wallCheck.CheckSlideWall(transform.forward, out RaycastHit hit))
            {
                wallNormal = hit.normal;
            }
            else
            {
                wallNormal = -transform.forward;
            }
        }

        wallNormal.y = 0f;
        wallNormal.Normalize();
        wallJumpDirection = wallNormal;

        // 1. Rotate to face away from the wall (in the jump direction)
        if (wallJumpDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(wallJumpDirection);
        }

        // 2. Initial forces
        currentVerticalVelocity = wallJumpImpulse;
        rData.VerticalDirection = currentVerticalVelocity;
        rData.HorizontalDirection = wallJumpDirection * wallJumpHorizontalSpeed;

        // 3. Set MovementState to WallJump
        rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.WallJump;

        // 4. Advance jump counter for multi-jump combos
        rData.JumpCounter++;
        if (rData.JumpCounter > 3)
        {
            rData.JumpCounter = 1;
        }

        // 5. Animations
        animator.SetBool("Airborne", false);
        animator.SetBool("DoubleJump", false);
        animator.SetBool("TripleJump", false);
        animator.SetBool("SingleJump", true);

        // 6. Block movement input during jump trajectory towards destination
        Effects.Add(new Effect(EffectsDB.BlockMovement, true));

        // 6. Start Timer for jump destination duration
        wallJumpTimer = new Timer("WallJumpTimer", wallJumpHorizontalTime, WhenWallJumpEnds);
        wallJumpTimer.ResetTime();
        wallJumpTimer.StartTimer();
    }

    private void WhenWallJumpEnds()
    {
        if (AbilityState != State.Active) return;

        // Return to normal Air state (or Ground if landed)
        if (controller.isGrounded)
        {
            rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Ground;
        }
        else
        {
            rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Air;
        }

        StopAbility();
    }

    protected override void WhenInterrupted()
    {
        if (wallJumpTimer != null)
        {
            wallJumpTimer.PauseTimer();
        }

        Effects.Clear();
    }
}
