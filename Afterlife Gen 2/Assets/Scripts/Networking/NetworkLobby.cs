using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/*
    eu
    au
    us
    asia,
    sa,
    jp
 
 */
enum LobbyNetworkMode
{
    Lobby = 0,
    Map,
    MainMenu,
}
public class NetworkLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] LobbyNetworkMode m_NetworkMode = LobbyNetworkMode.Lobby;
    [SerializeField] Text m_Code;
    [Header("Player Models")]
    [SerializeField] GameObject m_Exterminator;
    [SerializeField] GameObject m_Pharmacist;
    [SerializeField] GameObject m_Trapper;
    [SerializeField] GameObject m_Cultist;
    [Space]
    int Location = 0;
    [SerializeField] Transform[] m_PlayerSpawns;

    DataManager m_SaveManager;
    public string m_CodeValue;

    int m_PlayerSelected;
    [SerializeField] GameObject m_HostButton;
    
    void Start()
    {
        m_SaveManager = FindAnyObjectByType<DataManager>();
        m_PlayerSelected = PlayerPrefs.GetInt("Selected_specialist");

        SpawnPlayerByID(m_PlayerSelected);


        if (m_NetworkMode == LobbyNetworkMode.Lobby)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (m_HostButton)
                    m_HostButton.SetActive(true);
            }
            else
            {
                if (m_HostButton)
                    m_HostButton.SetActive(false);
            }

            m_Code.text = PhotonNetwork.CurrentRoom.Name;
            m_CodeValue = PhotonNetwork.CurrentRoom.Name;

            CopyCode();
        }
    }

    
    public bool SpawnPlayerByID(int _index)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
                Location = i;
        }

        switch (_index)
        {
            case 0:
                PhotonNetwork.Instantiate(m_Exterminator.name, m_PlayerSpawns[Location].position, Quaternion.LookRotation(transform.forward));
                m_SaveManager.SetPlayersSavedSpecialist(m_Exterminator.name);
                return true;
            case 1:
                PhotonNetwork.Instantiate(m_Pharmacist.name, m_PlayerSpawns[Location].position, Quaternion.LookRotation(transform.forward));
                m_SaveManager.SetPlayersSavedSpecialist(m_Pharmacist.name);
                return true;
            case 2:
                PhotonNetwork.Instantiate(m_Trapper.name, m_PlayerSpawns[Location].position, Quaternion.LookRotation(transform.forward));
                m_SaveManager.SetPlayersSavedSpecialist(m_Trapper.name);
                return true;
            case 3:
                PhotonNetwork.Instantiate(m_Cultist.name, m_PlayerSpawns[Location].position, Quaternion.LookRotation(transform.forward));
                m_SaveManager.SetPlayersSavedSpecialist(m_Cultist.name);
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
