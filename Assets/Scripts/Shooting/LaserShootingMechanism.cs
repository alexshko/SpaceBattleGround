using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

namespace SpaceBattle.Shooting
{
    public class LaserShootingMechanism : NetworkBehaviour
    {
        [Tooltip("How far the locking mechanism can spot an enemy")]
        public float maxDistanceOfShoot = 1000;
        [Tooltip("Initial lasers amount")]
        public int numOfLasers = 100;
        [Tooltip("Which layers the lasers can hit")]
        public LayerMask shootableLayers;
        [Tooltip("If checked, can shoot firends and even himself when goes faster then the lasers")]
        public bool isFriendlyFireAccepted = false;

        private Button shootButtonRef;

        private Transform lockedTarget;
        public Transform LockedTarget
        {
            get => lockedTarget;
        }
        private bool isFiring;
        public bool IsFiring
        {
            get => isFiring;
        }

        //has to be public so when lasercanon shoots it will decrease it:
        [SyncVar]
        public int curNumOfshots;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            lockedTarget = null;
            curNumOfshots = numOfLasers;

            //update the ammodisplay:
            GameObject.FindObjectOfType<AmmoDisplay>().localShootingMechanism = this;
            shootButtonRef = findShootButton();
#if UNITY_STANDALONE
            shootButtonRef.gameObject.SetActive(false);
            //once we hidden it, no need to keep it.
            shootButtonRef = null;
#endif
        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            curNumOfshots = numOfLasers;
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                Ray castRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
#if UNITY_STANDALONE
                isFiring = Input.GetButton("Fire1");
#endif
#if UNITY_ANDROID
                isFiring = shootButtonRef.GetComponent<ShootButton>().isShooting;
#endif
                CmdSetFiringState(isFiring);


                Debug.DrawRay(castRay.origin, castRay.direction, Color.green, 0.5f);
                if (Physics.Raycast(castRay, out hit, maxDistanceOfShoot, shootableLayers))
                {
                    //fired and there is target in the distance:
                    startHomingOnTarget(hit.transform);
                }
                else
                {
                    //fired and there is nothing targetable in the distance:
                    startHomingOnTarget(null);
                }
            }
        }


        private void startHomingOnTarget(Transform transform)
        {
            if(lockedTarget != transform)
            {
                lockedTarget = transform;
                CmdSetTarget(lockedTarget);
            }
        }

        [Command]
        private void CmdSetTarget(Transform target)
        {
            lockedTarget = target;
        }

        [Command]
        private void CmdSetFiringState(bool fireState)
        {
            isFiring = fireState;
        }


        ////pickup of ammocell. has to be on the server:
        //[Server]
        //private void OnTriggerEnter(Collider collider)
        //{
        //    if (collider.gameObject.tag == "AmmoCell")
        //    {
        //        curNumOfshots += collider.GetComponent<PickUpObject>().numOfExtraShots;
        //    }
        //}


        private Button findShootButton()
        {
            foreach (var btn in GameObject.FindObjectsOfType<Button>())
            {
                if (btn.name == "ShootButton")
                {
                    return btn;
                }
            }
            return null;
        }
    }
}
