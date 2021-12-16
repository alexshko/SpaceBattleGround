using SpaceBattle.Shooting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBattle.core.Pickables
{
    public class AmmoAdditionPickable : PickableObject
    {
        [Tooltip("Amount of ammo to add")]
        [SerializeField] private int ammoToAdd = 100;

        protected override void ExecuteActionOnPickUp(Transform trans)
        {
            LaserShootingMechanism spaceship = trans.GetComponent<LaserShootingMechanism>();
            if (spaceship)
            {
                spaceship.curNumOfshots += ammoToAdd;
            }
        }
    }
}