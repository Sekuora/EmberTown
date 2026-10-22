using System;
using UnityEngine;

[Serializable]
public class WallPushCondition : AbilityCondition
{
    public override bool CanUse(RuntimeDataBridge rData)
    {
        if (rData == null) return false;

        // Player must be grounded
        if (rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.Ground &&
            rData.StatesDB.MovementState != PlayerStatesDB.MovementStates.WallPush)
        {
            return false;
        }

        InputDataBridge input = rData.GetComponent<InputDataBridge>();
        if (!input || input.MoveValue.sqrMagnitude <= 0.01f) return false;

        WallPush wallPush = rData.GetComponent<WallPush>();
        Vector3 inputDir = wallPush ? wallPush.GetInputMoveDirection() : rData.HorizontalDirection;
        if (inputDir.sqrMagnitude <= 0.001f) return false;

        WallCheck wallCheck = rData.GetComponent<WallCheck>();
        if (!wallCheck) return false;

        float maxDist = wallPush != null ? wallPush.DetectionDistance : (wallCheck.HeavyWallDistance > 0f ? wallCheck.HeavyWallDistance : 1.5f);

        if (wallCheck.CheckHeavyWall(inputDir, out RaycastHit hit, maxDist))
        {
            // Moving towards the wall
            return Vector3.Dot(inputDir, hit.normal) < 0f;
        }

        return false;
    }
}
