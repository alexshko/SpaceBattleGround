using System;
using UnityEngine;
using Mirror;

namespace SpaceBattle.Shooting
{   
    public class LaserCanon : NetworkBehaviour
    {
        [Tooltip("Number of shots per second")]
        public int shotsPerSecond = 2;
        [Tooltip("preb of the shot to be spawned")]
        public Transform LaserShotPref;
        [Tooltip("How long the shot will last, unless it hits something")]
        public float lifeOfShot;
        [Tooltip("The initial position and rotation of the shot")]
        public Transform positionToShootFrom;


        private LaserShootingMechanism shootMech;
        private float lastShotTime;

        public override void OnStartServer()
        {
            base.OnStartServer();

            lastShotTime = 0;
            shootMech = GetComponent<LaserShootingMechanism>();
            if (!shootMech)
            {
                throw new MissingMechanismException();
            }
        }

        [Server]
        private void Update()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            //shootMech holds information if there is locked target, if fired and number of shots:
            Transform homingTarget = shootMech.LockedTarget;
            bool isFiring = shootMech.IsFiring;
            int numOfShotsLeft = shootMech.curNumOfshots;

            if (isFiring && (Time.time > lastShotTime + 1.0f / shotsPerSecond) && numOfShotsLeft>0)
            {
                Shoot(homingTarget);
                lastShotTime = Time.time;
                shootMech.curNumOfshots--;
            }
        }

        [Server]
        private void Shoot(Transform shootTarget)
        {
            //bugfix: the spaceship is controlled by the client and synced
            //position the laser far enough from the spaceship so it will not collide with it when synced
            Transform laserShot = Instantiate(LaserShotPref, positionToShootFrom.position + 1.1f * positionToShootFrom.forward, positionToShootFrom.rotation);
            if (!laserShot) return;

            laserShot.GetComponent<LaserShot>().target = shootTarget;
            laserShot.GetComponent<LaserShot>().whoShot = transform;
            laserShot.GetComponent<LaserShot>().isFriendlyFireAccepted = shootMech.isFriendlyFireAccepted;
            NetworkServer.Spawn(laserShot.gameObject);

            Destroy(laserShot.gameObject, lifeOfShot);

        }
    }

    public class MissingMechanismException : Exception
    {
        public MissingMechanismException() : base("Couldn't find a shootingMechanism in all parents")
        {
        }
    }
}
