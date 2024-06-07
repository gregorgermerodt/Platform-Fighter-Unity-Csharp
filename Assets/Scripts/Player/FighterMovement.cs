using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Rider.Unity.Editor;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class FighterMovement : MonoBehaviour
{
    public enum LookDirection
    {
        RIGHT,
        LEFT
    }

    public enum MovementState
    {
        IDILING,
        WALKING,
        TURNAROUNDING,
        SHIELDING,
        SHIELD_DROPING,
        DASHING,
        DASH_TURNAROUNDING,
        RUNNING,
        RUN_TURNAROUNDING,
        BREAK_RUNNING,
        JUMPSQUATING,
        JUMPING,
        DOUBLE_JUMPING,
        AIR_IDILING,
        AIRDODGING,
        TUMBLING,
        BAD_LANDING,
        LANDING,
        FREEFALLING
    }

    public enum ActionState
    {
        STARTED,
        ACTIVE,
        CANCELED
    }

    [SerializeField] PlayerGlobals.Player playerNumber;

    InputAction movementAction;
    bool isMovementActionStarted;
    bool isMovementActionPerformed;
    bool isMovementActionCanceled;

    InputAction jumpAction;
    bool isJumpActionStarted;
    bool isJumpActionPerformed;
    bool isJumpActionCanceled;

    [SerializeField] float stickFlickVelocityThreshold = 0.5f;
    Vector2 lastLeftStickPosition;
    Vector2 leftStickMovementVelocity;
    bool isStickFlicked;
    bool isLeftStickResting { get { return !isMovementActionPerformed; } }

    MovementState movementState;
    LookDirection lookDirection = LookDirection.RIGHT;
    Vector2 velocity;
    int frameCounter;
    int midAirJumpCount;

    bool isOnGround { get { return !airBourneStates.Contains(movementState); } }
    bool isJumpingLocked { get { return noJumpStates.Contains(movementState); } }
    bool isShieldLocked { get { return noShieldStates.Contains(movementState); } }
    bool isFastfalling;
    bool isMovementLocked;

    [SerializeField] bool isInvincible;

    // Debug
    [SerializeField] float runSpeed = 20f;
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float deceleration_unused = 2f;
    [SerializeField] float dashSpeed = 30f;

    static readonly MovementState[] airBourneStates = new MovementState[]
    {
        MovementState.JUMPING,
        MovementState.AIR_IDILING,
        MovementState.AIRDODGING,
        MovementState.TUMBLING,
        MovementState.FREEFALLING
    };

    static readonly MovementState[] noJumpStates = new MovementState[] {
        MovementState.LANDING,
        MovementState.BAD_LANDING,
        MovementState.FREEFALLING,
        MovementState.AIRDODGING
    };

    static readonly MovementState[] noShieldStates = new MovementState[] {
        MovementState.SHIELDING,
        MovementState.SHIELD_DROPING,
        MovementState.DASHING,
        MovementState.DASH_TURNAROUNDING,
        MovementState.RUN_TURNAROUNDING,
        MovementState.BREAK_RUNNING,
        MovementState.JUMPSQUATING,
        MovementState.AIRDODGING,
        MovementState.BAD_LANDING,
        MovementState.LANDING,
        MovementState.FREEFALLING,
        // Shield state in the air is an airdodge
    };

    static readonly MovementState[] turnaroundStates = new MovementState[] {
        MovementState.TURNAROUNDING,
        MovementState.DASH_TURNAROUNDING,
        MovementState.RUN_TURNAROUNDING
    };
    void OnEnable()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        //inputManager.allowControllerAssigns = true;

        playerNumber = PlayerGlobals.Player.PLAYER_1;
        movementAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Movement");
        movementAction.started += context => isMovementActionStarted = true;
        movementAction.performed += context => isMovementActionPerformed = true;
        movementAction.canceled += context =>
        {
            isMovementActionPerformed = false;
            isMovementActionCanceled = true;
        };

        jumpAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Jump");
        jumpAction.started += context => isJumpActionStarted = true;
        jumpAction.performed += context => isJumpActionPerformed = true;
        jumpAction.canceled += context =>
        {
            isJumpActionPerformed = false;
            isJumpActionCanceled = true;
        };
    }

    void Start()
    {
        velocity = Vector2.zero;
        leftStickMovementVelocity = Vector2.zero;
        lastLeftStickPosition = Vector2.zero;
        UpdateDeviceId(FindAnyObjectByType<InputManager>());
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        Vector2 leftStickPosition = movementAction.ReadValue<Vector2>();
        MovementState lastMovementState = movementState;
        switch (lastMovementState)
        {
            case MovementState.IDILING:
                OnIdle(leftStickPosition);
                break;
            case MovementState.WALKING:
                OnWalk(leftStickPosition);
                break;
            case MovementState.TURNAROUNDING:
                OnTurnaround(leftStickPosition);
                break;
            case MovementState.SHIELDING:
                OnShield(leftStickPosition);
                break;
            case MovementState.SHIELD_DROPING:
                OnShieldDrop();
                break;
            case MovementState.DASHING:
                OnDash(leftStickPosition);
                break;
            case MovementState.DASH_TURNAROUNDING:
                OnDashTurnaround(leftStickPosition);
                break;
            case MovementState.RUNNING:
                OnRun(leftStickPosition);
                break;
            case MovementState.BREAK_RUNNING:
                OnBreakRun(leftStickPosition);
                break;
        }
        // Shield from spesific states
        // TODO: If shield started, ChangeIntoMovementState(MovementState.Shield)

        // TODO: Check if touching ground and reset jumps
        // Jump from almost every state
        if (isJumpActionStarted &&
            midAirJumpCount < 2 &&
            !isJumpingLocked)
        {
            ChangeMovementState(MovementState.JUMPSQUATING);
        }

        ResetInputStates();
        frameCounter++;
    }

    private void OnTurnaround(Vector2 leftStickPosition)
    {
        if (frameCounter > 10)
            ChangeMovementState(MovementState.IDILING);

        OnIdle(leftStickPosition);
    }

    private void OnBreakRun(Vector2 leftStickPosition)
    {
        if (frameCounter > 10)
            ChangeMovementState(MovementState.IDILING);
    }

    private void OnRun(Vector2 leftStickPosition)
    {

        if (!IsLookDirectionEqualStick(leftStickPosition, 0.9f)) // Stop the run
            ChangeMovementState(MovementState.BREAK_RUNNING);

        else if (IsLookDirectionOppositeStick(leftStickPosition)) // Turn around
            ChangeMovementState(MovementState.RUN_TURNAROUNDING);
        transform.position += new Vector3(leftStickPosition.x, 0, 0) * runSpeed * Time.deltaTime;
    }

    void OnDash(Vector2 leftStickPosition)
    {
        if (frameCounter < 5)
        {
            // TODO: Forgive Player: Check if it should have been a smash attack

            if (IsLookDirectionOppositeStick(leftStickPosition, 0.9f)) // Dash Dance
            {
                ReverseLookDirection();
                ChangeMovementState(MovementState.DASH_TURNAROUNDING);
            }
        }
        else if (frameCounter < 10)
        {
            // TODO: Dash attack
        }
        else if (frameCounter >= 10) // Switch to run after 10 frames
        {
            ChangeMovementState(MovementState.RUNNING);
            return;
        }
        transform.position += new Vector3(leftStickPosition.x, 0, 0) * dashSpeed * Time.deltaTime;
    }

    void OnDashTurnaround(Vector2 leftStickPosition)
    {
        OnDash(leftStickPosition);
    }

    private void OnShieldDrop()
    {
        if (frameCounter > 10)
            ChangeMovementState(MovementState.IDILING);
    }

    void OnShield(Vector2 leftStickPosition)
    {
        if (frameCounter > 2)
        {
            // TODO: Shield active and rolls
            // if not holding shield button
            ChangeMovementState(MovementState.SHIELD_DROPING);
        }
        return;
    }

    private void OnWalk(Vector2 leftStickPosition)
    {
        // Stop walking
        if (isLeftStickResting)
        {
            ChangeMovementState(MovementState.IDILING);
        }
        // Forgive Player: Check if it should have been a dash instead of walk
        if (frameCounter < 2)
        {
            if (IsLookDirectionEqualStick(leftStickPosition, 0.9f))
            {
                ChangeMovementState(MovementState.DASHING);
                return;
            }
            else if (IsLookDirectionOppositeStick(leftStickPosition, 0.9f))
            {
                ChangeMovementState(MovementState.DASH_TURNAROUNDING);
                return;
            }
        }
        transform.position += new Vector3(leftStickPosition.x, 0, 0) * walkSpeed * Time.deltaTime;
    }

    void OnIdle(Vector2 leftStickPosition)
    {
        // Movement (left stick)
        if (isMovementActionPerformed)
        {
            if (IsLookDirectionEqualStick(leftStickPosition))
            {
                // If stick is flicked within 1 frame, it's a dash, otherwise a walk
                if (leftStickPosition.x > 0.9f || leftStickPosition.x < -0.9f)
                    ChangeMovementState(MovementState.DASHING);
                else
                    ChangeMovementState(MovementState.WALKING);
            }
            else // if (IsLookDirectionOppositeStick(leftStickPosition))
            {
                ReverseLookDirection();
                // If stick is flicked within 1 frame, it's a dash, otherwise a walk
                if (leftStickPosition.x > 0.9f || leftStickPosition.x < -0.9f)
                    ChangeMovementState(MovementState.DASH_TURNAROUNDING);
                else
                    ChangeMovementState(MovementState.TURNAROUNDING);
            }
        }
    }

    void ChangeMovementState(MovementState movementState, bool resetFrameCounter = true)
    {
        this.movementState = movementState;
        if (resetFrameCounter)
            ResetFrameCounter();
    }

    void ResetFrameCounter()
    {
        // It's -1 and not 0, because it gets updated after each FixedUpdate iteration and thus -1 + 1 = 0
        frameCounter = -1;
    }

    void ReverseLookDirection()
    {
        lookDirection = lookDirection == LookDirection.RIGHT ? LookDirection.LEFT : LookDirection.RIGHT;
        if (lookDirection == LookDirection.RIGHT)
        {
            transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -45f, 0f);
        }
    }

    bool IsLookDirectionOppositeStick(Vector2 stickPosition, float xMagnitude = 0.0f) =>
        (lookDirection == LookDirection.LEFT && stickPosition.x > xMagnitude) || (lookDirection == LookDirection.RIGHT && stickPosition.x < -xMagnitude);

    bool IsLookDirectionEqualStick(Vector2 stickPosition, float xMagnitude = 0.0f) =>
        (lookDirection == LookDirection.RIGHT && stickPosition.x > xMagnitude) || (lookDirection == LookDirection.LEFT && stickPosition.x < -xMagnitude);

    void ResetInputStates()
    {
        isMovementActionStarted = false;
        isMovementActionCanceled = false;

        isJumpActionStarted = false;
        isJumpActionCanceled = false;
    }

    public void UpdateDeviceId(InputManager inputManager)
    {
        int playerControllerId = inputManager.playerControllerDeviceIds[(int)playerNumber - 1];
        if (playerControllerId == -1)
        {
            movementAction.Disable();
            jumpAction.Disable();
            return;
        }
        movementAction.Enable();
        jumpAction.Enable();
        movementAction.actionMap.devices = new InputDevice[] { InputSystem.GetDeviceById(playerControllerId) };
        jumpAction.actionMap.devices = new InputDevice[] { InputSystem.GetDeviceById(playerControllerId) };
    }
}
