using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkWorldCameraHud : MonoBehaviourPunCallbacks
{
    Canvas m_Canvas;
    
    [SerializeField] Text[] m_PlayerNames;

    private void Start()
    {
        m_Canvas = GetComponent<Canvas>();
        UpdateNames();
    }

    public void AssignCamera(Camera _cam)
    {
        m_Canvas.worldCamera = _cam;
    }

    private void UpdateNames()
    {
        for (int i = 0; i < 4; i++)
        {
            m_PlayerNames[i].text = "";
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int Level = (int)PhotonNetwork.PlayerList[i].CustomProperties["PlayerLevel"];
            int dev = (int)PhotonNetwork.PlayerList[i].CustomProperties["Developer"];

            if (dev == 1)
            {
                m_PlayerNames[i].color = Color.red;
            }

            m_PlayerNames[i].text = PhotonNetwork.PlayerList[i].NickName + " lvl: " + Level.ToString();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateNames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateNames();
    }
}
