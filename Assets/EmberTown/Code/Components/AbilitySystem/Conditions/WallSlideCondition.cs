using System;
using UnityEngine;

[Serializable]
public class WallSlideCondition : AbilityCondition
{
    public override bool CanUse(RuntimeDataBridge rData)
    {
        if (rData == null) return false;

        // Player must be airborne, in wall jump, or already in wall slide
        if (rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.Air &&
            rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.WallSlide &&
            rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.WallJump)
        {
            return false;
        }

        // Must be in the fall / descending phase (not jumping upward)
        if (rData.VerticalDirection > 0f) return false;

        WallCheck wallCheck = rData.GetComponent<WallCheck>();
        if (!wallCheck) return false;

        WallSlide wallSlide = rData.GetComponent<WallSlide>();
        Vector3 moveDir = wallSlide ? wallSlide.GetInputMoveDirection() : rData.HorizontalDirection;
        if (moveDir.sqrMagnitude <= 0.001f)
        {
            moveDir = rData.HorizontalDirection.sqrMagnitude > 0.001f ? rData.HorizontalDirection : rData.transform.forward;
        }

        float detectionDist = wallSlide != null ? wallSlide.DetectionDistance : (wallCheck.SlideWallDistance > 0f ? wallCheck.SlideWallDistance : 0.35f);
        return wallCheck.CheckSlideWall(moveDir, out _, detectionDist);
    }
}
