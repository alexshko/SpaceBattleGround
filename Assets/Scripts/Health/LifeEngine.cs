using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using SpaceBattle.Shooting;

namespace SpaceBattle.Life
{
    public class LifeEngine : NetworkBehaviour
    {
        [Tooltip("The max life the player has")]
        public int MaxLife = 100;
        [Tooltip("Prefab for explosion effect")]
        public Transform ExplosionPrefab;
        [Tooltip("If there is Lifebar attached to it, it will update it")]
        public Slider LifeBarRef;

        //the action to preform when life gets to zero:
        public Action ActionOnDie=null;
        //action to perform on client when life changes:
        public Action<float,float> actionClientOnChangeLife=null;


        [SyncVar(hook =nameof(HookUpdatedLife))]
        [SerializeField] private float currentLife;
        public float LifeRemain
        {
            get => currentLife;
            [Server]
            set
            {
                if (isServer)
                currentLife = value;
            }
        }

        private void Start()
        {
            //when dies should destroy itself.
            ActionOnDie += ServerInflictDeath;
            //when hit, it should decrease the lifebar in all clients:
            actionClientOnChangeLife += UpdateLifeBar;
            currentLife = MaxLife;
        }



        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            //on the local prefab there should not be a lifebar on top of the spaceship:
            if (LifeBarRef)
            {
                LifeBarRef.gameObject.SetActive(false);
            }

            //update the display of health:
            HealthDisplay.singleton.LifeEngineLocalPlayer = this;
        }

        [Server]
        private void OnTriggerEnter(Collider other)
        {
                //if the object hit something or got hit by something that's not laser or another player, then it should be killed.
                if (other.GetComponent<LaserShot>() == null && other.gameObject.tag != "Player" && other.gameObject.tag != "HealingCell" && other.gameObject.tag != "AmmoCell")
                {
                    Debug.LogFormat("spaceship exploded by {0} in position {1}", other.name, other.transform.position);
                //has to be killed:
                    ServerTakeDamage(MaxLife + 1);
                }
        }

        [Server]
        public void ServerTakeDamage(float damage)
        {
            //reduce life, check if he needs to die.
            //ActionOnDie executes method ServerInflictDeath and it is possible to add more methods to it (that will be executed upon death)
            float newLife = Mathf.Clamp(currentLife - damage, 0, MaxLife);
            if (newLife <= 0)
            {
                //life over:
                if (ActionOnDie != null)
                {
                    ActionOnDie();
                    return;
                }
            }
            ////update the life bar on all clients
            //UpdateLifeBarIfExist(currentLife, newLife);
            currentLife = newLife;
        }

        //the function has to be on the server because its taking lives and should not be controlled by the player.
        [Command]
        public void CmdTakeDamage(float damage)
        {
            ServerTakeDamage(damage);
        }

        private void HookUpdatedLife(float oldLife, float newLife)
        {
            actionClientOnChangeLife?.Invoke(oldLife, newLife);
        }

        [Server]
        private void ServerInflictDeath()
        {
            //make explosion effect on all clients:
            Transform expl = Instantiate(ExplosionPrefab, transform.position, ExplosionPrefab.rotation);
            NetworkServer.Spawn(expl.gameObject);

            //tell the player to disconnect from server and unspawn him:
            TargetDisconnectWhenDie();
            NetworkServer.UnSpawn(gameObject);
            Destroy(gameObject);
        }

        [TargetRpc]
        public void TargetDisconnectWhenDie()
        {
            NetworkManager.singleton.StopClient();
        }

        private void UpdateLifeBar(float oldLife, float newLife)
        {
            if (LifeBarRef)
            {
                LifeBarRef.value = Mathf.Clamp01(newLife / MaxLife);
            }
        }

    }
}
