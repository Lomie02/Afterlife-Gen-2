using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Networking General")]
    [SerializeField] GameObject m_NetworkScreenObject;

    [Space]
    [SerializeField] UnityEvent m_OnConnected;

    private void Start()
    {
        if (m_NetworkScreenObject && !m_NetworkScreenObject.activeSelf)
        {
            m_NetworkScreenObject.SetActive(true);
        }
    }
    public void ConnectToServer() // Connect to the server
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()// Has connected to the server
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // Join Main menu 
    {
        m_NetworkScreenObject.SetActive(false);
        m_OnConnected.Invoke();
    }
}
