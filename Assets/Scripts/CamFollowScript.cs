using UnityEngine;

namespace SpaceBattle.core { 
    [ExecuteAlways]
    public class CamFollowScript : MonoBehaviour
    {
        [Tooltip("The GameObject To follow")]
        public Transform objToFollow;
        [Tooltip("The offset the camera should keep from the object")]
        public Vector3 initOffset;

        // Update is called once per frame
        void Update()
        {
            if (objToFollow && objToFollow.gameObject.activeInHierarchy)
            {
                transform.position = objToFollow.position - objToFollow.forward;
                transform.LookAt(objToFollow);
                transform.position = objToFollow.position + transform.forward * initOffset.z + transform.up * initOffset.y + transform.right * initOffset.x;
            }
        }
    }
}
