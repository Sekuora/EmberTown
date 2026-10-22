using System;
using UnityEngine;

[Serializable]
public class WallJumpCondition : AbilityCondition
{
    public override bool CanUse(RuntimeDataBridge rData)
    {
        if (rData == null) return false;

        return rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.WallSlide;
    }
}
