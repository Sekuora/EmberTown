using System;

[Serializable]
public class AirborneCondition : AbilityCondition
{

    public override bool CanUse(RuntimeDataBridge rData)
    {

        return rData.StatesDB.MovementState == PlayerStatesDB.MovementStates.Air;
    }


}