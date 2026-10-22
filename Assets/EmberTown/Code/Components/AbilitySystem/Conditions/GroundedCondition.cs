using System;

[Serializable]
public class GroundedCondition : AbilityCondition
{


    public override bool CanUse(RuntimeDataBridge rData)
    {

        return rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.Ground ||
               rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.WallPush;
    }


}