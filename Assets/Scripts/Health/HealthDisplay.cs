using UnityEngine;
using UnityEngine.UI;


namespace SpaceBattle.Life
{
    public class HealthDisplay : MonoBehaviour
    {

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

