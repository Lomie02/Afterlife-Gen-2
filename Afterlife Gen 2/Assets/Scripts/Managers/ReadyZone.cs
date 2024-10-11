using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

enum ReadyZoneMode
{
    Lobby = 0,
    InGame,
}

public class ReadyZone : MonoBehaviour
{
    [SerializeField] ReadyZoneMode m_ReadyZoneMode = ReadyZoneMode.Lobby;

    [SerializeField] bool m_HostIsReady = false;
    [SerializeField] PositonLerp m_ReadyUpDoor;

    [Header("Host Stuff")]
    [SerializeField] GameObject m_MonitorFlare;

    [Space]
    [SerializeField] Text m_ReadyText;
    int m_PlayersInReadyZone = 0;

    [SerializeField] GameManager m_GameManager;
    [SerializeField] GameObject m_LoadingScreen;

    PhotonView m_View;
    bool m_StopAcceptingPlayers = false;

    private void Start()
    {
        if (m_ReadyZoneMode == ReadyZoneMode.Lobby)
        {
            if (!PhotonNetwork.IsMasterClient)
                m_MonitorFlare.SetActive(false);
        }

        m_View = GetComponent<PhotonView>();
        m_GameManager = FindAnyObjectByType<GameManager>();
    }

    public void SubmitLoadingScreen(GameObject _object) // Clients will need to submit their loading screens to the Ready Zone to avoid order of init problems
    {
        m_LoadingScreen = _object;
    }

    public void ReadyUpHost() // When the host is ready to start the match.
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        m_View.RPC("RPC_GameReadyToStart", RpcTarget.All);
        m_HostIsReady = true;
    }

    [PunRPC]
    public void RPC_GameReadyToStart()
    {
        m_ReadyText.text = "LETS GO!";
        m_ReadyUpDoor.LerpPositions(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        m_PlayersInReadyZone++;
        CheckPlayerZoneCount();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        m_PlayersInReadyZone--;
    }

    [PunRPC]
    public void RPC_ReadyUP()// 
    {
        m_StopAcceptingPlayers = true;
    }

    private void CheckPlayerZoneCount()
    {

        switch (m_ReadyZoneMode)
        {
            case ReadyZoneMode.Lobby:
                if (m_PlayersInReadyZone >= PhotonNetwork.PlayerList.Length && !m_StopAcceptingPlayers)
                {
                    m_View.RPC("RPC_ReadyUP", RpcTarget.All);

                    m_View.RPC("RPC_DisplayLoadingScreen", RpcTarget.All);
                    m_GameManager.ChangeNetworkScene("mansion_mp");
                }
                break;

            case ReadyZoneMode.InGame:

                if (m_PlayersInReadyZone >= PhotonNetwork.PlayerList.Length)
                {
                    m_GameManager.ChangeNetworkScene("Afterlife_Corp");
                }
                break;
        }
        
    }

    [PunRPC]
    public void RPC_DisplayLoadingScreen()
    {
        m_LoadingScreen.SetActive(true);
    }
}
