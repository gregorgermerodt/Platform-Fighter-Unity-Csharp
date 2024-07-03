using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering.Universal.Internal;

public class EnviromentalCollisionBox
{
    public enum ECBPosition : int
    {
        TOP = 0,
        LEFT = 1,
        BOTTOM = 2,
        RIGHT = 3
    }

    public PolygonCollider2D collider2d { get; private set; }

    public EnviromentalCollisionBox(PolygonCollider2D collider2d)
    {
        this.collider2d = collider2d;
    }

    public Vector3 this[ECBPosition index]
    {
        get
        {
            Vector2 point = collider2d.points[(int)index];
            return new Vector3(point.x, point.y, 0.0f);
        }
        set => collider2d.points[(int)index] = new Vector2(value.x, value.y);
    }
}

[RequireComponent(typeof(PolygonCollider2D))]
public class FighterPhysics : MonoBehaviour
{
    [SerializeField] private float REGULAR_FALL_GRAVITY = 9.81f;
    [SerializeField] private float JUMP_FORCE = 2.5f;
    [SerializeField] private float TERMINAL_FALL_VELOCITY = 2.0f;
    [SerializeField] private LayerMask GROUND_MASKS;

    [SerializeField] private EnviromentalCollisionBox ecb;

    [field: SerializeField] public Vector2 velocity { get; protected set; } = Vector2.zero;
    public Vector3 velocity3d { get => new Vector3(velocity.x, velocity.y, 0.0f); }
    [SerializeField] private bool isGrounded = false;

    void Awake()
    {
        ecb = new EnviromentalCollisionBox(GetComponent<PolygonCollider2D>());
    }

    void FixedUpdate()
    {
        UpdateVelocity();
        UpdatePosition();

        //CheckForGroundCollision();

        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 10;
        // frame rate
        //float currentFrameRate = 1.0f / Time.deltaTime;
        //Debug.Log("FPS: " + Mathf.RoundToInt(currentFrameRate).ToString());
    }

    private void UpdateVelocity()
    {
        // Gravity
        if (!isGrounded)
        {
            velocity = new Vector2(velocity.x, Mathf.Max(velocity.y - REGULAR_FALL_GRAVITY * Time.fixedDeltaTime, -TERMINAL_FALL_VELOCITY));
        }

        // Check if path is possible
        //Vector3 direction = new Vector3(velocity.x, velocity.y, 0).normalized;
        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, direction, out hit, velocity.magnitude, GROUND_MASKS))
        //{
        //    hit.
        //}
        //else
        //{
        //    Debug.Log("Pfad frei");
        //}
    }



    private void UpdatePosition()
    {
        if (velocity == Vector2.zero)
            return;
        Vector3 newPosition = transform.position + velocity3d * Time.deltaTime;
        float raycastLength = velocity3d.magnitude * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity3d.normalized, raycastLength, 8);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject.layer);

            velocity = new Vector2(velocity.x, 0);
            transform.position = hit.point;
        }
        else
        {
            transform.position = newPosition;
        }
        if (transform.position.y < -5)
        {
            transform.position += new Vector3(0, 20, 0);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + velocity3d * Time.deltaTime);
    }



    //private void CheckForGroundCollision()
    //{
    //    isGrounded = Physics.CheckSphere(position: ecb[EnviromentalCollisionBox.ECBPosition.BOTTOM], radius: 0.1f, layerMask: GROUND_MASKS);
    //}

    //private void UpdateUpVelocity()
    //{
    //    // > -10f sets maximum fall speed; !isGrounded checks if the character is on the ground
    //    //if (upVelocity > -10f && !isGrounded)
    //    //{
    //    //    upVelocity -= gravityAcceleration * gravityAccelerationFactor;
    //    //}
    //    //Debug.Log("UpdateUpVelocity: " + upVelocity);
    //    if (!isGrounded)
    //    {
    //        if (velocity.y > TERMINAL_FALL_VELOCITY)
    //        {
    //            velocity -= new Vector2(0.0f, GRAVITY_ACCELERATION * gravityAccelerationFactor);
    //        }
    //        else
    //        {
    //            velocity.y = TERMINAL_FALL_VELOCITY;
    //        }
    //    }else if(isGrounded && !isJumping)
    //    {
    //        velocity.y = 0.0f;
    //    }
    //}

    public void SetVelocity(Vector2 velocity) => this.velocity = new Vector2(velocity.x, velocity.y);
    public void AddVelocity(Vector2 velocity) => this.velocity += new Vector2(velocity.x, velocity.y);
    public void SetHorizontalVelocity(float xVelocity) => this.velocity = new Vector2(xVelocity, this.velocity.y);
    public void SetVerticalVelocity(float yVelocity) => this.velocity = new Vector2(this.velocity.x, yVelocity);
    public void AddHorizontalVelocity(float xVelocity) => this.velocity += new Vector2(xVelocity, this.velocity.y);
    public void AddVerticalVelocity(float yVelocity) => this.velocity += new Vector2(this.velocity.x, yVelocity);

    public void Jump()
    {
        if (isGrounded)
        {
            AddVelocity(new Vector2(0.0f, JUMP_FORCE));
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (((1 << collision.gameObject.layer) & groundMask) != 0)
    //    {

    //    }
    //}


    //public void Setvelocity.y(float y_velocity)
    //{
    //    velocity.y = y_velocity;
    //}
    //public void SetVelocityVector(float x, float y)
    //{
    //    velocity += new Vector2(x,y);
    //}
}
