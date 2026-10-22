using UnityEngine;


public class RuntimeDataBridge : MonoBehaviour
{


    // Horizontal Plane Movement Vector
    public Vector3 HorizontalDirection;

    // Vertical Y Axis Movement Value
    public float VerticalDirection;

    // Initialize player states database instance
    public PlayerStatesDB StatesDB = new PlayerStatesDB();

    // Multi-jump combo counter
    public int JumpCounter;

}