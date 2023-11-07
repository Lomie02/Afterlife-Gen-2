using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

enum LobbyNetworkMode
{
    Lobby = 0,
    Map,
}
public class NetworkLobby : MonoBehaviourPunCallbacks
{

    [SerializeField] LobbyNetworkMode m_NetworkMode = LobbyNetworkMode.Lobby;
    [SerializeField] Text m_Code;
    [Header("Player Models")]
    [SerializeField] GameObject m_Pharmacist;
    [SerializeField] GameObject m_Trapper;
    [SerializeField] GameObject m_Excercist;
    [SerializeField] GameObject m_Mechanic;
    [Space]
    int Location = 0;
    [SerializeField] Transform[] m_PlayerSpawns;

    DataManager m_SaveManager;
    public string m_CodeValue;

    [SerializeField] GameObject m_HostButton;
    void Start()
    {
        m_SaveManager = FindAnyObjectByType<DataManager>();
        SpawnPlayer();

        if (m_NetworkMode == LobbyNetworkMode.Lobby)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (m_HostButton)
                {
                    m_HostButton.SetActive(true);
                }
            }
            else
            {
                if (m_HostButton)
                {
                    m_HostButton.SetActive(false);
                }
            }
            m_Code.text = PhotonNetwork.CurrentRoom.Name;
            m_CodeValue = PhotonNetwork.CurrentRoom.Name;

            CopyCode();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log(newPlayer.NickName + " Joined.");
    }

    public void SpawnPlayer()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                Location = i;
            }
        }


        if (m_NetworkMode == LobbyNetworkMode.Lobby)
        {
            PhotonNetwork.Instantiate(m_Pharmacist.name, m_PlayerSpawns[Location].position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(m_SaveManager.GetPlayersSavedSpecialist(), m_PlayerSpawns[Location].position, Quaternion.identity);
        }

    }

    public bool SpawnPlayerByID(int _index)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                Location = i;
            }
        }

        if (_index == 1)
        {
            PhotonNetwork.Instantiate(m_Pharmacist.name, m_PlayerSpawns[Location].position, Quaternion.identity);
            m_SaveManager.SetPlayersSavedSpecialist(m_Pharmacist.name);
            return true;
        }
        else if (_index == 2)
        {
            PhotonNetwork.Instantiate(m_Trapper.name, m_PlayerSpawns[Location].position, Quaternion.identity);
            m_SaveManager.SetPlayersSavedSpecialist(m_Trapper.name);
            return true;
        }
        else if (_index == 3)
        {
            PhotonNetwork.Instantiate(m_Excercist.name, m_PlayerSpawns[Location].position, Quaternion.identity);
            m_SaveManager.SetPlayersSavedSpecialist(m_Excercist.name);
            return true;
        }
        else if (_index == 4)
        {
            PhotonNetwork.Instantiate(m_Mechanic.name, m_PlayerSpawns[Location].position, Quaternion.identity);
            m_SaveManager.SetPlayersSavedSpecialist(m_Mechanic.name);
            return true;
        }

        return false;
    }

    public void CopyCode()
    {
        TextEditor te = new TextEditor();
        te.text = PhotonNetwork.CurrentRoom.Name;
        te.SelectAll();
        te.Copy();
    }

}
