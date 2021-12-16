using SpaceBattle.Life;
using UnityEngine;

namespace SpaceBattle.core.Pickables
{
    public class HealthRestorePickable : PickableObject
    {
        [Tooltip("The max health that will be restored")]
        [SerializeField] private int amountHealth = 100;
        protected sealed override void ExecuteActionOnPickUp(Transform trans)
        {
            LifeEngine spaceship = trans.GetComponent<LifeEngine>();
            if (spaceship)
            {
                //if the max life possible is less than amountHealth, we should extend the max life:
                spaceship.MaxLife = Mathf.Max(spaceship.MaxLife, amountHealth);
                //restore amountHealth life:
                trans.GetComponent<LifeEngine>().LifeRemain = amountHealth;
            }
        }
    }
}
