using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ReadyZone : MonoBehaviour
{
    PhotonView m_View;
    [SerializeField] bool m_HostIsReady = false;
    [SerializeField] PositonLerp m_ReadyUpDoor;

    [Header("Host Stuff")]
    [SerializeField] GameObject m_MonitorFlare;

    [Space]
    [SerializeField] Text m_ReadyText;
    public int m_PlayersInReadyZone = 0;

    [SerializeField] GameManager m_GameManager;

    [SerializeField] GameObject m_LoadingScreen;
    bool m_StopAcceptingPlayers = false;
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            m_MonitorFlare.SetActive(false);
        }

        m_View = GetComponent<PhotonView>();
        m_GameManager = FindObjectOfType<GameManager>();
    }

    public void SubmitLoadingScreen(GameObject _object)
    {
        m_LoadingScreen = _object;
    }

    public void ReadyUpHost()
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        m_PlayersInReadyZone++;
        CheckPlayerZoneCount();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        m_PlayersInReadyZone--;
    }

    [PunRPC]
    public void RPC_ReadyUP()
    {
        m_StopAcceptingPlayers = true;
    }

    private void CheckPlayerZoneCount()
    {
        if (m_PlayersInReadyZone >= PhotonNetwork.PlayerList.Length && !m_StopAcceptingPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                m_View.RPC("RPC_ReadyUP", RpcTarget.All);
            }

            m_View.RPC("RPC_DisplayLoadingScreen", RpcTarget.All);

            m_GameManager.ChangeNetworkScene("mansion_mp");
        }
    }

    [PunRPC]
    public void RPC_DisplayLoadingScreen()
    {
        m_LoadingScreen.SetActive(true);
    }
}
