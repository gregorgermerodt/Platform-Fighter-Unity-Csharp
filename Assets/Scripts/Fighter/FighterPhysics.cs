using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering.Universal.Internal;
using System.Data.Common;

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
    [SerializeField] private float GROUND_PROXIMITY_TOLERANCE = 0.05f;
    [SerializeField] private float MAX_STEP_HEIGHT = 0.25f;

    [SerializeField] private EnviromentalCollisionBox ecb;
    [SerializeField] private LayerMask groundLayerMasks;

    [field: SerializeField] public Vector2 velocity { get; protected set; } = Vector2.zero;
    [field: SerializeField] public bool isGrounded { get; private set; } = false;
    [field: SerializeField] public Collider floorCollider { get; private set; }

    void Awake()
    {
        ecb = new EnviromentalCollisionBox(GetComponent<PolygonCollider2D>());
    }

    void Update()
    {
        UpdateVelocity();
        CheckGrounding();
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0.0f, 25.0f, 0.0f);
        }
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
            velocity = new Vector2(velocity.x, velocity.y + Mathf.Max(-REGULAR_FALL_GRAVITY * Time.deltaTime, -TERMINAL_FALL_VELOCITY));
        }
        else
        {
            velocity = new Vector2(velocity.x, Mathf.Max(velocity.y, 0.0f));
        }
    }

    private void CheckGrounding()
    {
        Vector3 velocity3d = new Vector3(velocity.x, velocity.y, 0.0f);
        Vector3 possibleNewPosition = transform.position + velocity3d * Time.deltaTime;
        Vector3 tolleranceAboveGround = new Vector3(0.0f, GROUND_PROXIMITY_TOLERANCE, 0.0f);

        if (velocity.y > 0.0f)
        {
            floorCollider = null;
            isGrounded = false;
        }
        //else if (velocity.x == 0.0f)
        //{
        //    RaycastHit floorHit;
        //    bool isFloorHit = Physics.Raycast(
        //        transform.position + Vector3.up * MAX_STEP_HEIGHT,
        //        -Vector3.up,
        //        out floorHit,
        //        MAX_STEP_HEIGHT,
        //        groundLayerMasks
        //    );
        //    if (isFloorHit)
        //    {
        //        velocity = new Vector2(velocity.x, Mathf.Max(0.0f, velocity.y));
        //        possibleNewPosition = floorHit.point + tolleranceAboveGround;
        //        floorCollider = floorHit.collider;
        //        isGrounded = true;
        //    }
        //}
        else
        {
            float downCheckDistance = isGrounded ? MAX_STEP_HEIGHT : GROUND_PROXIMITY_TOLERANCE;
            Vector3 horizontalPossiblePosition = new Vector3(possibleNewPosition.x, transform.position.y, 0.0f);
            RaycastHit upSlopeHit;
            bool isUpSlopeHit = Physics.Raycast(
                horizontalPossiblePosition + Vector3.up * downCheckDistance,
                -Vector3.up,
                out upSlopeHit,
                downCheckDistance,
                groundLayerMasks
            );
            RaycastHit downSlopeHit;
            bool isDownSlopeHit = Physics.Raycast(
                horizontalPossiblePosition,
                -Vector3.up,
                out downSlopeHit,
                downCheckDistance,
                groundLayerMasks
            );
            if (!isUpSlopeHit && !isDownSlopeHit)
            {
                floorCollider = null;
                isGrounded = false;
            }
            else
            {
                if (floorCollider == null)
                {
                    if (isUpSlopeHit)
                    {
                        possibleNewPosition = upSlopeHit.point + tolleranceAboveGround;
                        floorCollider = upSlopeHit.collider;
                    }
                    else // if (isDownSlopeHit) 
                    {
                        possibleNewPosition = downSlopeHit.point + tolleranceAboveGround;
                        floorCollider = downSlopeHit.collider;
                    }
                }
                else
                {
                    if (upSlopeHit.collider == floorCollider)
                    {
                        possibleNewPosition = upSlopeHit.point + tolleranceAboveGround;
                    }
                    else
                    {
                        possibleNewPosition = downSlopeHit.point + tolleranceAboveGround;
                        floorCollider = downSlopeHit.collider;
                    }
                }
                isGrounded = true;
            }
        }
        transform.position = possibleNewPosition;
    }

    void OnDrawGizmos()
    {
        Vector3 velocity3d = new Vector3(velocity.x, velocity.y, 0.0f);
        Vector3 possibleNewPosition = transform.position + velocity3d * Time.deltaTime;
        Vector3 horizontalPossiblePosition = new Vector3(possibleNewPosition.x, transform.position.y, 0.0f);

        Vector3 position = horizontalPossiblePosition;
        Vector3 direction = Vector3.up * MAX_STEP_HEIGHT;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position, position + direction);
        Gizmos.DrawLine(position, position - direction);
    }

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