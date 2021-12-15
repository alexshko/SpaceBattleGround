using UnityEngine;
using Mirror;
using SpaceBattle.Life;

namespace SpaceBattle.Shooting
{    public class LaserShot : NetworkBehaviour
    {

        private AudioSource audioSource;
        public AudioClip laserSound;


        //this is being by LaserCanon component that creates this prefab:
        public Transform target { get; set; }
        [Tooltip("The velocity of the shot")]
        public float velocityOfShot = 100;
        [Tooltip("Prefab of explosion to be activated in a hit")]
        public Transform explosionPref;
        [Tooltip("The amount of damage the shot does")]
        public int damageOfShot = 10;

        //the spaceship that shot the laser:
        public Transform whoShot { get; set; }
        //is ok to friendly fire:
        public bool isFriendlyFireAccepted;

        protected Rigidbody rb;
        Vector3 firstPositionOfTarget;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (!rb) Debug.LogError("Missing Rigidbody on the Shot");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (target)
            {
                firstPositionOfTarget = target.position;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            //make a sound on the client. will be called when the laser is created (=is shot) on the client.
            audioSource = GameObject.Find("SoundManager").GetComponent<AudioSource>();
            audioSource.PlayOneShot(laserSound);
        }

        //always calculate the direction to the target and change the velocity to be in that direction:
        [Server]
        private void Update()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            Vector3 dirToTarget = calcTargetDirection();
            rb.velocity = velocityOfShot * dirToTarget.normalized;
        }

        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Shootable"))
            {
                //if its the same one who shot the laser and friendly fire is not accepted, then ignore it.
                if (other.transform == whoShot && !isFriendlyFireAccepted) return;

                MakeExplosion();
                Debug.LogFormat("Hit {0}", other.gameObject.name);
                //if it has life, then it should decrease the life by damage:
                LifeEngine enemyLife = other.GetComponent<LifeEngine>();
                if (enemyLife)
                {
                    enemyLife.ServerTakeDamage(damageOfShot);
                }

                Destroy(this.gameObject);
            }
        }

        private void MakeExplosion()
        {
            Transform expl = Instantiate(explosionPref, transform.position, explosionPref.rotation);
            NetworkServer.Spawn(expl.gameObject);
            //todo: maybe need to unspawn it. maybe not because it its destroyed by itself on the client
        }

        //should be called when it is destroyed after a few seconds or when it hits a shootable object
        private void OnDestroy()
        {
            NetworkServer.UnSpawn(gameObject);
        }

        //to make other types of lasers (that follow the target), this method can be overriden
        protected virtual Vector3 calcTargetDirection()
        {
            Vector3 dirToTarget = transform.forward;
            if (target)
            {
                dirToTarget = firstPositionOfTarget - transform.position;
                dirToTarget = dirToTarget.normalized;
            }
            return dirToTarget;
        }
    }
}
