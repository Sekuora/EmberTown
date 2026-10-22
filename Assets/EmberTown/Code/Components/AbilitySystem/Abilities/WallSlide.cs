using System.Collections.Generic;
using UnityEngine;

public class WallSlide : Ability
{
    [Header("Wall Slide Settings")]
    [SerializeField]
    private float wallSlideMaxSpeed = 3f;

    [SerializeField]
    private float wallSlideGravityMultiplier = 0.333f;

    [SerializeField]
    private float wallSlideHorizontalSpeed = 4f;

    [SerializeField]
    private float wallStickForce = 0.2f;

    [SerializeField]
    private float wallSlideDetectionDistance = 0.35f;

    public float DetectionDistance => wallSlideDetectionDistance;

    private WallCheck wallCheck;
    private Camera playerCamera;
    private Vector3 lastWallNormal;

    public Vector3 WallNormal => lastWallNormal;

    protected override void InitializeConditions()
    {
        if (Conditions == null)
        {
            Conditions = new List<AbilityCondition>();
        }

        Conditions.Add(new WallSlideCondition());

        if (!InterruptorAbilities.Contains(typeof(Jump).Name))
        {
            InterruptorAbilities.Add(typeof(Jump).Name);
        }

        if (!InterruptorAbilities.Contains(typeof(Dash).Name))
        {
            InterruptorAbilities.Add(typeof(Dash).Name);
        }

        if (!InterruptorAbilities.Contains(typeof(WallJump).Name))
        {
            InterruptorAbilities.Add(typeof(WallJump).Name);
        }


    }

    private void Start()
    {
        wallCheck = GetComponent<WallCheck>();
        FindPlayerCamera();
    }

    private void FindPlayerCamera()
    {
        if (playerCamera) return;

        var camGO = CinematicSubsystem.Call() ? CinematicSubsystem.Call().FindCamera("PlayerFLCamera") : null;
        if (camGO)
        {
            playerCamera = camGO.GetComponent<Camera>();
        }
        if (!playerCamera)
        {
            playerCamera = Camera.main;
        }
    }

    public Vector3 GetInputMoveDirection()
    {
        if (input == null || input.MoveValue.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        FindPlayerCamera();

        if (playerCamera != null)
        {
            Vector3 camFwd = playerCamera.transform.forward;
            Vector3 camRight = playerCamera.transform.right;
            camFwd.y = 0f;
            camRight.y = 0f;
            camFwd.Normalize();
            camRight.Normalize();
            return (camRight * input.MoveValue.x + camFwd * input.MoveValue.y);
        }

        return (transform.forward * input.MoveValue.y + transform.right * input.MoveValue.x);
    }

    private void Update()
    {
        if (AbilityState == State.Active)
        {
            WallSlideAction();
        }
        else
        {
            if (CheckCanUse())
            {
                abilityController.TryUseAbility(this);
            }
        }
    }

    private void WallSlideAction()
    {
        if (!wallCheck) wallCheck = GetComponent<WallCheck>();

        // If touched the ground, exit wall slide
        if (controller.isGrounded)
        {
            ExitWallSlide();
            return;
        }

        Vector3 inputDir = GetInputMoveDirection();

        // Cast direction: into the wall we're sliding on, or input/forward
        Vector3 castDir = lastWallNormal != Vector3.zero ? -lastWallNormal : (inputDir.sqrMagnitude > 0.001f ? inputDir : transform.forward);

        if (!wallCheck || !wallCheck.CheckSlideWall(castDir, out RaycastHit hit, wallSlideDetectionDistance))
        {
            ExitWallSlide();
            return;
        }

        lastWallNormal = hit.normal;

        // Align character rotation straight into the wall
        Vector3 faceDir = -hit.normal;
        faceDir.y = 0f;
        if (faceDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(faceDir);
        }

        // Detach if player pulls strongly away from wall
        if (inputDir.sqrMagnitude > 0.01f && Vector3.Dot(inputDir, hit.normal) > 0.5f)
        {
            ExitWallSlide();
            return;
        }

        // Maintain state
        rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.WallSlide;

        // Apply slide gravity capped at wallSlideMaxSpeed
        float slideGravity = Mathf.Abs(data.Gravity) * wallSlideGravityMultiplier;
        rData.VerticalDirection = Mathf.MoveTowards(rData.VerticalDirection, -wallSlideMaxSpeed, slideGravity * Time.deltaTime);

        // Horizontal control: project input onto wall plane, add gentle stick inward
        Vector3 tangentMove = Vector3.ProjectOnPlane(inputDir, hit.normal);
        float hSpeed = wallSlideHorizontalSpeed > 0f ? wallSlideHorizontalSpeed : data.WalkAcceleration;
        rData.HorizontalDirection = (tangentMove * hSpeed) - (hit.normal * wallStickForce);
    }

    private void ExitWallSlide()
    {
        lastWallNormal = Vector3.zero;

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

    protected override void UseAbility()
    {
        rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.WallSlide;
        rData.VerticalDirection = Mathf.Min(rData.VerticalDirection, 0f);

        Vector3 initDir = GetInputMoveDirection();
        if (initDir.sqrMagnitude <= 0.001f) initDir = transform.forward;
        if (wallCheck && wallCheck.CheckSlideWall(initDir, out RaycastHit hit, wallSlideDetectionDistance))
        {
            lastWallNormal = hit.normal;
        }

        WallSlideAction();
    }

    protected override void WhenInterrupted()
    {
        lastWallNormal = Vector3.zero;

        if (controller.isGrounded)
        {
            rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Ground;
        }
        else
        {
            rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Air;
        }
    }
}
