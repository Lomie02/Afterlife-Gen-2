using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;

public class TextChatManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Text m_DisplayChat;
    [SerializeField] InputField m_ChatInputBox;

    [SerializeField] PhotonView m_MyView;
    bool m_IsTypingInChat = false;

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        m_DisplayChat.text += "\n" + newPlayer.NickName + " Has Joined.";
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        m_DisplayChat.text += "\n" + otherPlayer.NickName + " Has Disconnected.";
    }

    public void SendTextChatMessage()
    {
        string _ConvertedString = SteamFriends.GetPersonaName() + ": " + m_ChatInputBox.text;

        m_MyView.RPC("RPC_SendOverChats", RpcTarget.All, _ConvertedString);
        m_ChatInputBox.text = "";
    }

    [PunRPC]
    public void RPC_SendOverChats(string _input)
    {
        m_DisplayChat.text += "\n" + _input;
    }

    public bool IsTextChatShowing()
    {
        return m_IsTypingInChat;
    }
    public void SetChatDisplay(bool _state)
    {
        m_ChatInputBox.gameObject.SetActive(_state);
        m_IsTypingInChat = _state;
    }
}
