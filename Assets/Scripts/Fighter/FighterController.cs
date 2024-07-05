using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FighterController : MonoBehaviour
{
    [SerializeField] private float DOWN_CHECK_DISTANCE = 0.2f;
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

    [field: SerializeField] public bool isInHitStun { get; private set; } = false;


    void Awake()
    {
        //rigidbody = GetComponent<Rigidbody>();
    }

    void OnValidate()
    {
        groundLayerMasks = 1 << LayerMask.NameToLayer("Ground");
        platformLayerMasks = 1 << LayerMask.NameToLayer("Platform");

        collider = GetComponent<BoxCollider>();
        collider.center = Vector3.up * DOWN_CHECK_DISTANCE;
        collider.size = new Vector3(
            FIGHTER_WIDTH,
            DOWN_CHECK_DISTANCE,
            FIGHTER_WIDTH
        );
        collider.includeLayers = groundLayerMasks | platformLayerMasks;
        collider.excludeLayers = 1 << LayerMask.NameToLayer("Player");
        collider.isTrigger = true;

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

        if (currentVelocity.y <= 0.0f)
            HandleGround();

        transform.Translate(currentVelocity, Space.World);

        if (transform.position.y < -10)
        {
            transform.position = new Vector3(0.0f, 25.0f, 0.0f);
        }
    }

    private void HandleGround()
    {
        Vector3 origin = transform.position + new Vector3(currentVelocity.x, DOWN_CHECK_DISTANCE, 0.0f);
        Vector3 dir = currentVelocity.y != 0 ? currentVelocity.normalized : new Vector3(currentVelocity.x, -DOWN_CHECK_DISTANCE, 0.0f).normalized;
        float rayLength = DOWN_CHECK_DISTANCE * 2 + currentVelocity.magnitude;
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(
            origin,
            dir,
            out hitInfo,
            rayLength,
            groundLayerMasks | platformLayerMasks
        );
        if (hit)
        {
            if (Vector3.Dot(-Vector3.up, hitInfo.normal) < 0)
            {
                if (hitInfo.distance > DOWN_CHECK_DISTANCE * 2)
                {
                    transform.Translate((hitInfo.point - transform.position) * 0.5f, Space.World);
                    currentVelocity.y = -DOWN_CHECK_DISTANCE;
                }
                else
                {
                    transform.position = hitInfo.point;
                    currentVelocity.y = 0.0f;
                }
                isGrounded = true;
            }
        }
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

    public void ApproachVertivalVelocity(float acceleration, float targetVelocityY, bool capToTargetVelocity = true)
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
        Vector3 origin = transform.position + new Vector3(currentVelocity.x, DOWN_CHECK_DISTANCE, 0.0f);
        Vector3 dir = currentVelocity.normalized;
        float rayLength = DOWN_CHECK_DISTANCE * 2 + currentVelocity.y;
        Gizmos.DrawRay(origin, dir * rayLength);
    }
}