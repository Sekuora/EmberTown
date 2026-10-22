using System;

[Serializable]
public class PlayerStatesDB
{
    // States Data
    private MovementStates movementState;

    // Public Members
    public MovementStates MovementState { get => movementState; set => movementState = value; }




    // State Enums
    public enum MovementStates
    {
        Ground,
        Air,
        Water,
        WallSlide,
        WallPush,
        WallJump

    }

}
