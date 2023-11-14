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

    [Header("Developer")]
    [SerializeField] GameObject m_DevScreen;
    [SerializeField] InputField m_DevCode;
    [SerializeField] Button m_BecomeDev;

    ExitGames.Client.Photon.Hashtable m_PlayerProps = new ExitGames.Client.Photon.Hashtable();
    int m_Level = 0;
    int m_IsDeveloper = 0;
    private void Start()
    {
        if (m_NetworkScreenObject && !m_NetworkScreenObject.activeSelf)
        {
            m_NetworkScreenObject.SetActive(true);
        }
        m_GameManager = FindObjectOfType<GameManager>();
        m_ErrorScreen.SetActive(false);

        m_BecomeDev.onClick.AddListener(BecomeDeveloper);
        m_BecomeDev.onClick.AddListener(delegate { m_DevScreen.SetActive(false); });
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.D))
        {
            m_DevScreen.SetActive(true);
        }
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

        if (PlayerPrefs.HasKey("players_level"))
        {
            m_Level = PlayerPrefs.GetInt("player_levels");
        }
        else
        {
            m_Level = 1;
            PlayerPrefs.SetInt("player_levels", m_Level);
        }

        if (PlayerPrefs.HasKey("developerState"))
        {
            m_IsDeveloper = PlayerPrefs.GetInt("developerState");

            if (m_IsDeveloper == 1)
            {
                m_Username.color = Color.green;
            }
        }
        else
        {
            PlayerPrefs.SetInt("developerState", m_IsDeveloper);
        }

        SetUpPlayerProfile();
        m_Username.text = PhotonNetwork.LocalPlayer.NickName + " lvl: " + m_Level;
    }

    void SetUpPlayerProfile()
    {
        if (m_PlayerProps.ContainsKey("PlayerLevel"))
            m_PlayerProps["PlayerLevel"] = m_Level;
        else
            m_PlayerProps.TryAdd("PlayerLevel", m_Level);


        if (m_PlayerProps.ContainsKey("Developer"))
            m_PlayerProps["Developer"] = m_IsDeveloper;
        else
            m_PlayerProps.TryAdd("Developer", m_IsDeveloper);


        PhotonNetwork.LocalPlayer.CustomProperties = m_PlayerProps;
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

    public void BecomeDeveloper()
    {
        if (m_DevCode.text == "442190" && m_IsDeveloper == 0)
        {
            m_Username.color = Color.green;
            m_IsDeveloper = 1;
            PlayerPrefs.SetInt("developerState", m_IsDeveloper);
            SetUpPlayerProfile();
        }
    }
}
