﻿using Mirror;
using SpaceBattle.Client.playfab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattle.Client
{
    public class MyNetworkRoomPlayer : NetworkRoomPlayer
    {
        [SyncVar(hook ="UpdateDisplayNameInUI")]
        [SerializeField]
        string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                CmdUpdateDisplayName(value);
            }
        }
        [Command]
        private void CmdUpdateDisplayName(string newDispName)
        {
            displayName = newDispName;
        }
        void UpdateDisplayNameInUI(string oldValue, string newValue)
        {
            foreach (var item in GetComponentsInChildren<TMP_Text>())
            {
                if (item.gameObject.name == "txtDisplayName")
                {
                    item.text = DisplayName;
                }
            }
            
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetPositionOfRoomPlayer();
            if (isLocalPlayer)
            {
                DisplayName = GameUser.singelton.userID;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SetPositionOfRoomPlayer();
        }

        private void SetPositionOfRoomPlayer()
        {
            Canvas mainCanvas = GameObject.FindObjectOfType<Canvas>();
            transform.SetParent(mainCanvas.transform);
            //todo: change 150 to the width of the prefab:
            GetComponent<RectTransform>().anchoredPosition = new Vector2(index * 150, 0);
        }

        public override void IndexChanged(int oldIndex, int newIndex)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(index * 150, 0);
        }

        private void SetActiveComponentsInDisplay()
        {
            foreach (var btn in GetComponentsInChildren<Button>(includeInactive:true))
            {
                if (btn.gameObject.name == "btnRemove")
                {
                    //display the Remove Button:
                    //This button only shows on the Host for all players other than the Host
                    //bool shouldActive = (isServer && index > 0) || isServerOnly;
                    bool shouldActive = (index > 0) && ClientConnectMultiplayer.singelton.isHostOfGame;
                    btn.gameObject.SetActive(shouldActive);
                }
                else if (btn.gameObject.name == "btnReady")
                {
                    //the ready button should be displayed only if it's the player:
                    btn.gameObject.SetActive(NetworkClient.active && isLocalPlayer && !readyToBegin);
                }
                else if (btn.gameObject.name == "btnCancel")
                {
                    //the cancel button should be displayed only if it's the player:
                    btn.gameObject.SetActive(NetworkClient.active && isLocalPlayer && readyToBegin);
                }
            }

            foreach (var txt in GetComponentsInChildren<TMP_Text>(includeInactive: true))
            {
                if (txt.gameObject.name == "txtReady")
                {
                    txt.text = readyToBegin ? "Ready" : "Waiting...";
                }
                else if (txt.gameObject.name == "txtDisplayName")
                {
                    txt.text = DisplayName;
                }
            }
        }

        [Command(requiresAuthority =false)]
        public void CancelConnectionOFClientToServer()
        {
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        private void Update()
        {
            SetActiveComponentsInDisplay();
        }
    }
}
