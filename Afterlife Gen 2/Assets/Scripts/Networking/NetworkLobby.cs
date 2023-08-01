using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class NetworkLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] Text m_Code;
    [Header("Player Models")]
    [SerializeField] GameObject m_Pharmacist;
    [SerializeField] GameObject m_Trapper;
    [SerializeField] GameObject m_Excercist;
    [SerializeField] GameObject m_Mechanic;
    [Space]
    int Location = 0;
    [SerializeField] Transform[] m_PlayerSpawns;

    public string m_CodeValue;

    [SerializeField] GameObject m_HostButton;
    void Start()
    {
        SpawnPlayer();
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
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log(newPlayer.NickName + " Joined.");
    }

    void SpawnPlayer()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                Location = i;
            }
        }

        PhotonNetwork.Instantiate(m_Pharmacist.name, m_PlayerSpawns[Location].position, Quaternion.identity);
    }

    public void CopyCode()
    {
        TextEditor te = new TextEditor();
        te.text = m_Code.text;
        te.SelectAll();
        te.Copy();
    }

}
