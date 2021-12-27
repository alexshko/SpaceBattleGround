using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Life
{
    public class HealthDisplay : MonoBehaviour
    {
        [Tooltip("Reference to the life engine of the spaceship to display in the UI")]
        public LifeEngine lifeEngineLocalPlayer;

        public void Update()
        {
            if(lifeEngineLocalPlayer != null)
            {
                GetComponent<Text>().text = lifeEngineLocalPlayer.LifeRemain.ToString();
            }
        }
    }
}

