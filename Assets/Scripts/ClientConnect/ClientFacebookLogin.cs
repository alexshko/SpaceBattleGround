using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using SpaceBattle.Server.playfab;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceBattle.Client.playfab
{
    public delegate void OutputErrorChannel(string message);

    public class ClientFacebookLogin : MonoBehaviour
    {
        [Tooltip("Reference to the panel of the login.")]
        [SerializeField] GameObject loginPanel;
        [Tooltip("Reference to the log panel")]
        [SerializeField] TMP_Text logPanelRef;

        OutputErrorChannel PrintToMessageChannels=null;


        private static ClientFacebookLogin singleton;
        public static ClientFacebookLogin Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new ClientFacebookLogin();
                }
                return singleton;
            }
            set
            {
                singleton = value;
            }
        }

        #region facebook login:

        private void Awake()
        {
            Singleton = this;
        }
        private void Start()
        {
            PrintToMessageChannels += (message => Debug.Log(message));
            PrintToMessageChannels += (message => logPanelRef.text += "\n" + message);
        }

        /// <summary>
        /// called from facebook login button:
        /// </summary>
        public void NewLoginFacebook()
        {
            if (!PlayfabSettings.singleton.isServerInstance)
            {
                if (PlayfabSettings.singleton.isRemote)
                {
                    if (!FB.IsInitialized)
                    {
                        FB.Init(onInitComplete: () =>
                        {
                            FB.LogInWithReadPermissions(new List<string> { "public_profile", "email", "user_friends" }, LoginFacebookResultFunc);
                        });
                    }
                    else
                    {
                        FB.LogInWithReadPermissions(new List<string> { "public_profile", "email", "user_friends" }, LoginFacebookResultFunc);
                    }
                }
                else
                {
                    ClientCustomLogin.ContinueAfterLogin();
                }
            }
        }

        /// <summary>
        /// Check if log in to facebook succeeded. if yes, then link the account to the current device
        /// </summary>
        /// <param name="result">the result of the attempt to log in</param>
        private void LoginFacebookResultFunc(ILoginResult result)
        {
            if (result == null || string.IsNullOrEmpty(result.Error))
            {
                //Success. Use the token retrieved from the facebook login to connect to playfab:
                PlayfabFacebookLogin(AccessToken.CurrentAccessToken.TokenString);
            }
            else
            {
                PrintToMessageChannels("Falied to login with facebook");
            }
        }
        /// <summary>
        /// link facebook account to the current playfab user, if it's already connected to other user, then change it's device's id to this one
        /// </summary>
        /// <param name="token">facebook token</param>
        private void PlayfabFacebookLogin(string token)
        {
            //first try to link this device to facebook's account:
            LinkFacebookAccountRequest req = new LinkFacebookAccountRequest { AccessToken = token };
            PlayFabClientAPI.LinkFacebookAccount(req, LinkDeviceToFacebookSuccess,
                err =>
                {
                    switch (err.Error) {
                        //if the error happens because the facebook user already connected to other device, then connect this device to
                        //the user with the facebbok accoount
                        case PlayFabErrorCode.LinkedAccountAlreadyClaimed:
                            LinkThisDeviceToAcclaimedAccount(token);
                            break;

                        default:
                            PrintToMessageChannels("something went wrong: " + err.ErrorMessage);
                            break;
                    }
                });
        }
        private void LinkDeviceToFacebookSuccess(LinkFacebookAccountResult res)
        {
            PrintToMessageChannels("Facebook account linked successfully");
            GameUser.singelton.isFaceBookRegistered = true;
            UpdateDisplayNameFacebook();
        }

        private void LinkThisDeviceToAcclaimedAccount(string token)
        {
            //we link the entered facebook account to the current playfab id:
            LoginWithFacebookRequest faceReq = new LoginWithFacebookRequest { AccessToken = token, TitleId = PlayFabSettings.TitleId };
            PlayFabClientAPI.LoginWithFacebook(faceReq, 
                res => {
                    //after logged in to the playfab user with the facebook account, change it's device id to the current device' customID
                    ChangeDeviceIDForPlayFabAccount(GameUser.singelton.deviceID, res.PlayFabId);
                }, 
                err => {
                    PrintToMessageChannels("Failed to link the device to the facebook account");
                });
        }

        /// <summary>
        ///after logged in to the playfab user with the facebook account, change it's device id to the current device' customID
        /// </summary>
        /// <param name="customId">custom id of the new device</param>
        /// <param name="playfabID">the playfabid of the user with registerred facebook account</param>
        private void ChangeDeviceIDForPlayFabAccount(string customId, string playfabID)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //todo: add here for android device:
                LinkAndroidDeviceIDRequest linkReq = new LinkAndroidDeviceIDRequest { AndroidDeviceId = customId, ForceLink = true };
                PlayFabClientAPI.LinkAndroidDeviceID(linkReq,
                    res => ChangeDeviceIDSuccess(playfabID),
                    err => PrintToMessageChannels("Problem with linking the device to the facebook acount"));

            }
            else
            {
                LinkCustomIDRequest linkReq = new LinkCustomIDRequest { CustomId = customId, ForceLink=true };
                PlayFabClientAPI.LinkCustomID(linkReq,
                    res => ChangeDeviceIDSuccess(playfabID),
                    err => PrintToMessageChannels("Problem with linking the device to the facebook acount"));
            }
        }

        //todo: make it suitable for action upstairs:
        /// <summary>
        /// After changed succesfuly the acclaimed acount to the new devices' id, update the userID in the <see cref="GameUser"/> singleton and also
        /// the display name.
        /// </summary>
        /// <param name="playfabID"></param>
        private void ChangeDeviceIDSuccess(string playfabID)
        {
            PrintToMessageChannels("linked the device to the facebook account");
            GameUser.singelton.playFabId = playfabID;
            GetDisplayNameAndGoToLobby();
        }

        /// <summary>
        /// update the display name. called after logged to facebook:
        /// </summary>
        private void UpdateDisplayNameFacebook()
        {
            FB.API("/me?fields=id,name,email", Facebook.Unity.HttpMethod.GET, UpdateNameFromFacebookData);
        }

        private void UpdateNameFromFacebookData(IGraphResult result)
        {
            string newDispName = result.ResultDictionary["email"].ToString().Split('@')[0];
            //update the display name and then go to next screen:
            ClientCustomLogin.PlayfabUpdateDisplayName(newDispName, 
                actionOnFinishUpdate: ()=> 
                {
                    GameUser.singelton.displayName = newDispName;
                    PrintToMessageChannels("Updated the display name from facebook");
                    LoginLater();
                },
                actionOnFail: ()=>
                {
                    PrintToMessageChannels("Failed to use the display name of facebook acount.");
                    LoginLater();
                });
        }

        private void GetDisplayNameAndGoToLobby()
        {
            ClientCustomLogin.PlayfabGetUserInfo(actionOnGetDisplayName: dispName=> {

                //update the display name in the singleton:
                GameUser.singelton.displayName = dispName;
            }, actionIfLoggedToFacebook: LoginLater, null);
        }

        public void LoginLater()
        {
            ClientCustomLogin.ContinueAfterLogin();
        }

        public void ActivateLoginPanelIfNeeded()
        {
            if (GameUser.singelton.isLoggenIn && !GameUser.singelton.isFaceBookRegistered)
            {
                //todo: pop up login menu.
                if (!loginPanel.activeInHierarchy)
                {
                    loginPanel.SetActive(true);
                }
            }
        }

        #endregion
    }
}
