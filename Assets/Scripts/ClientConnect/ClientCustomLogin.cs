using PlayFab;
using PlayFab.ClientModels;
using SpaceBattle.Server.playfab;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBattle.Client.playfab
{
    public enum AppLoginType { Custom, Android };
    public class ClientCustomLogin : MonoBehaviour
    {
        static string loginTypeString = "spacebattle.logintype";
        [SerializeField] private TMPro.TMP_Text errorConsol;

        private void Start()
        {
            if (PlayfabSettings.singleton.isServerInstance)
            {
                //for server instance we go straight to the lobby:
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                //if it's client:
                if (PlayfabSettings.singleton.isRemote)
                {
                    //frictionless login implementation:
                    bool isLoginSaved = CheckIfSavedLoggedIn();
                    bool isLoggedIn = CheckIFLoggedIn();
                    //if the user is not yet logged in, check if he has saved credentials.
                    if (!isLoggedIn)
                    {
                        //if has saved credentials, use them to log in.
                        if (isLoginSaved)
                        {
                            LoginFromSavedPrefs();
                        }
                        //if credentials are not saved, then log in as new user:
                        else
                        {
                            NewLoginAnnonimsly();
                        }
                    }
                }
                else
                {
                    ContinueAfterLogin();
                }
            }
        }

        private bool CheckIfSavedLoggedIn()
        {
            return PlayerPrefs.HasKey(loginTypeString);
        }

        private bool CheckIFLoggedIn()
        {
            return GameUser.singelton.isLoggenIn;
        }

        #region Annonimous login functions:

        /// <summary>
        /// make login from the credentials saved in player prefs, according to the device type.
        /// the user already exists in the DB.
        /// </summary>
        private void LoginFromSavedPrefs()
        {
            string logType = PlayerPrefs.GetString(loginTypeString);
            if (logType == AppLoginType.Android.ToString())
            {
                PlayfabLoginAnnonimslyAndroid(newAccount: false);
            }
            else if (logType == AppLoginType.Custom.ToString())
            {
                string id = PlayerPrefs.GetString("customID");
                PlayfabLoginAnnonimslyCustomID(newAccount: false, userID: id);
            }
        }

        /// <summary>
        /// make new user login according to device type
        /// the user will be created in the DB and saved to playerprefs
        /// </summary>
        private void NewLoginAnnonimsly()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //sign in with adnroid device id:
                PlayfabLoginAnnonimslyAndroid(newAccount: true);
            }
            else
            {
                //sign in with custom id:
                string userID = GenerateCustomID();
                PlayfabLoginAnnonimslyCustomID(newAccount: true, userID);
            }
        }

        /// <summary>
        /// helper function for log in by custom id
        /// </summary>
        /// <param name="newAccount">if true it will create a new user</param>
        /// <param name="userID"> the id of the device linked to the user</param>
        private void PlayfabLoginAnnonimslyCustomID(bool newAccount, string userID)
        {
            LoginWithCustomIDRequest req = new LoginWithCustomIDRequest { CreateAccount = newAccount, CustomId = userID};
            PlayFabClientAPI.LoginWithCustomID(req, res=> {
                Debug.Log("connected with custom id: " + userID);
                UpdateUserDisplayName(res.NewlyCreated, res.PlayFabId);
                //save the data of the login for next time:
                //if (newAccount)
                //{
                //    PlayerPrefs.SetString(loginTypeString, AppLoginType.Custom.ToString());
                //    PlayerPrefs.SetString("customID", userID);
                //}

                ////update the device id in the user's singleton:
                //GameUser.singelton.deviceID = userID;

                SaveDataOfUser(AppLoginType.Custom, userID, playfabID: res.PlayFabId);
            }, HandleLoginError);
        }

        /// <summary>
        /// helper function for log in by android device unique id
        /// </summary>
        /// <param name="newAccount">if true it will create a new user</param>

        private void PlayfabLoginAnnonimslyAndroid(bool newAccount)
        {
#if UNITY_ANDROID
            //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
            AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
            string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");

            LoginWithAndroidDeviceIDRequest req = new LoginWithAndroidDeviceIDRequest() { CreateAccount = newAccount, AndroidDeviceId = android_id };
            PlayFabClientAPI.LoginWithAndroidDeviceID(req, res =>
            {
                UpdateUserDisplayName(res.NewlyCreated, res.PlayFabId);
                Debug.Log("Connected with Android device: "+ android_id);
                //if (newAccount)
                //{
                //    PlayerPrefs.SetString(loginTypeString, AppLoginType.Android.ToString());
                //}

                ////update the device id in the user's singleton:
                //GameUser.singelton.deviceID = android_id;
                SaveDataOfUser(AppLoginType.Android, android_id, playfabID: res.PlayFabId);
            }
                , HandleLoginError);
#endif
        }

        private void HandleLoginError(PlayFabError err)
        {
            Debug.Log("something went wrong: " + err.ErrorMessage);
            errorConsol.text += "something went wrong: " + err.ErrorMessage;
        }

        /// <summary>
        /// after login, check for display name and update it to the <see cref="GameUser"/>.
        /// <br/>
        /// if it's new player the display name will be concatanated of <paramref name="userID"/>
        /// </summary>
        /// <param name="newlyCreated">is it new user</param>
        /// <param name="userID">the id of the user</param>
        private void UpdateUserDisplayName(bool newlyCreated, string userID)
        {
            //GameUser.singelton.playFabId = res.PlayFabId;
            
            //if we just created the player, then update his display name:
            if (newlyCreated)
            {
                string newUserDispName = "Spaceship " + userID.Substring(0, 4);
                PlayfabUpdateDisplayName(newUserDispName, actionOnFinishUpdate: ()=> {
                    GameUser.singelton.displayName = newUserDispName;
                    popUpLoginScreen();
                });
            }
            else
            {
                //if it's not new user, then retrieve it's display name.
                //also check if he's has facebook account linked, if yes then finish login process:
                PlayfabGetUserInfo(actionOnGetDisplayName: 
                    dispName =>
                    {
                        GameUser.singelton.displayName = dispName;
                    }
                ,actionIfLoggedToFacebook:ContinueAfterLogin, actionIfNotLoggedToFacebook: popUpLoginScreen);
            }
        }

        private void popUpLoginScreen()
        {
            ClientFacebookLogin.Singleton.ActivateLoginPanelIfNeeded();
        }

        #endregion

        #region generic functions
        private void SaveDataOfUser(AppLoginType loginType, string deviceID, string playfabID = null)
        {
            PlayerPrefs.SetString(loginTypeString, loginType.ToString());

            GameUser.singelton.deviceID = deviceID;
            PlayerPrefs.SetString("customID", deviceID);

            if (!string.IsNullOrEmpty(playfabID))
            {
                GameUser.singelton.playFabId = playfabID;
            }
        }

        public static void PlayfabUpdateDisplayName(string dispName, Action actionOnFinishUpdate=null, Action actionOnFail=null)
        {
            UpdateUserTitleDisplayNameRequest dispNameChangeReq = new UpdateUserTitleDisplayNameRequest { DisplayName = dispName };
            PlayFabClientAPI.UpdateUserTitleDisplayName(dispNameChangeReq, 
                res => {
                    actionOnFinishUpdate?.Invoke();
                },
                err =>
                {
                    Debug.LogWarning("Couldn't update the display name: "+err.ErrorMessage);
                    actionOnFail?.Invoke();
                });
        }

        public static void PlayfabGetUserInfo(Action<string> actionOnGetDisplayName=null, Action actionIfLoggedToFacebook=null, Action actionIfNotLoggedToFacebook=null)
        {
            GetAccountInfoRequest accountInfoReq = new GetAccountInfoRequest { PlayFabId = GameUser.singelton.playFabId };
            PlayFabClientAPI.GetAccountInfo(accountInfoReq,
                res => {
                    string dispName = res.AccountInfo.TitleInfo.DisplayName;
                    actionOnGetDisplayName?.Invoke(dispName);

                    //if facebook is registered:
                    if (res.AccountInfo.FacebookInfo!=null)
                    {
                        GameUser.singelton.isFaceBookRegistered = true;
                        actionIfLoggedToFacebook?.Invoke();
                    }
                    else
                    {
                        actionIfNotLoggedToFacebook?.Invoke();
                    }
                }, err => {
                    Debug.LogWarning("Coud not fetch the user's account info" + err.ErrorMessage);
                });
        }
        /// <summary>
        /// Called after Login is finished, if it's in the Login scene we should load the Libby scene.
        /// However if it's some other scene, there should be no change.
        /// </summary>
        public static void ContinueAfterLogin()
        {
            if (SceneManager.GetActiveScene().name == "Login")
            {
                SceneManager.LoadScene("Lobby");
            }
        }

        private string GenerateCustomID()
        {
            string tempID = Guid.NewGuid().ToString();
            return tempID.Substring(0, 8);
        }
        #endregion
    }

    public class GameUser
    {
        public string playFabId = "";
        public string deviceID = "";
        public string displayName = "";
        public bool isFaceBookRegistered = false;

        #region variables for multiplayer game:
        public string sessionID = "";
        public string GuidID = "";
        #endregion

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
        public bool isLoggenIn
        {
            get => !string.IsNullOrEmpty(singelton.playFabId);
        }
    }
}
