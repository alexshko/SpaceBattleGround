using System;
using System.Collections;
using UnityEngine;
using Mirror;
using SpaceBattle.Map;
using SpaceBattle.core;
using TMPro;
using UnityEngine.UI;

namespace SpaceBattle.Spaceship
{
    public enum SpaceshipMovementStatus { flying, stopping, hovering };

    public class SpaceshipMovement : NetworkBehaviour
    {
        [Header("Spaceship Flying Parameters")]
        public float forwardAcceleration = 1000;
        public float rollTorque = 100; //z-axis
        public float rollToStanceTorque = 10;
        public float pitchTorque = 100; //x-axis
        public float yawTorque = 10; //y-axis
#if UNITY_ANDROID
        [Tooltip("the move of the finger on the screen is calculated as if the player's screen is smaller by multiplying it by this parameter, thus makes it more sensetive")]
        [SerializeField] private float touchInputSensetivity = 0.3f;
#endif
        [Range(0, 200)]
        public float minSpeed = 5;
        [Range(0, 200)]
        public float maxSpeed = 100;
        //[Tooltip("The reference to the Acceleration throttle")]
        //public Transform speedForceMeterRef;

        [Header("Spaceship Hover Stance")]
        [Range(0, 20)]
        public float hoverAnimSeconds = 4;
        [Range(0, 10)]
        public float forceToApplyInHover = 10;


        private float speedAxis;
        private float rollAxis;
        private float pitchAxis;
        private float yawAxis;
        private Vector3 forceToApply;
        private Vector3 rollToApply;
        private Vector3 pitchToApply;
        private Vector3 yawToApply;
        private Rigidbody rb;
        private Coroutine standStillCoroutine = null;

        [Header("General")]
        [SerializeField] private SpaceshipMovementStatus moveStat;
        private bool isReachedStop = false;
        [SerializeField] private float speedMagnitude;
        private float epsilonAngleComparison = 2.0f;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isLocalPlayer)
            {
                //if the spaceship is current's player, we want to make him local on the minimap. if not, then he will get the default enemy value:
                GetComponent<MiniMapTarget>().type = TypeOfPinpoint.LocalPlayer;
            }
            GetComponent<MiniMapTarget>().enabled = true;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            forceToApply = Vector3.zero;
            rb = GetComponent<Rigidbody>();
            if (!rb)
            {
                Debug.LogError("missing rigidbody on the spaceship");
            }
            //set initial speed:
            rb.velocity = minSpeed * transform.forward;
            moveStat = SpaceshipMovementStatus.flying;
            Cursor.visible = false;

            //make the main camera to follow only this local object
            Camera.main.GetComponent<CamFollowScript>().objToFollow = transform ;
            //set the UI speed indicator to show the velocity of the ship from Rigidbody:
            SetSpeedIndicator();
        }

        private void SetSpeedIndicator()
        {
            GameObject.FindObjectOfType<UISpeedIndicator>().rbSpaceship = rb;
        }

        private void CalcAxis()
        {
            //decided not to roll, but the functionality is there:
            //rollAxis = Input.GetAxis("Horizontal"); //left&right
            
            //adjust the speed axis so it will go full untill it reaches the right portion of the speed:
            speedAxis = GetComponent<AccelerationAxisEngine>().AccelerationAxis;
#if UNITY_STANDALONE    
            pitchAxis = -Input.GetAxis("Mouse Y");
            yawAxis = Input.GetAxis("Mouse X");
#endif

#if UNITY_ANDROID
            if (Input.touchCount >= 1)
            {
                pitchAxis = Mathf.Clamp(-Input.touches[0].deltaPosition.y / (Screen.height * touchInputSensetivity), -1, 1);
                yawAxis = Mathf.Clamp(Input.touches[0].deltaPosition.x / (Screen.width * touchInputSensetivity), -1, 1);
            }
            else
            {
                pitchAxis = yawAxis = 0;
            }
#endif
        }

        void Update()
        {
            if (isLocalPlayer)
            {
                //init the variables:
                CalcAxis();
                rollToApply = Vector3.zero;
                pitchToApply = Vector3.zero;
                //used for checking if the spaceship reached to full stop when it's on stopping status:
                isReachedStop = false;

                //incase its not hovering anymore, we should cancel the hovering coroutine and init the forward force.
                //becuse the parameters are recalculated every frame when not hovering.
                if (moveStat != SpaceshipMovementStatus.hovering)
                {
                    forceToApply = Vector3.zero;
                    CancelHoverStanceCoroutine();
                }

                if (moveStat == SpaceshipMovementStatus.flying)
                {
                    calcForwardForceToApply();
                    //rollToApply = rollAxis * rollTorque * (-transform.forward);
                    pitchToApply = pitchAxis * pitchTorque * transform.right;
                    yawToApply = yawAxis * yawTorque * transform.up;
                }
                else if (moveStat == SpaceshipMovementStatus.stopping)
                {
                    //put the acceleration meter to nuetral position:
                    GetComponent<AccelerationAxisEngine>().SetAccelerationMeter(0, updateUI:true);
                    speedAxis = GetComponent<AccelerationAxisEngine>().AccelerationAxis;

                    //if he is in hovering mode, then should apply force to stop and torquqe to roll to identity stance:
                    bool stoppedForce = calcForwardForceToStance();
                    bool stoppedRot = calcRotationToHoverStance();
                    isReachedStop = stoppedForce && stoppedRot;
                }
                else if (moveStat == SpaceshipMovementStatus.hovering)
                {
                    ActivateHoverStanceCoroutine();
                    yawToApply = yawAxis * yawTorque * transform.up;
                    pitchToApply = pitchAxis * pitchTorque * transform.right;
                }
                else
                {
                    //incase later added new status and didn't take it into account.
                    throw new InvalidMoveStatusException(moveStat);
                }

                updateHoverStatus();
                //update the speed for debugging purpses:
                speedMagnitude = rb.velocity.magnitude;
            }
        }

        private void ActivateHoverStanceCoroutine()
        {
            if (standStillCoroutine == null)
            {
                standStillCoroutine = StartCoroutine(stanceStillCoroutine());
            }
        }
        private void CancelHoverStanceCoroutine()
        {
            if (standStillCoroutine != null)
            {
                StopCoroutine(standStillCoroutine);
                standStillCoroutine = null;
            }
        }

        private IEnumerator stanceStillCoroutine()
        {
            forceToApply = -forceToApplyInHover * Vector3.up;
            yield return new WaitForSeconds(hoverAnimSeconds / 4);
            while (true)
            {
                forceToApply = forceToApplyInHover * Vector3.up;
                yield return new WaitForSeconds(hoverAnimSeconds / 2);
                forceToApply = -forceToApplyInHover * Vector3.up;
                yield return new WaitForSeconds(hoverAnimSeconds / 2);
            }

        }

        //return true if got to the correct speed:
        private bool calcForwardForceToStance()
        {
            //todo:
            //may cause bug, try changing isStopped = rb.velocity but in z-localspace
            bool isStopped = (rb.velocity.magnitude < 0.1f);
            if (!isStopped)
            {
                //make force in the direction oppsite to the current velocity so it will come to stop.
                Vector3 directionToApply = -rb.velocity.normalized;
                forceToApply = forwardAcceleration * directionToApply;
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
            return isStopped;
        }

        //return true if got to the correct rotation
        //used stopping state, because we can't be sure what angle the spacehip is at when starts to stop:
        private bool calcRotationToHoverStance()
        {
            float angleDeg = transform.localEulerAngles.z;
            if (angleDeg < epsilonAngleComparison || angleDeg > 360- epsilonAngleComparison)
            {
                rollToApply = Vector3.zero;
                return true;
            }
            else if (angleDeg < 180)
            {
                rollToApply = rollToStanceTorque * (-transform.forward);
                return false;
            }
            else
            {
                rollToApply = rollToStanceTorque * transform.forward;
                return false;
            }
        }

        //calculate the correct status of the ship movement:
        private void updateHoverStatus()
        {
            if (Input.GetButtonDown("Hover"))
            {
                if (moveStat == SpaceshipMovementStatus.flying)
                {
                    moveStat = SpaceshipMovementStatus.stopping;
                }
                else if (moveStat == SpaceshipMovementStatus.hovering || moveStat == SpaceshipMovementStatus.stopping)
                {
                    moveStat = SpaceshipMovementStatus.flying;
                }
            }

            //isReachedStop calculated in method that happens during stopping status:
            else if (isReachedStop)
            {
                moveStat = SpaceshipMovementStatus.hovering;
            }
        }

        private void calcForwardForceToApply()
        {
            float forceCoffecient = 0;
            float reqSpeed = speedAxis * maxSpeed;
            if (rb.velocity.magnitude < minSpeed)
            {
                //to gain minimum speed after it went from hovering to flying mode:
                forceCoffecient = 1;
            }
            else
            {
                //forceCoffecient according to the required speed.
                //0=>minspeed. 1=>maxspeed.
                forceCoffecient = (reqSpeed - rb.velocity.magnitude) / (maxSpeed - minSpeed);
            }
            //apply force to increase velocity
            forceToApply = forceCoffecient * forwardAcceleration * transform.forward;

            //the velocity is increased in forward direction,  but the spaceship's velocity is in previous forward direction (when going up),
            //so we have to adjust the velocity direction:
            rb.velocity = rb.velocity.magnitude * transform.forward;
            rb.velocity = ClampVector(rb.velocity, minSpeed, maxSpeed);
        }

        private static Vector3 ClampVector(Vector3 vec, float minVal, float maxVal)
        {
            if (vec.magnitude >= maxVal)
            {
                return vec.normalized * maxVal;
            }
            if (vec.magnitude <= minVal)
            {
                return vec.normalized * minVal;
            }
            return vec;
        }

        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                //bugfix: make sure it doesn't rotate on the z-axis.
                rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, 0));

                rb.AddForce(forceToApply, ForceMode.Force);
                rb.AddTorque(pitchToApply+yawToApply, ForceMode.Force);
                //rb.AddTorque(rollToApply, ForceMode.Force);
                Debug.Log("Pitch: " + pitchToApply);
            }
        }

        /// <summary>
        /// incase its a client, then when the player is destroyed he should disconnect:
        /// </summary>
        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                //todo: check if this is still needed:
                if (isClient)
                {
                    NetworkManager.singleton.StopClient();
                }
                Cursor.visible = true;
            }
        }
    }

    public class InvalidMoveStatusException : Exception
    {
        public InvalidMoveStatusException(SpaceshipMovementStatus moveStat) : base("wrong paramteter to move status of the spaceship: " + moveStat.ToString())
        {
        }
    }
}
