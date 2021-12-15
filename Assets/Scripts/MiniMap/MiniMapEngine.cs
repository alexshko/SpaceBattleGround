using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Map
{
    public class MiniMapEngine : MonoBehaviour
    {
        [Tooltip("list of all the targets that are not the local player")]
        public List<MiniMapTarget> listOfTargets;
        [Tooltip("the local player Target")]
        public MiniMapTarget localTarget;

        [Tooltip("Prefab of dot on the map that represents the player")]
        public Transform TargetPinpointLocalPref;
        [Tooltip("Prefab of dot on the map that represents the enemy")]
        public Transform TargetPinpointEnemyPref;
        [Tooltip("Prefab of dot on the map that represents a neutral object")]
        public Transform TargetPinpointNeutralPref;

        [Tooltip("minimum height difference between the local player and the enemy to be shown on the map")]
        public float minForHeightDifference;


        [Tooltip("The radius of the world to take into account")]
        public float WorldMaxRadius = 1000;
        private float radarRadius;

        public static MiniMapEngine singelton;

        private void Awake()
        {
            singelton = this;
            listOfTargets = new List<MiniMapTarget>();
        }

        private void Start()
        {
            radarRadius = Mathf.Min(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height)/2.0f;
        }
        //Update is called once per frame
        void Update()
        {
            //set the loacl target to be in the center of the minimap. 
            //localtarget can be set after the map has been initialized, so it should be set in update:
            if (!localTarget) return;

            localTarget.pinpointMapRef.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            foreach (MiniMapTarget target in listOfTargets)
            {
                UpdatePinpointMap(target);
            }
        }

        private void UpdatePinpointMap(MiniMapTarget target)
        {
            //direction vector to the target:
            Vector3 targetDirWorld = calcDirectionToLocalPlayer(target);
            //projected to x-z plane (without the height):
            Vector3 targetDirWorldProjected = new Vector3(targetDirWorld.x, 0, targetDirWorld.z);
            //player's forward vector in world space, projected to x-z plane:
            Vector3 playerForwardWorldProjected = localTarget.transform.forward;
            playerForwardWorldProjected.y = 0;

            //calculate the angle between player's forward vector and the direction to target, projected to x-z plane:
            float angleLocalToTarget = -Vector3.SignedAngle(playerForwardWorldProjected, targetDirWorldProjected, Vector3.up);
            UpdateTargetOnMap(target, targetDirWorldProjected, angleLocalToTarget);
            updateUpDownArrowOnPinpoint(target, targetDirWorld);

        }

        //update the arrow on the pinpoint:
        private void updateUpDownArrowOnPinpoint(MiniMapTarget target, Vector3 targetDirWorld)
        {
            float heightDeiff = targetDirWorld.y;
            if (heightDeiff > minForHeightDifference)
            {
                setArrowOfPinpoint(target, true);
            }
            else if (heightDeiff < -minForHeightDifference)
            {
                setArrowOfPinpoint(target, false);
            }
            else
            {
                unSetPinointArrow(target);
            }
        }

        private void UpdateTargetOnMap(MiniMapTarget target, Vector3 targetDirWorldProjected, float angleLocalToTarget)
        {
            //put the target on the map in the center, rotate angleLocalToTarget degrees and go forward(up) the required distance
            //in the end clamp it to not go over the map radius:
            target.pinpointMapRef.anchoredPosition = Vector3.zero;
            target.pinpointMapRef.rotation = Quaternion.Euler(0, 0, angleLocalToTarget);
            target.pinpointMapRef.anchoredPosition = Vector2.ClampMagnitude(target.pinpointMapRef.up * (targetDirWorldProjected.magnitude / WorldMaxRadius), 1) * radarRadius;
        }

        private void unSetPinointArrow(MiniMapTarget target)
        {
            foreach (var img in target.pinpointMapRef.GetComponentsInChildren<Image>())
            {
                if (img.name != "Arrow") continue;
                img.enabled = false;
            }
        }

        private void setArrowOfPinpoint(MiniMapTarget target,  bool isUp)
        {
            foreach (var img in target.pinpointMapRef.GetComponentsInChildren<Image>())
            {
                if (img.name != "Arrow") continue;
                img.enabled = true;

                float requiredDegree = isUp ? 0 : 180;
                img.rectTransform.rotation = Quaternion.Euler(0, 0, requiredDegree);
            }   
        }

        private Vector3 calcDirectionToLocalPlayer(MiniMapTarget target)
        {
            Vector3 dirLocalToTarget = target.transform.position - localTarget.transform.position;
            return dirLocalToTarget;
        }
    }
}
