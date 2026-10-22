using UnityEngine;

/// <summary>
/// Character Data from Character Subsystem enum data.
/// Used in game entities.
/// </summary>
public class CharacterDataBridge : MonoBehaviour
{

    // Character Data from Character Subsystem
    [SerializeField]
    private CharacterSubsystem.Names characterName;

    // Public Members
    public CharacterSubsystem.Names CharacterName { get => characterName; set => characterName = value; }

}