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
    [SerializeField] InputField m_UsernameInput;

    [Space]
    [SerializeField] Text m_Username;
    [SerializeField] Text m_FailMessage;
    [SerializeField] Text m_FailType;
    [SerializeField] GameObject m_ErrorScreen;

    private void Start()
    {
        if (m_NetworkScreenObject && !m_NetworkScreenObject.activeSelf)
        {
            m_NetworkScreenObject.SetActive(true);
        }
        m_GameManager = FindObjectOfType<GameManager>();
        m_ErrorScreen.SetActive(false);
    }
    public void ConnectToServer() // Connect to the server
    {
        PhotonNetwork.ConnectUsingSettings();

        if (PlayerPrefs.HasKey("username"))
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("username");
        }
        else
        {
            int num = Random.Range(50, 999);
            PhotonNetwork.LocalPlayer.NickName = "Employee_" + num.ToString();
        }

        m_Username.text = PhotonNetwork.LocalPlayer.NickName;
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
        m_NetworkScreenObject.SetActive(true);
        int code = Random.Range(10000, 99999);
        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { IsVisible = true, MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(PhotonNetwork.CloudRegion + "-" + code.ToString(), roomOptions);
    }

    public override void OnCreatedRoom()
    {
        m_GameManager.ChangeScene("Afterlife_Corp");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        m_NetworkScreenObject.SetActive(false);

        m_ErrorScreen.SetActive(true);
        m_FailType.text = "Failed To Create Lobby.";
        m_FailMessage.text = "An issue occured trying to create a lobby. Error Code: " + returnCode.ToString();
    }

    public void JoinLobby()
    {
        m_NetworkScreenObject.SetActive(true);
        PhotonNetwork.JoinRoom(m_CodeInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        m_NetworkScreenObject.SetActive(false);
        m_ErrorScreen.SetActive(true);

        m_FailType.text = "Failed To Join Lobby";
        m_FailMessage.text = "The lobby you are trying to join my not exist or the code you have is incorrect. The lobby may also be full. Error Code: " + returnCode.ToString();
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    public void ChangeUsername()
    {
        PhotonNetwork.LocalPlayer.NickName = m_UsernameInput.text;
        PlayerPrefs.SetString("username", m_UsernameInput.text);
        m_Username.text = PhotonNetwork.LocalPlayer.NickName;
    }
}
