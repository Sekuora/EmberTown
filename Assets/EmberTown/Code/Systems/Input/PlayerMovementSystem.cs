using UnityEngine;

public class PlayerMovementSystem : MonoBehaviour
{

    // Component References
    private InputDataBridge input;

    private EntityDataBridge data;

    private RuntimeDataBridge rData;

    private AbilityController abilityController;

    private CharacterController controller;

    private ETCamera camera;

    private WallCheck wallCheck;

    private FreelookCamera flCamera;

    private Animator animator;

    private void Awake()
    {
        input = GetComponent<InputDataBridge>();
        data = GetComponent<EntityDataBridge>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        rData = GetComponent<RuntimeDataBridge>();
        wallCheck = GetComponent<WallCheck>();
    }

    private void Start()
    {
        camera = CinematicSubsystem.Call().FindCamera("PlayerFLCamera");

        if (camera)
        {
            flCamera = camera.GetComponent<FreelookCamera>();
        }
        else
        {
            Debug.Log("Missing Camera Component!");
        }

        abilityController = GetComponent<AbilityController>();

        UpdateMovementState();

    }

    // Update is called once per frame
    private void Update()
    {
        bool blockMovement = abilityController && abilityController.GetEffectValue<bool>(EffectsDB.BlockMovement);

        bool isWallSlideActive = abilityController && abilityController.ActiveAbility is WallSlide;

        if (!blockMovement && !isWallSlideActive)
        {
            // Add Movement from Input and Camera Direction
            AddMovement();

            Rotate(rData.HorizontalDirection);
            Move(rData.HorizontalDirection);


        }

        // Animation
        BlendAnimations();

        if (DialogueSubsystem.Call().DSPlayer.State == DialoguePlayer.States.Active) { return; }

        // Add movement to final trajectory
        Vector3 moveTrajectory = new Vector3(rData.HorizontalDirection.x, rData.VerticalDirection, rData.HorizontalDirection.z);

        // Move controller
        controller.Move(moveTrajectory * Time.deltaTime);

        // Update Movement State
        UpdateMovementState();
    }

    private void UpdateMovementState()
    {
        bool isWallPushActive = abilityController && abilityController.ActiveAbility is WallPush;
        bool isWallSlideActive = abilityController && abilityController.ActiveAbility is WallSlide;
        bool isWallJumpActive = abilityController && abilityController.ActiveAbility is WallJump;

        if (controller.isGrounded)
        {
            if (!isWallPushActive)
            {
                rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Ground;
            }
        }
        else
        {
            if (!isWallSlideActive && !isWallJumpActive)
            {
                rData.StatesDB.MovementState = PlayerStatesDB.MovementStates.Air;
            }
        }
    }

    private void BlendAnimations()
    {

        animator.SetFloat("HorizontalVelocity", rData.HorizontalDirection.x);
        animator.SetFloat("VerticalVelocity", rData.HorizontalDirection.z);
    }

    private void AddMovement()
    {
        // Camera Movement
        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Movement 
        rData.HorizontalDirection = (cameraRight * input.MoveValue.x) + (cameraForward * input.MoveValue.y);

    }

    private void Rotate(Vector3 inHMoveDirection)
    {
        // Rotation
        if (inHMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inHMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, data.RotationSpeed * Time.deltaTime);

        }
    }

    private void Move(Vector3 inHMoveDirection)
    {
        float speedMultiplier = 1f;

        if (abilityController && abilityController.HasEffect(EffectsDB.IncreaseSpeed))
        {
            speedMultiplier = abilityController.GetEffectValue<float>(EffectsDB.IncreaseSpeed);
        }

        if (abilityController && abilityController.ActiveAbility is WallPush wallPush)
        {
            speedMultiplier *= wallPush.SpeedMultiplier;
        }

        // Movement Value
        rData.HorizontalDirection = inHMoveDirection * data.WalkAcceleration * speedMultiplier;
    }



    private void LateUpdate()
    {
        flCamera.CameraUpdate(input.LookValue);
    }
}

