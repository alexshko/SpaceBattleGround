using PlayFab;
using PlayFab.ClientModels;
using SpaceBattle.Server.playfab;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceBattle.Client.playfab
{
    public class ClientLoginPlayfab : MonoBehaviour
    {
        [SerializeField] private InputField email;
        [SerializeField] private InputField password;
        [SerializeField] private InputField displayName;
        [SerializeField] private TMPro.TMP_Text errorConsol;

        private void Start()
        {
            if (PlayfabSettings.singleton.isServerInstance)
            {
                SceneManager.LoadScene("Lobby");
            }
        }

        public void Login()
        {
            if (!PlayfabSettings.singleton.isServerInstance)
            {
                if (PlayfabSettings.singleton.isRemote)
                {
                    if (string.IsNullOrEmpty(email.text) || string.IsNullOrEmpty(password.text) || (displayName && string.IsNullOrEmpty(displayName.text)))
                    {
                        errorConsol.text += "\nMissing Parameter";
                        Debug.Log("Missing Parameter");
                        return;
                    }
                    var requestByMail = new LoginWithEmailAddressRequest { Email = email.text, Password = password.text, InfoRequestParameters=new GetPlayerCombinedInfoRequestParams() { GetPlayerProfile=true} };
                    PlayFabClientAPI.LoginWithEmailAddress(requestByMail, OnLoginSuccess, error=> { errorConsol.text += error.ErrorMessage; });
                }
                else
                {
                    ContinueAfterLogin();
                }
            }
        }

        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Congratualtions, successfuly logged in: " + result.PlayFabId);
            GameUser.singelton.playFabId = result.PlayFabId;
            GameUser.singelton.userID = result.InfoResultPayload.PlayerProfile.DisplayName;
            ContinueAfterLogin();
        }


        public void registerNewPlayer()
        {
            if (!PlayfabSettings.singleton.isServerInstance)
            {
                if (PlayfabSettings.singleton.isRemote)
                {
                    if (!email || !password || !displayName)
                    {
                        errorConsol.text += "\nMissing Parameter";
                        Debug.Log("Missing Parameter");
                        return;
                    }
                    RegisterPlayFabUserRequest req = new RegisterPlayFabUserRequest { DisplayName = displayName.text, Email = email.text, Password = password.text, RequireBothUsernameAndEmail = false, InfoRequestParameters = new GetPlayerCombinedInfoRequestParams() { GetPlayerProfile = true } };
                    PlayFabClientAPI.RegisterPlayFabUser(req, OnRegisterSuccess, OnRegisterFailure);
                }
                else
                {
                    ContinueAfterLogin();
                }
            }
        }
        private void OnRegisterSuccess(RegisterPlayFabUserResult res)
        {
            Debug.Log("Registered successfully");
            GameUser.singelton.playFabId = res.PlayFabId;
            GameUser.singelton.userID = displayName.text;
            ContinueAfterLogin();
        }
        private void OnRegisterFailure(PlayFabError error)
        {
            Debug.Log("Failed to register.");
            errorConsol.text += "\nFailed to register new user... " + error.Error;
            Debug.Log(error.Error);
        }


        private void ContinueAfterLogin()
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public class GameUser
    {
        public string playFabId = "";
        public string userID = "";
        public string sessionID = "";
        public string GuidID = "";

        private static GameUser instance;
        public static GameUser singelton
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameUser();
                }
                return instance;
            }
        }
    }
}
