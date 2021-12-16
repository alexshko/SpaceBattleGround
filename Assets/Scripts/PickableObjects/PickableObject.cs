using Mirror;
using System;
using UnityEngine;
using System.Linq;

namespace SpaceBattle.core.Pickables
{
    public abstract class PickableObject : NetworkBehaviour
    {
        [Tooltip("Prefab to play when a player picked an item")]
        public Transform PickUpSpark;

        private AudioSource audioSource;
        [Tooltip("The audio sound to be played when an object is picked up")]
        public AudioClip pickUpSound;
        [SerializeField] private float soundRadius = 15;

        /// <summary>
        /// Action to be executed when the pickable object is picked up:
        /// </summary>
        /// <param name="trans">the spaceship that picked it</param>
        protected abstract void ExecuteActionOnPickUp(Transform trans);

        private void Start()
        {
            audioSource = GameObject.Find("SoundManager").GetComponent<AudioSource>();
        }

        [Server]
        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "Player")
            {
                //call a target function so only the spaceship that picked the object will make the sound:
                MakeSoundInRadius();
                Transform expl = Instantiate(PickUpSpark, transform.position, PickUpSpark.rotation);
                NetworkServer.Spawn(expl.gameObject);

                //execute the action of the picked up object in the spaceship Transform, has to be implemented in the derived class:
                ExecuteActionOnPickUp(collider.transform);

                //todo: check this:
                NetworkServer.UnSpawn(gameObject);
                Destroy(gameObject);
            }
        }

        private void MakeSoundInRadius()
        {
            //get all shootables within the range, then take only the spaceships (marked with tag "player") and call the TargetRPC with their NetworkConnection:
            Collider[] objectsAround = Physics.OverlapSphere(transform.position, soundRadius, LayerMask.NameToLayer("Shootable"));
            foreach (var spaceship in objectsAround.Where((c) => c.tag == "Player"))
            {
                Debug.Log("Make sound on client: " + spaceship.transform.GetComponent<NetworkIdentity>().connectionToClient);
                TargetMakeSoundInRadius(spaceship.transform.GetComponent<NetworkIdentity>().connectionToClient);
            } ;

        }

        [TargetRpc]
        private void TargetMakeSoundInRadius(NetworkConnection conn)
        {
            //todo: change this so will be played on all the clients:
            audioSource.PlayOneShot(pickUpSound);
        }
    }
}
