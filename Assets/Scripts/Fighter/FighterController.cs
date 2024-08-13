using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using System.Data.Common;
using System;
using UnityEngine.TextCore;

[RequireComponent(typeof(BoxCollider))]
public class FighterController : MonoBehaviour
{
    [SerializeField] private float CHECK_DISTANCE = 0.2f;
    [SerializeField] private float FIGHTER_WIDTH = 1.0f;
    [SerializeField] private float MAX_SLOPE_ANGLE_DEGREE = 45;

    private new BoxCollider collider;
    //private new Rigidbody rigidbody;
    [SerializeField] private LayerMask groundLayerMasks;
    [SerializeField] private LayerMask platformLayerMasks;

    [SerializeField] public Vector3 currentVelocity;
    //[SerializeField] private Vector3 accumalatedVelocity;

    [SerializeField] private Collider floorCollider;
    [field: SerializeField] public bool isGrounded { get; private set; } = false;
    [field: SerializeField] public bool wasGrounded { get; private set; } = false;

    private Vector3 previousPosition;

    [field: SerializeField] public bool isInHitStun { get; private set; } = false;

    public FighterMoveset.FaceDirection faceDirection { get; private set; }
    public bool isStickHoldingDown = false;

    void Awake()
    {
        //rigidbody = GetComponent<Rigidbody>();
    }

    void OnValidate()
    {
        groundLayerMasks = 1 << LayerMask.NameToLayer("Ground");
        platformLayerMasks = 1 << LayerMask.NameToLayer("Platform");
        //rigidbody = GetComponent<Rigidbody>();
        //rigidbody.automaticCenterOfMass = false;
        //rigidbody.automaticInertiaTensor = false;
        //rigidbody.useGravity = true;
        //rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        //rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        //rigidbody.freezeRotation = true;
        //rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
        //rigidbody.includeLayers = groundLayerMasks;
    }

    public void UpdateTick()
    {
        wasGrounded = isGrounded;
        isGrounded = false;
        previousPosition = transform.position;

        if (currentVelocity.y <= 0.0f)
            HandleGround();
        else if (currentVelocity.y > 0.0f)
            HandleCeiling();
        //if (currentVelocity.magnitude != 0.0f)
        //HandleWalls();

        transform.Translate(currentVelocity, Space.World);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
    }

    private void HandleCeiling()
    {
        Vector3 origin = previousPosition + Vector3.right * currentVelocity.x;
        Vector3 rayDirection = Vector3.up;
        RaycastHit hitInfo;
        float rayLength = 2.0f;

        bool hit;

        hit = Physics.Raycast(
            origin,
            rayDirection,
            out hitInfo,
            rayLength,
            groundLayerMasks
        );
        if (hit)
        {
            Vector3 ceilingSlopeDirection = Vector3.Cross(hitInfo.normal, Vector3.forward);
            if (ceilingSlopeDirection.y < 0.0f)
                ceilingSlopeDirection.y *= 1.0f;
            Vector3.Normalize(ceilingSlopeDirection);
            currentVelocity = ceilingSlopeDirection * Vector3.Dot(currentVelocity, ceilingSlopeDirection);
            transform.position = hitInfo.point - new Vector3(0.0f, 2.0f, 0.0f);
            //currentVelocity.y = 0.0f;
        }
    }

    private void HandleGround()
    {
        RaycastHit hitInfo;
        Vector3 origin;
        Vector3 rayDirection;
        float rayLength;

        if (wasGrounded)
        {
            origin = previousPosition + Vector3.right * currentVelocity.x + Vector3.up * CHECK_DISTANCE;
            rayDirection = -Vector3.up;
            rayLength = CHECK_DISTANCE * 2;
            if (CheckGroundCommon(origin, rayDirection, rayLength, out hitInfo)
            && !CheckShouldPlayerFallThroughPlatform(hitInfo))
            {
                transform.position = hitInfo.point;
                currentVelocity.y = 0.0f;
                floorCollider = hitInfo.collider;
                isGrounded = true;
                return;
            }
        }

        //origin = previousPosition - (currentVelocity.normalized * CHECK_DISTANCE);
        origin = previousPosition;
        rayDirection = currentVelocity.normalized;
        rayLength = CHECK_DISTANCE + currentVelocity.magnitude;

        if (CheckGroundCommon(origin, rayDirection, rayLength, out hitInfo)
        && !CheckShouldPlayerFallThroughPlatform(hitInfo))
        {

            //if (hitInfo.distance > CHECK_DISTANCE)
            //{
            //    currentVelocity = rayDirection * hitInfo.distance * 0.75f;
            //    //transform.position = hitInfo.point - rayDirection * hitInfo.distance;
            //}
            //else
            //{
            transform.position = hitInfo.point;
            currentVelocity.y = 0.0f;
            floorCollider = hitInfo.collider;
            isGrounded = true;
            //}
        }


    }

    private bool CheckShouldPlayerFallThroughPlatform(RaycastHit hitInfo)
    {
        return 1 << hitInfo.transform.gameObject.layer == platformLayerMasks && isStickHoldingDown;
    }

    private bool CheckGroundCommon(Vector3 origin, Vector3 rayDirection, float rayLength, out RaycastHit hitInfo)
    {
        bool hit;

        hit = Physics.Raycast(
            origin,
            rayDirection,
            out hitInfo,
            rayLength,
            groundLayerMasks | platformLayerMasks
        );

        if (hit)
        {
            if (Vector3.Dot(-Vector3.up, hitInfo.normal) < 0)
            {
                return true; // Änderung hier, um den Status zurückzugeben
            }
        }
        return false; // Änderung hier, um den Status zurückzugeben
    }


    public void OnCollisionEnter(Collision other)
    {
        //CheckForGround(other);
    }

    public void OnCollisionStay(Collision other)
    {
        //CheckForGround(other);
    }

    public void OnCollisionExit(Collision other)
    {
        if ((other.gameObject.layer & (groundLayerMasks | platformLayerMasks)) > 0)
        {
            Physics.IgnoreCollision(collider, other.collider, false);
            //rigidbody.isKinematic = true;
            isGrounded = false;
        }
    }

    public void ApproachHorizontalVelocity(float acceleration, float targetVelocityX, bool capToTargetVelocity = true)
    {
        acceleration = Mathf.Abs(acceleration);

        float direction = Mathf.Sign(targetVelocityX - currentVelocity.x);
        float newVelocityX = currentVelocity.x + (direction * acceleration * Time.fixedDeltaTime);

        if (capToTargetVelocity && direction * newVelocityX >= direction * targetVelocityX)
            currentVelocity = new Vector3(targetVelocityX, currentVelocity.y, currentVelocity.z);
        else
            currentVelocity = new Vector3(newVelocityX, currentVelocity.y, currentVelocity.z);
    }

    public void ApproachVerticalVelocity(float acceleration, float targetVelocityY, bool capToTargetVelocity = true)
    {
        acceleration = Mathf.Abs(acceleration);

        float direction = Mathf.Sign(targetVelocityY - currentVelocity.y);
        float newVelocityY = currentVelocity.y + (direction * acceleration * Time.fixedDeltaTime);

        if (capToTargetVelocity && direction * newVelocityY >= direction * targetVelocityY)
            currentVelocity = new Vector3(currentVelocity.x, targetVelocityY, 0.0f);
        else
            currentVelocity = new Vector3(currentVelocity.x, newVelocityY, 0.0f);
    }

    //public void FallThroughPlatform() => isFallingThroguhQueued = true;
    public void SetVelocity(Vector2 velocity) => currentVelocity = new Vector3(velocity.x, velocity.y, 0.0f);
    public void SetHorizontalVelocity(float newVelocityX) => currentVelocity = new Vector3(newVelocityX, currentVelocity.y, 0.0f);
    public void SetVerticalVelocity(float newVelocityY) => currentVelocity = new Vector3(currentVelocity.x, newVelocityY, 0.0f);

    public void AddVelocity(Vector2 velocity) => currentVelocity += new Vector3(velocity.x, velocity.y, 0.0f);
    public void AddHorizontalVelocity(float addedVelocityX) => currentVelocity += new Vector3(addedVelocityX, 0.0f, 0.0f);
    public void AddVerticalVelocity(float addedVelocityY) => currentVelocity += new Vector3(0.0f, addedVelocityY, 0.0f);

    public Vector3 GetVelocity() => currentVelocity;

    void OnDrawGizmos()
    {
        Vector3 origin;
        Vector3 rayDirection;
        float rayLength;

        if (currentVelocity.y < 0.0f)
        {
            if (wasGrounded)
            {
                origin = previousPosition + Vector3.right * currentVelocity.x + Vector3.up * CHECK_DISTANCE;
                rayDirection = -Vector3.up;
                rayLength = CHECK_DISTANCE * 2;
            }
            else
            {
                origin = previousPosition;
                rayDirection = currentVelocity.normalized;
                rayLength = CHECK_DISTANCE + currentVelocity.magnitude;
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, rayDirection * rayLength);
        }
    }

    public void UpdateLookDirection(FighterMoveset.FaceDirection lookDirection)
    {
        faceDirection = lookDirection;
        if (lookDirection == FighterMoveset.FaceDirection.RIGHT && transform.eulerAngles.y > 0.0f)
        {
            transform.rotation = Quaternion.Euler(0f, -70.0f, 0.0f);
        }
        else if (lookDirection == FighterMoveset.FaceDirection.LEFT && transform.eulerAngles.y - 360 < 0.0f)
        {
            transform.rotation = Quaternion.Euler(0f, 70.0f, 0.0f);
        }
    }

    internal void DestroyHitbox(AttackHitbox ahb)
    {
        Destroy(ahb);
    }
}