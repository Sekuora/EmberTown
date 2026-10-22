using System;
using UnityEngine;

[Serializable]
public abstract class AbilityCondition
{
    public abstract bool CanUse(RuntimeDataBridge rData);
}