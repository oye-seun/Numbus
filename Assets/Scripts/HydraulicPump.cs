using UnityEngine;

public class HydraulicPump : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private GameObject pusher;

    
    [Header("Target Detection")]
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private float maxPushDistance = 5f;
    [SerializeField] private LayerMask targetLayer; // Layer for pushable objects
    [SerializeField] private LayerMask triggerLayer; // Layer for objects that activate the pump
    [SerializeField] private bool useSphereCast = true; // True for SphereCast, False for Raycast
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLines = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is on the trigger layer
        if (((1 << collision.gameObject.layer) & triggerLayer) != 0)
        {
            PushTarget();
        }
    }

    private void PushTarget()
    {
        Debug.Log("Object hit");
        RaycastHit2D hit = new RaycastHit2D();
        //bool targetFound = false;

        // Calculate the ray's start position and direction
        Vector3 rayStart = pusher.transform.position;
        Vector3 rayDirection = pusher.transform.up;

        // Draw debug ray
        if (showDebugLines)
        {
            Debug.DrawRay(rayStart, rayDirection * maxPushDistance, Color.red, 1f);
        }

        if (useSphereCast)
        {
            // Use SphereCast to detect objects within a radius
            hit = Physics2D.CircleCast(
                rayStart,
                detectionRadius,
                rayDirection
            );



            //// Draw debug sphere
            //if (showDebugLines)
            //{
            //    Debug.DrawLine(rayStart, rayStart + rayDirection * maxPushDistance, Color.yellow, 1f);
            //    Debug.DrawWireSphere(rayStart + rayDirection * maxPushDistance, detectionRadius);
            //}
        }
        else
        {
            // Use Raycast for precise detection
            hit = Physics2D.Raycast(
                rayStart,
                rayDirection,
                detectionRadius
            );
        }

        if (hit.collider != null)
        {
            Rigidbody2D targetRb = hit.collider.attachedRigidbody;
            if (targetRb != null)
            {
                targetRb.AddForce(rayDirection * pushForce, ForceMode2D.Impulse);
            }
        }
    }
}