using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayBoxCollider : MonoBehaviour
{
    [SerializeField] private Vector3 normal;
    private new BoxCollider collider;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        
    }
    
    void OnValidate()
    {
        if (normal != Vector3.zero && Mathf.Abs(normal.magnitude - 1) > 0.01) {
            normal = normal.normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(normal));
    }
}