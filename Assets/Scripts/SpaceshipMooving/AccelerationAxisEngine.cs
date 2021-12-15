using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Spaceship
{
    [RequireComponent(typeof(SpaceshipMovement))]
    public class AccelerationAxisEngine : MonoBehaviour
    {
        //reference to the acceleration throttle:
        private Slider accelerationThrottleRef;
        //the value of the accelerationAxis:
        private float accelerationAxis;


        public float AccelerationAxis { get => accelerationAxis; }

        private void Awake()
        {
            accelerationThrottleRef = FindThrottleUI();
        }


        // Update is called once per frame
        void Update()
        {
            if (accelerationThrottleRef)
            {
#if UNITY_ANDROID
                SetAccelerationMeter(accelerationThrottleRef.value, false);
#endif
#if UNITY_STANDALONE
                SetAccelerationMeter(Input.GetAxis("Vertical"), true);
#endif
            }
        }

        //set the value to the accelerationAxis and update the meter:
        public void SetAccelerationMeter(float accelMeter, bool updateUI)
        {
            accelerationAxis = accelMeter;
            //update the throttle:
            if (updateUI)
            {
                accelerationThrottleRef.value = accelerationAxis;
            }
        }

        private Slider FindThrottleUI()
        {
            foreach (var throt in GameObject.FindObjectsOfType<Slider>())
            {
                if (throt.name == "speedBar")
                {
                    return throt;
                }
            }
            return null;
        }
    }
}
