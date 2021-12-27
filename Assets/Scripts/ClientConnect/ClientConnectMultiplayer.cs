using kcp2k;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using SpaceBattle.Server.playfab;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBattle.Client.playfab
{
    public class ClientConnectMultiplayer : MonoBehaviour
    {
        public string ip;
        public List<Port> listOfPorts;
        [SerializeField] private TMP_Text txtSession;
        [SerializeField] private TMP_Text txtDisplayName;
        public bool isHostOfGame = false;

        public static ClientConnectMultiplayer singelton;

        private void OnEnable()
        {
            if (!singelton)
            {
                singelton = this;
            }
        }

        
        public void CreateMultiplyaerGame()
        {
            isHostOfGame = true;
            CreateMultiplyaerGame("");
        }

        public void JoinMultiplayerGame()
        {
            isHostOfGame = false;
            string sessionIdInit = "";
            sessionIdInit = txtSession?.text;
            CreateMultiplyaerGame(sessionIdInit);
        }
        private void CreateMultiplyaerGame(string password="")
        {
            if (!PlayfabSettings.singleton.isServerInstance)
            {
                if (PlayfabSettings.singleton.isRemote)
                {
                    MakeRequestToServer(password);
                }
                else
                {
                    NetworkManager.singleton.StartClient();
                }
            }
        }


        private void MakeRequestToServer(string sessionIdInit="")
        {
            RequestMultiplayerServerRequest req = new RequestMultiplayerServerRequest();

            req.BuildId = PlayfabSettings.singleton.buildId;
            if (string.IsNullOrEmpty(sessionIdInit))
            {
                sessionIdInit = GenerateRandomPassword();
            }
            req.SessionId = CreateHashedGUID(sessionIdInit);
            //put the session 8 chars code in the variable:
            GameUser.singelton.GuidID = req.SessionId;
            GameUser.singelton.sessionID = sessionIdInit;

            List<string> regions = new List<string>();
            regions.Add(AzureRegion.NorthEurope.ToString());
            req.PreferredRegions = regions;

            PlayFabMultiplayerAPI.RequestMultiplayerServer(req, reqServerSuccess, ErrorHandle);
        }

        private void reqServerSuccess(RequestMultiplayerServerResponse response)
        {
            Debug.Log("Successfully logged to server");
            ip = response.IPV4Address;
            listOfPorts = response.Ports;

            NetworkManager.singleton.networkAddress = ip;
            NetworkManager.singleton.GetComponent<KcpTransport>().Port = (ushort)listOfPorts.Find(p => p.Name == "port7777").Num;


            NetworkManager.singleton.StartClient();

        }
        private void ErrorHandle(PlayFabError error)
        {
            Debug.Log("Error logging to server: " + error.ErrorMessage);
        }

        //create a guid for the playfab request from a password, by using MD5 hash:
        private string CreateHashedGUID(string password)
        {
            byte[] sessionIdBytes = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(password.Substring(0, 8)));
            return new Guid(sessionIdBytes).ToString();
        }

        private string GenerateRandomPassword()
        {
            string result = System.Guid.NewGuid().ToString("n").Substring(0, 8);
            return result;
        }

        public void GoToMainScreen()
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Lobby");
        }

        private void Start()
        {
            //GetPlayerProfileRequest request = new GetPlayerProfileRequest() {
            //    PlayFabId = GameUser.singelton.playFabId,
            //    ProfileConstraints = new PlayerProfileViewConstraints() { ShowDisplayName = true }
            //};
            //PlayFabClientAPI.GetPlayerProfile(request, res => { Debug.Log("User: " + res.ToJson());}, err => { Debug.Log("Coudn't retrieve user info"); });
            if (txtDisplayName)
            {
                txtDisplayName.text = GameUser.singelton.displayName;
            }
        }

    }

    
}
