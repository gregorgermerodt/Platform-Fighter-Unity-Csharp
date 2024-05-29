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

[RequireComponent(typeof(Rigidbody))]
public class FighterMovement : MonoBehaviour
{
    public enum LookDirection
    {
        RIGHT,
        LEFT
    }

    public enum MovementState
    {
        IDLE,
        WALK,
        TURNAROUND,
        SHIELD,
        SHIELD_DROP,
        DASH,
        DASH_TURNAROUND,
        RUN,
        RUN_TURNAROUND,
        BREAK_RUN,
        JUMPSQUAT,
        JUMP,
        DOUBLE_JUMP,
        AIR_IDLE,
        AIRDODGE,
        TUMBLE,
        BAD_LANDING,
        LANDING,
        FREEFALL,
        ATTACK,
        SPECIAL_ATTACK
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

    Rigidbody rb;

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
    [SerializeField] float movementSpeed = 100f;
    [SerializeField] float jumpForce = 7f;

    static readonly MovementState[] airBourneStates = new MovementState[]
    {
        MovementState.JUMP,
        MovementState.AIR_IDLE,
        MovementState.AIRDODGE,
        MovementState.TUMBLE,
        MovementState.FREEFALL
    };

    static readonly MovementState[] noJumpStates = new MovementState[] {
        MovementState.LANDING,
        MovementState.BAD_LANDING,
        MovementState.FREEFALL,
        MovementState.AIRDODGE
    };

    static readonly MovementState[] noShieldStates = new MovementState[] {
        MovementState.SHIELD,
        MovementState.SHIELD_DROP,
        MovementState.DASH,
        MovementState.DASH_TURNAROUND,
        MovementState.RUN_TURNAROUND,
        MovementState.BREAK_RUN,
        MovementState.JUMPSQUAT,
        MovementState.AIRDODGE,
        MovementState.BAD_LANDING,
        MovementState.LANDING,
        MovementState.FREEFALL,
        MovementState.ATTACK,
        MovementState.SPECIAL_ATTACK
        // Shield state in the air is an airdodge
    };

    static readonly MovementState[] turnaroundStates = new MovementState[] {
        MovementState.TURNAROUND,
        MovementState.DASH_TURNAROUND,
        MovementState.RUN_TURNAROUND
    };
    void OnEnable()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        //inputManager.allowControllerAssigns = true;

        playerNumber = PlayerGlobals.Player.PLAYER_1;
        movementAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Movement");
        movementAction.started += context => isMovementActionStarted = true;
        movementAction.performed += context => isMovementActionPerformed = true;
        movementAction.canceled += context => isMovementActionCanceled = true;

        jumpAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Jump");
        jumpAction.started += context => isJumpActionStarted = true;
        jumpAction.performed += context => isJumpActionPerformed = true;
        jumpAction.canceled += context => isJumpActionCanceled = true;

        rb = GetComponent<Rigidbody>();
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
            case MovementState.IDLE:
                OnIdle(leftStickPosition);
                break;
            case MovementState.WALK:
                OnWalk(leftStickPosition);
                break;
            case MovementState.TURNAROUND:
                OnTurnaround(leftStickPosition);
                break;
            case MovementState.SHIELD:
                OnShield(leftStickPosition);
                break;
            case MovementState.SHIELD_DROP:
                OnShieldDrop();
                break;
            case MovementState.DASH:
                OnDash(leftStickPosition);
                break;
            case MovementState.DASH_TURNAROUND:
                OnDashTurnaround(leftStickPosition);
                break;
            case MovementState.RUN:
                OnRun(leftStickPosition);
                break;
            case MovementState.BREAK_RUN:
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
            ChangeIntoMovementState(MovementState.JUMPSQUAT);
        }

        ResetInputStates();
        frameCounter++;
    }

    private void OnTurnaround(Vector2 leftStickPosition)
    {
        if (frameCounter > 10)
            ChangeIntoMovementState(MovementState.IDLE);

        OnIdle(leftStickPosition);
    }

    private void OnBreakRun(Vector2 leftStickPosition)
    {
        if (frameCounter == 10)
            ChangeIntoMovementState(MovementState.IDLE);
    }

    private void OnRun(Vector2 leftStickPosition)
    {

        if (!IsLookDirectionEqualStick(leftStickPosition, 0.9f)) // Stop the run
            ChangeIntoMovementState(MovementState.BREAK_RUN);

        else if (IsLookDirectionOppositeStick(leftStickPosition)) // Turn around
            ChangeIntoMovementState(MovementState.RUN_TURNAROUND);

    }

    void OnDash(Vector2 leftStickPosition)
    {
        if (frameCounter < 5)
        {
            // TODO: Forgive Player: Check if it should have been a smash attack

            if (IsLookDirectionOppositeStick(leftStickPosition, 0.9f)) // Dash Dance
            {
                ReverseLookDirection();
                ChangeIntoMovementState(MovementState.DASH_TURNAROUND);
            }
        }
        else if (frameCounter < 10)
        {
            // TODO: Dash attack
        }
        else if (frameCounter >= 10) // Switch to run after 10 frames            
            ChangeIntoMovementState(MovementState.RUN);

    }

    void OnDashTurnaround(Vector2 leftStickPosition)
    {
        OnDash(leftStickPosition);
    }

    private void OnShieldDrop()
    {
        if (frameCounter > 10)
            ChangeIntoMovementState(MovementState.IDLE);
    }

    void OnShield(Vector2 leftStickPosition)
    {
        if (frameCounter > 2)
        {
            // TODO: Shield active and rolls
            // if not holding shield button
            ChangeIntoMovementState(MovementState.SHIELD_DROP);
        }
        return;
    }

    private void OnWalk(Vector2 leftStickPosition)
    {
        // Stop walking
        if (isLeftStickResting)
        {
            ChangeIntoMovementState(MovementState.IDLE);
        }
        // Forgive Player: Check if it should have been a dash instead of walk
        else if (frameCounter < 2)
        {
            if (IsLookDirectionEqualStick(leftStickPosition, 0.9f))
            {
                ChangeIntoMovementState(MovementState.DASH);
            }
            else if (IsLookDirectionOppositeStick(leftStickPosition, 0.9f))
            {
                ChangeIntoMovementState(MovementState.DASH_TURNAROUND);
            }
        }
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
                    ChangeIntoMovementState(MovementState.DASH);
                else
                    ChangeIntoMovementState(MovementState.WALK);
            }
            else // if (IsLookDirectionOppositeStick(leftStickPosition))
            {
                ReverseLookDirection();
                // If stick is flicked within 1 frame, it's a dash, otherwise a walk
                if (leftStickPosition.x > 0.9f || leftStickPosition.x < -0.9f)
                    ChangeIntoMovementState(MovementState.DASH_TURNAROUND);
                else
                    ChangeIntoMovementState(MovementState.TURNAROUND);
            }
        }
    }

    void OnMovementPerformed(Vector2 movementInput)
    {
        float moveX = movementInput.x;
        rb.velocity += new Vector3(moveX * movementSpeed * Time.deltaTime, 0, 0);
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
            rb.velocity += Vector3.up * jumpForce;
    }

    void SwitchMovementState(MovementState movementState) => this.movementState = movementState;

    void ChangeIntoMovementState(MovementState movementState, bool resetFrameCounter = true)
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

    void ReverseLookDirection() => lookDirection = lookDirection == LookDirection.RIGHT ? LookDirection.LEFT : LookDirection.RIGHT;

    bool IsLookDirectionOppositeStick(Vector2 stickPosition, float xMagnitude = 0.0f) =>
        (lookDirection == LookDirection.LEFT && stickPosition.x > xMagnitude) || (lookDirection == LookDirection.RIGHT && stickPosition.x < -xMagnitude);

    bool IsLookDirectionEqualStick(Vector2 stickPosition, float xMagnitude = 0.0f) =>
        (lookDirection == LookDirection.RIGHT && stickPosition.x > xMagnitude) || (lookDirection == LookDirection.LEFT && stickPosition.x < -xMagnitude);

    void ResetInputStates()
    {
        isMovementActionStarted = false;
        isMovementActionPerformed = isMovementActionCanceled ? false : isMovementActionPerformed;
        isMovementActionCanceled = false;

        isJumpActionStarted = false;
        isJumpActionPerformed = isJumpActionCanceled ? false : isJumpActionPerformed;
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
