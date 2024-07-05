using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayBoxCollider : MonoBehaviour
{
    private new BoxCollider collider;
    private BoxCollider collisionCheckTrigger = null;

    [field: SerializeField] public Vector3 normal { get; private set; }
    [field: SerializeField] private bool isLocalDirection;
    [field: SerializeField] private float penetrationDepthThreshold = 0.2f;

    public Vector3 passthroughDirection => isLocalDirection ? transform.TransformDirection(normal.normalized) : normal.normalized;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        collisionCheckTrigger.size = new Vector3(
            collider.size.x + normal.x,
            collider.size.y + normal.y,
            collider.size.z + normal.z
        );
        collisionCheckTrigger.center = collider.center;
        collisionCheckTrigger.isTrigger = true;
    }

    void OnValidate()
    {
        if (normal != Vector3.zero && Mathf.Abs(normal.magnitude - 1) > 0.01)
        {
            normal = normal.normalized;
        }
        collider = GetComponent<BoxCollider>();
        collider.isTrigger = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Physics.ComputePenetration(
        collisionCheckTrigger, collisionCheckTrigger.bounds.center, transform.rotation,
        other, other.bounds.center, other.transform.rotation,
        out Vector3 collisionDirection, out float penetrationDepth))
        {
            float dot = Vector3.Dot(passthroughDirection, collisionDirection);

            if (dot < 0)
            {
                if (penetrationDepth < penetrationDepthThreshold)
                {
                    Physics.IgnoreCollision(collider, other, false);
                }
            }
            else
            {
                Physics.IgnoreCollision(collider, other, true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(collider.center), passthroughDirection * 2);
    }
}