using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Steamworks;
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

    [SerializeField] Dropdown m_RegionList;
    public string m_CurrentRegion;
    [SerializeField] Text m_LoadingTextStatus;

    string[] m_RegionSupported = new string[] { "au", "eu", "us", "asia", "jp", "sa" };

    ExitGames.Client.Photon.Hashtable m_PlayerProps = new ExitGames.Client.Photon.Hashtable();
    int m_Level = 0;
    int m_IsDeveloper = 0;

    // Create co-op Game Stats
    [SerializeField] Toggle m_IsPrivateMatch;
    [SerializeField] InputField m_LobbyName;
    [SerializeField] InputField m_LobbyPassword;
    private void Start()
    {
        if (m_NetworkScreenObject && !m_NetworkScreenObject.activeSelf)
        {
            m_NetworkScreenObject.SetActive(true);
        }
        m_GameManager = FindFirstObjectByType<GameManager>();
        m_ErrorScreen.SetActive(false);

        m_BecomeDev.onClick.AddListener(BecomeDeveloper);
        m_BecomeDev.onClick.AddListener(delegate { m_DevScreen.SetActive(false); });

        m_LoadingTextStatus.text = "Connecting To Regions...";

        if (PlayerPrefs.HasKey("players_pref_region"))
            SetRegion(PlayerPrefs.GetString("players_pref_region"));
        else
            TestClientsPing();

        m_RegionList.onValueChanged.AddListener(OnRegionListUpdated);


        m_LoadingTextStatus.text = "Connecting To Steam.";
        if (SteamManager.Initialized)
        {
            m_Username.text = SteamFriends.GetPersonaName();
        }
        else // if failed to connect to steam then the entire game wont load. Needs to Restart
        {
            m_ErrorScreen.SetActive(true);
            m_FailType.text = "Failed To Connect To Steam.";
            m_FailMessage.text = "Afterlife has failed to connect to the steam network. Please restart & try again.";
        }
    }

    public void OnRegionListUpdated(int _index)
    {
        SetRegion(m_RegionSupported[_index]);
    }

    void TestClientsPing()
    {
        if (!PhotonNetwork.ConnectToBestCloudServer())
        {
            m_CurrentRegion = PhotonNetwork.BestRegionSummaryInPreferences;
            SetRegion(m_CurrentRegion);
        }

        m_CurrentRegion = PhotonNetwork.CloudRegion;
    }

    public void SetRegion(string _desiredRegion)
    {
        PhotonNetwork.ConnectToRegion(_desiredRegion);
        m_CurrentRegion = PhotonNetwork.CloudRegion;
        PlayerPrefs.SetString("players_pref_region", _desiredRegion);
    }

    public void ConnectToServer() // Connect to the server
    {
        m_LoadingTextStatus.text = "Connecting To Afterlife Game Servers.";

        PhotonNetwork.ConnectUsingSettings();

        PhotonNetwork.LocalPlayer.NickName = SteamFriends.GetPersonaName();

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
    }

    void SetUpPlayerProfile()
    {
        if (m_PlayerProps.ContainsKey("PlayerLevel"))
            m_PlayerProps["PlayerLevel"] = m_Level;
        else
            m_PlayerProps.TryAdd("PlayerLevel", m_Level);

        if (m_PlayerProps.ContainsKey("Specialist"))
            m_PlayerProps["Specialist"] = PlayerPrefs.GetInt("Selected_specialist");
        else
            m_PlayerProps.TryAdd("Specialist", PlayerPrefs.GetInt("Selected_specialist"));

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
        m_ErrorScreen.SetActive(true);
    }

    public override void OnJoinedLobby() // Join Main menu 
    {
        m_NetworkScreenObject.SetActive(false);

        if (!SteamUserStats.GetStat("player_level_overall", out m_Level))
        {
            m_Level = 1;
            SteamUserStats.SetStat("player_level_overall", m_Level);
        }
        
        m_Username.text = SteamFriends.GetPersonaName() + " Lvl: " + m_Level.ToString();
        m_OnConnected.Invoke();
    }

    public void CreateSoloGame() // Create a solo match
    {
        m_NetworkScreenObject.SetActive(true);
        int code = Random.Range(100000, 999999); // picks a random number to act as the solo pass

        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { IsVisible = false, MaxPlayers = 1 };
        PhotonNetwork.CreateRoom("Solo-" + code.ToString(), roomOptions);
    }

    public void CreateCoopMatch() // Create a multiplayer match
    {
        m_NetworkScreenObject.SetActive(true);

        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { IsVisible = !m_IsPrivateMatch.isOn, MaxPlayers = 4 };

        if (m_IsPrivateMatch.isOn) // if game is private
            PhotonNetwork.CreateRoom(m_LobbyPassword.text, roomOptions);
        else
            PhotonNetwork.CreateRoom(m_LobbyName.text, roomOptions);
    }

    public void QuickPlayMatchMaking()
    {
        PhotonNetwork.JoinRandomRoom();
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
        PhotonNetwork.JoinRoom("au-" + m_CodeInput.text);
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
