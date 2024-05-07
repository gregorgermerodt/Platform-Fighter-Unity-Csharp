using System.Collections;
using System.Collections.Generic;
using JetBrains.Rider.Unity.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FighterMovement : MonoBehaviour
{
    public enum LookDirection {
        RIGHT,
        LEFT
    }
    public enum MovementState {
        IDLE,
        WALK,
        DASH,
        RUN,
        TURNING_AROUND_RIGHT,
        TURNING_AROUND_LEFT,
        RUN_TURNING_AROUND_RIGHT,
        RUN_TURNING_AROUND_LEFT,
        JUMP,
        AIR_IDLE,
        AIRDOGE,
        FREEFALL
    }

    [SerializeField] PlayerGlobals.Player player;
    InputAction movementAction;
    InputAction jumpAction;
    Rigidbody rb;

    MovementState movementState;
    Vector2 velocity;
    bool isOnGround;
    LookDirection lookDirection;
    int timesJumped;
    int frameCounter;
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] float jumpForce = 10f;

    void OnEnable()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        //inputManager.allowControllerAssigns = true;

        player = PlayerGlobals.Player.PLAYER_1;
        movementAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Movement");
        movementAction.performed += OnMovementPerformed;
        movementAction.Enable();

        jumpAction = inputManager.inputActions.FindActionMap("InGame").FindAction("Jump");
        jumpAction.performed += OnJumpPerformed;
        jumpAction.Enable();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        velocity = Vector2.zero;
    }

    void Update()
    {

    }

    void OnMovementPerformed(InputAction.CallbackContext context)
    {
        velocity = context.ReadValue<Vector2>();
        float moveX = velocity.x;
        rb.velocity += new Vector3(moveX * movementSpeed * Time.deltaTime, 0, 0);
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
            rb.velocity += Vector3.up * jumpForce;
    }
}
