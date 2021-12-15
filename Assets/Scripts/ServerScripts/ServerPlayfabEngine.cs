using UnityEngine;
using Mirror;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using PlayFab.MultiplayerAgent.Model;

namespace SpaceBattle.Server.playfab
{
    public class ServerPlayfabEngine : MonoBehaviour
    {
        public static ServerPlayfabEngine singelton;

        [HideInInspector] public List<ConnectedPlayer> listOfConnections;
        public float secondsForCheckingAmountOfUsers = 2 * 60;
        Coroutine waitForPlayers;

        private void Awake()
        {
            singelton = this;
        }
        private void Start()
        {
            listOfConnections = new List<ConnectedPlayer>();
            if (Application.isBatchMode || PlayfabSettings.singleton.isServerInstance)
            {
                if (PlayfabSettings.singleton.isRemote)
                {
                    PlayFabMultiplayerAgentAPI.Start();
                    waitForPlayers = StartCoroutine(ReadyForPlayers());
                    PlayFabMultiplayerAgentAPI.OnServerActiveCallback += ServerGotActiveEvent;
                    PlayFabMultiplayerAgentAPI.OnShutDownCallback += serverShutDown;

                    //check every 2 minutes, if no players then kill host:
                    StartCoroutine(checkIFHavePlayers());
                }
                else
                {
                    ServerGotActiveEvent();
                }
            }
        }

        public static void serverShutDown()
        {
            Debug.Log("Shut down");
            NetworkManager.singleton.StopHost();
            Application.Quit();
        }

        private IEnumerator ReadyForPlayers()
        {
            yield return new WaitForSeconds(0.5f);
            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
        }

        private void ServerGotActiveEvent()
        {
            Debug.Log("server is active");
            NetworkManager.singleton.StartServer();
        }

        private IEnumerator checkIFHavePlayers()
        {
            while (true)
            {
                yield return new WaitForSeconds(secondsForCheckingAmountOfUsers);
                Debug.Log("Number of players: " + listOfConnections.Count);
                if (listOfConnections.Count <= 0)
                {
                    //no players, shut down.
                    serverShutDown();
                }
            }
        }

        public void addPlayer(ConnectedPlayer conn)
        {
            listOfConnections.Add(conn);
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(listOfConnections);
        }
        public void removePlayer(ConnectedPlayer conn)
        {
            listOfConnections.Remove(conn);
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(listOfConnections);
            //incase after the player left, no more players in the game. need to shut down:
            if (listOfConnections.Count <= 0)
            {
                //no players, shut down.
                serverShutDown();
            }
        }
    }
}
