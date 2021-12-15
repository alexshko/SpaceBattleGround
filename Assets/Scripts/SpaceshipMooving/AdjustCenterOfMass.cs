using UnityEngine;

namespace SpaceBattle.Spaceship
{
    [ExecuteAlways]
    public class AdjustCenterOfMass : MonoBehaviour
    {
        private Rigidbody rb;

        private void Update()
        {
            rb = GetComponent<Rigidbody>();
            if (!rb)
            {
                Debug.LogError("Missing RigidBody to adjust center of mass");
            }
            rb.centerOfMass = Vector3.zero;
        }
    }
}
