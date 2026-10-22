using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This bridge is used to store values from the Input System to use 
/// in other player systems.
/// </summary>
public class InputDataBridge : MonoBehaviour
{

  // References
  public InputActionAsset InputActions;

  // Actions
  private InputAction moveAction;
  private InputAction lookAction;
  private InputAction jumpAction;

  private InputAction dashAction;

  private InputAction interactAction;

  // Vector Values
  public Vector2 MoveValue { get; private set; }
  public Vector2 LookValue { get; private set; }

  // Boolean Values
  public bool JumpPressed { get; private set; }

  public bool InteractPressed { get; private set; }

  public bool DashPressed { get; private set; }


  private void OnEnable()
  {
    InputActions.FindActionMap("Player").Enable();
  }

  private void OnDisable()
  {
    InputActions.FindActionMap("Player").Disable();
  }

  private void Awake()
  {
    moveAction = InputSystem.actions.FindAction("Move");
    lookAction = InputSystem.actions.FindAction("Look");
    jumpAction = InputSystem.actions.FindAction("Jump");
    dashAction = InputSystem.actions.FindAction("Dash");
    interactAction = InputSystem.actions.FindAction("Interact");

  }



  // Update is called once per frame
  private void Update()
  {
    // Store values
    MoveValue = moveAction.ReadValue<Vector2>();
    LookValue = lookAction.ReadValue<Vector2>();

    JumpPressed = jumpAction.WasPressedThisFrame();

    InteractPressed = interactAction.WasPressedThisFrame();

    DashPressed = dashAction.WasPressedThisFrame();

  }
}
