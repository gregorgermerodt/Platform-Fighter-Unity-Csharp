using System.Collections;
using System.Collections.Generic;
using JetBrains.Rider.Unity.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FighterMovement : MonoBehaviour
{
    PlayerGlobals.Player player;
    InputAction movementAction;
    InputAction jumpAction;

    Rigidbody rb;

    Vector2 velocity;
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float jumpForce = 10f;

    void OnEnable()
    {
        InputManager inputManager = FindAnyObjectByType<InputManager>();
        inputManager.allowControllerAssigns = true;

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
        rb.velocity += new Vector3(moveX * moveSpeed * Time.deltaTime, 0, 0);
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
            rb.velocity += Vector3.up * jumpForce;
    }
}
