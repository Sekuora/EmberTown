using UnityEngine;

public class ConditionPayload : MonoBehaviour
{


    private string conditionID;

    private bool flagToSet;

    public string ConditionID { get => conditionID; set => conditionID = value; }
    public bool FlagToSet { get => flagToSet; set => flagToSet = value; }
}