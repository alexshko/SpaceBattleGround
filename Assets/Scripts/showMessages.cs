using UnityEngine;
using Mirror;

public class showMessages : NetworkBehaviour
{
    private GameObject winMessage = null;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        winMessage = findWinningMessage();
    }

    //find the panel inside the canvas that's shown when the game is won by the player:
    private GameObject findWinningMessage()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (!canvas)
        {
            Debug.LogFormat("could not find the canvas");
            return null;
        }

        //find the message:
        foreach (var item in canvas.GetComponentsInChildren<TMPro.TMP_Text>(includeInactive:true))
        {
            if (item.gameObject.name == "WinningMessage")
            {
                return item.gameObject;
            }
        }
        Debug.Log("Didn't dind the winning mesaage");
        return null;
    }

    [TargetRpc]
    public void TargetShowWinMessage()
    {
        if (winMessage)
        {
            winMessage.SetActive(true);
        }
        else
        {
            Debug.Log("Didn't find the win message");
        }
    }
}
