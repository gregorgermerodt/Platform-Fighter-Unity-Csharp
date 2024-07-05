using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FighterController : MonoBehaviour
{
    [SerializeField] private float FALL_SPEED = 9.81f;
    [SerializeField] private float TERMINAL_FALL_VELOCITY = 2.0f;
    [SerializeField] private float DOWN_CHECK_DISTANCE = 2.0f;
    [SerializeField] private float FIGHTER_WIDTH = 1.0f;

    private new BoxCollider collider;
    private new Rigidbody rigidbody;
    public bool isGrounded;

    [SerializeField] private LayerMask groundLayerMasks;
    [SerializeField] private LayerMask platformLayerMasks;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnValidate()
    {
        groundLayerMasks = 1 << LayerMask.NameToLayer("Ground");
        platformLayerMasks = 1 << LayerMask.NameToLayer("Platform");

        collider = GetComponent<BoxCollider>();
        collider.center = transform.position + Vector3.up * DOWN_CHECK_DISTANCE;
        collider.size = new Vector3(
            FIGHTER_WIDTH,
            DOWN_CHECK_DISTANCE,
            FIGHTER_WIDTH
        );
        collider.includeLayers = groundLayerMasks | platformLayerMasks;
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Player");
        collider.isTrigger = true;

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.automaticCenterOfMass = false;
        rigidbody.automaticInertiaTensor = false;
        rigidbody.useGravity = true;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rigidbody.freezeRotation = true;
        rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
        rigidbody.includeLayers = groundLayerMasks;
    }

    public void UpdateTick()
    {
        if (rigidbody.velocity.y < -TERMINAL_FALL_VELOCITY)
        {
            rigidbody.velocity += Vector3.up * FALL_SPEED;
            if (rigidbody.velocity.y > -TERMINAL_FALL_VELOCITY)
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, -TERMINAL_FALL_VELOCITY, 0.0f);
            }
        }

        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0.0f, 25.0f, 0.0f);
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        CheckForGround(other);
    }

    public void OnCollisionStay(Collision other)
    {
        CheckForGround(other);
    }

    private void CheckForGround(Collision other)
    {
        RaycastHit hitInfo;
        bool isHit = Physics.Raycast(
            transform.position + Vector3.up * DOWN_CHECK_DISTANCE,
            -Vector3.up,
            out hitInfo,
            DOWN_CHECK_DISTANCE * 3,
            groundLayerMasks | platformLayerMasks
        );
        if (isHit)
        {
            Physics.IgnoreCollision(collider, other.collider, false);
            transform.position = hitInfo.point;
            isGrounded = true;
        }
        else
        {
            Physics.IgnoreCollision(collider, other.collider, true);
        }
    }

    public void OnCollisionExit(Collision other)
    {
        if ((other.gameObject.layer & (groundLayerMasks | platformLayerMasks)) > 0)
        {
            Physics.IgnoreCollision(collider, other.collider, false);
            rigidbody.isKinematic = true;
            isGrounded = false;
        }
    }

    //public void FallThroughPlatform() => isFallingThroguhQueued = true;
    public void SetVelocity(Vector2 velocity) => rigidbody.velocity = new Vector3(velocity.x, velocity.y, 0.0f);
    public void AddVelocity(Vector2 velocity) => rigidbody.velocity += new Vector3(velocity.x, velocity.y, 0.0f);
    public void SetHorizontalVelocity(float xVelocity) => rigidbody.velocity = new Vector3(xVelocity, rigidbody.velocity.y, 0.0f);
    public void SetVerticalVelocity(float yVelocity) => rigidbody.velocity = new Vector3(rigidbody.velocity.x, yVelocity, 0.0f);
    public void AddHorizontalVelocity(float xVelocity) => rigidbody.velocity += new Vector3(xVelocity, rigidbody.velocity.y, 0.0f);
    public void AddVerticalVelocity(float yVelocity) => rigidbody.velocity += new Vector3(rigidbody.velocity.x, yVelocity, 0.0f);

    public float GetVelocityX() => rigidbody.velocity.x;
    public float GetVelocityY() => rigidbody.velocity.y;
}