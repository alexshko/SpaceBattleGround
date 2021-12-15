using Mirror;
using SpaceBattle.Client.playfab;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBattle.core
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] TMP_Text txtSession;
        [SerializeField] TMP_Text txtfullGuid;
        public void Start()
        {
            if (txtSession)
            {
                txtSession.text = "Session: "+GameUser.singelton.sessionID;
            }
            if (txtfullGuid)
            {
                txtfullGuid.text = "Guid: " + GameUser.singelton.GuidID;
            }
        }
        public void GoToMainScreen()
        {
            //GameObject oldRoomManager = GameObject.FindObjectOfType<NetworkRoomManager>().gameObject;
            //SceneManager.MoveGameObjectToScene(oldRoomManager, SceneManager.GetActiveScene());
            ClientConnectMultiplayer.singelton?.GoToMainScreen();
        }

        public void GotoScreen(string reqScreen)
        {
            SceneManager.LoadScene(reqScreen);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }

}
