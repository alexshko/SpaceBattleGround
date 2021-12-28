using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Life
{
    public class HealthDisplay : MonoBehaviour
    {
        public static HealthDisplay singleton;

        private void Awake()
        {
            singleton = this;
        }

        [Tooltip("Reference to the life engine of the spaceship to display in the UI")]
        private LifeEngine lifeEngineLocalPlayer;
        public LifeEngine LifeEngineLocalPlayer
        {
            get => lifeEngineLocalPlayer;
            set
            {
                //if there is already LifeEngine and we change it, then unregister the function from the action:
                if (lifeEngineLocalPlayer?.actionClientOnChangeLife != null)
                {
                    lifeEngineLocalPlayer.actionClientOnChangeLife -= UpdateHealthDisplay;
                }
                lifeEngineLocalPlayer = value;
                //register the function for the new LifeBar:
                lifeEngineLocalPlayer.actionClientOnChangeLife += UpdateHealthDisplay;
                //init with current value:
                UpdateHealthDisplay(0, lifeEngineLocalPlayer.LifeRemain);
            }
        }

        //every time the life is changed in the local LifeEngine, update the health display:
        private void UpdateHealthDisplay(float oldLife, float newLife)
        {
            GetComponent<Text>().text = newLife.ToString();
        }

    }
}

