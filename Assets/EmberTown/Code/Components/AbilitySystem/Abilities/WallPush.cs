using System.Collections.Generic;
using UnityEngine;

public class WallPush : Ability
{
    [Header("Wall Push Settings")]
    [SerializeField]
    private float minSpeedMultiplier = 0.0f;

    [SerializeField]
    private float wallPushDetectionDistance = 1.5f;

    public float DetectionDistance => wallPushDetectionDistance;

    private WallCheck wallCheck;
    private Camera playerCamera;

    public float SpeedMultiplier { get; private set; } = 1f;

    protected override void InitializeConditions()
    {
        if (Conditions == null)
        {
            Conditions = new List<AbilityCondition>();
        }

        Conditions.Add(new WallPushCondition());

        if (!InterruptorAbilities.Contains(typeof(Jump).Name))
        {
            InterruptorAbilities.Add(typeof(Jump).Name);
        }

        if (!InterruptorAbilities.Contains(typeof(Dash).Name))
        {
            InterruptorAbilities.Add(typeof(Dash).Name);
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
            return (camRight * input.MoveValue.x + camFwd * input.MoveValue.y).normalized;
        }

        return transform.forward;
    }

    private void Update()
    {
        if (AbilityState == State.Active)
        {
            WallPushAction();
        }
        else
        {
            if (CheckCanUse())
            {
                abilityController.TryUseAbility(this);
            }
        }
    }

    private void WallPushAction()
    {
        if (!wallCheck) wallCheck = GetComponent<WallCheck>();

        if (!controller.isGrounded)
        {
            ExitWallPush();
            return;
        }

        Vector3 moveDir = GetInputMoveDirection();
        if (moveDir.sqrMagnitude <= 0.001f)
        {
            ExitWallPush();
            return;
        }

        float maxDist = wallPushDetectionDistance > 0f ? wallPushDetectionDistance : (wallCheck ? wallCheck.HeavyWallDistance : data.WallCollisionDistance);

        if (!wallCheck || !wallCheck.CheckHeavyWall(moveDir, out RaycastHit hit, maxDist))
        {
            ExitWallPush();
            return;
        }

        if (Vector3.Dot(moveDir, hit.normal) >= 0f)
        {
            ExitWallPush();
            return;
        }

        // Maintain state
        rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.WallPush;

        // Apply slowdown based on distance to wall
        float distanceToWall = hit.distance - controller.skinWidth;
        distanceToWall = Mathf.Max(0f, distanceToWall);
        float distanceRatio = Mathf.Clamp01(distanceToWall / maxDist);
        SpeedMultiplier = Mathf.Lerp(minSpeedMultiplier, 1.0f, distanceRatio);
    }

    private void ExitWallPush()
    {
        SpeedMultiplier = 1f;

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
        rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.WallPush;
        WallPushAction();
    }

    protected override void WhenInterrupted()
    {
        SpeedMultiplier = 1f;

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
