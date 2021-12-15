using UnityEngine;

namespace SpaceBattle.Map
{
    public enum TypeOfPinpoint { LocalPlayer, Enemy,Neutral}
    public class MiniMapTarget : MonoBehaviour
    {
        
        public TypeOfPinpoint type;
        //reference to the pinpoint on the map.
        public RectTransform pinpointMapRef { get; set; }


        private void OnEnable()
        {
            if (pinpointMapRef == null)
            {
                Transform chosenPinpoint = choosePinpointPrefByType(type);
                pinpointMapRef = Instantiate(chosenPinpoint, chosenPinpoint.position, Quaternion.identity).GetComponent<RectTransform>();
                pinpointMapRef.SetParent(MiniMapEngine.singelton.transform);
            }

            //add to the correct property in MiniMapEngine, either local target (and in the middle of the map) or anything else and then
            //need to calculate the relative distance to him from the localtarget.
            if (type == TypeOfPinpoint.LocalPlayer)
            {
                MiniMapEngine.singelton.localTarget = this;
            }
            else
            {
                MiniMapEngine.singelton.listOfTargets.Add(this);
            }
        }

        private Transform choosePinpointPrefByType(TypeOfPinpoint type)
        {
            if (type == TypeOfPinpoint.Enemy) return MiniMapEngine.singelton.TargetPinpointEnemyPref;
            if (type == TypeOfPinpoint.Neutral) return MiniMapEngine.singelton.TargetPinpointNeutralPref;
            return MiniMapEngine.singelton.TargetPinpointLocalPref;
        }

        private void OnDisable()
        {
            if (type == TypeOfPinpoint.LocalPlayer)
            {
                MiniMapEngine.singelton.localTarget = null;
            }
            else
            {
                MiniMapEngine.singelton.listOfTargets.Remove(this);
            }
        }

        //if we instantiated new pinpointMapRef then need to destroy it when the object is dead:
        private void OnDestroy()
        {
            if (pinpointMapRef)
            {
                Destroy(pinpointMapRef.gameObject);
            }
        }
    }
}
