using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Networking General")]
    [SerializeField] GameObject m_NetworkScreenObject;

    [Space]
    [SerializeField] UnityEvent m_OnConnected;
    [SerializeField] GameManager m_GameManager;

    [SerializeField] InputField m_CodeInput;
    private void Start()
    {
        if (m_NetworkScreenObject && !m_NetworkScreenObject.activeSelf)
        {
            m_NetworkScreenObject.SetActive(true);
        }
        m_GameManager = FindObjectOfType<GameManager>();

    }
    public void ConnectToServer() // Connect to the server
    {
        PhotonNetwork.ConnectUsingSettings();
        int num = Random.Range(50, 999);
        PhotonNetwork.LocalPlayer.NickName = "Hunter" + num.ToString();
    }

    public override void OnConnectedToMaster()// Has connected to the server
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ConnectToServer();
    }

    public override void OnJoinedLobby() // Join Main menu 
    {
        m_NetworkScreenObject.SetActive(false);
        m_OnConnected.Invoke();
    }

    public void CreateLobby()
    {
        int code = Random.Range(10000, 99999);
        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { IsVisible = true, MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(PhotonNetwork.CloudRegion + "-" + code.ToString(), roomOptions);
    }

    public override void OnCreatedRoom()
    {
        m_GameManager.ChangeScene("Afterlife_Corp");
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinRoom(m_CodeInput.text);

    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
}
