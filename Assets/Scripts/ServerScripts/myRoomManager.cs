using Mirror;
using PlayFab.MultiplayerAgent.Model;
using SpaceBattle.Server.playfab;
using UnityEngine;

namespace SpaceBattle.Server
{
    public class myRoomManager : NetworkRoomManager
    {
        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            ConnectedPlayer cp = new ConnectedPlayer(conn.connectionId.ToString());
            ServerPlayfabEngine.singelton.addPlayer(cp);
            Debug.Log("Player connected to server");
        }

        public override void OnRoomServerConnect(NetworkConnection conn)
        {
            base.OnRoomServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            ConnectedPlayer player = ServerPlayfabEngine.singelton.listOfConnections.Find(p => p.PlayerId.Equals(conn.connectionId.ToString()));
            ServerPlayfabEngine.singelton.removePlayer(player);
            Debug.Log("Player disconnected from server");
            //ServerInit.singelton.listOfConnections.Remove(player);
        }
    }
}
