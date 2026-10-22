/// <summary>
/// Condition in Ember Town refers to a condtion required to trigger a conversation.
/// </summary>
public class Condition
{
    /* Maybe later on instead of using initializers
    // I could make a fluent / modular builder for conditions
    // So that they can be create similar to my dialogues
    // Example:
    // New Condition() =
    // {
    //    .ID(nameof(ItemIdDatabase.ItemId) or other string value)
    //    .AmountRequirement(50)
    //    .etc
    //    }
    */

    // Condition Initializer
    // ID Name
    public Condition(Types inType, CharacterSubsystem.Names inTargetCharacterID, bool inCheckFlagState)
    {
        targetCharacterID = inTargetCharacterID;
        checkFlagState = inCheckFlagState;
        type = inType;
    }

    // No ID Name
    public Condition(string inConditionID, Types inType, CharacterSubsystem.Names inTargetCharacterID, bool inCheckFlagState)
    {
        conditionID = inConditionID;
        targetCharacterID = inTargetCharacterID;
        checkFlagState = inCheckFlagState;
        type = inType;
    }

    // Item Amount Requirement
    public Condition(ItemIDsDatabase.ItemId itemId, Types inType, int inAmount)
    {
        conditionID = nameof(itemId);
        type = inType;
        requiredAmount = inAmount;
    }


    // Condition Types Enum
    public enum Types
    {
        MeetNPC,
        RequiresItem,
        RequiresItemLevel,
        RequiresItemAmount,
        RequriesSkillLevel,

    }


    // Private
    private Types type;

    private CharacterSubsystem.Names targetCharacterID;

    private Item requiredItem;

    private bool checkFlagState;

    private string conditionID;

    private int requiredAmount;



    // Public
    public Types Type { get => type; set => type = value; }
    public Item RequiredItem { get => requiredItem; set => requiredItem = value; }
    public CharacterSubsystem.Names TargetCharacterID { get => targetCharacterID; set => targetCharacterID = value; }
    public bool CheckFlagState { get => checkFlagState; set => checkFlagState = value; }
    public string ConditionID { get => conditionID; set => conditionID = value; }
    public int RequiredAmount { get => requiredAmount; set => requiredAmount = value; }
}
