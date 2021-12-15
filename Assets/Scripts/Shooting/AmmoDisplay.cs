using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Shooting
{
    public class AmmoDisplay : MonoBehaviour
    {

        public LaserShootingMechanism localShootingMechanism;

        public void Update()
        {
            if(localShootingMechanism != null)
            {
                GetComponent<Text>().text = localShootingMechanism.curNumOfshots.ToString();
            }
           
        }
    }

}


