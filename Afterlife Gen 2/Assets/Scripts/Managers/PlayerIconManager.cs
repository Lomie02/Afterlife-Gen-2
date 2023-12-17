using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerIconManager : MonoBehaviourPunCallbacks
{
    [Header("Player Icons")]
    [SerializeField] int m_PlayerIndex = 1;

    [SerializeField] Image[] m_PlayerIcons;
    [SerializeField] Sprite[] m_SpecialistImages;

    [Header("Player ScoreBoard")]
    [SerializeField] Image[] m_PlayerScoreIcons;
    [SerializeField] Text[] m_PlayerText;

    PhotonView m_MyView;
    ExitGames.Client.Photon.Hashtable m_PlayerProps = new ExitGames.Client.Photon.Hashtable();

    int m_CurrentPlayerIcon = 0;

    [SerializeField] Text m_PLayersName;

    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        if (m_MyView.IsMine)
        {
            m_PlayerProps = PhotonNetwork.LocalPlayer.CustomProperties;
            m_PlayerProps["Specialist"] = m_PlayerIndex;
            PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerProps);
            m_PLayersName.text = PhotonNetwork.LocalPlayer.NickName;
            UpdateIcons();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        UpdateIcons();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateIcons();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateIcons();
    }

    private void Update()
    {
        m_PLayersName.text = PhotonNetwork.LocalPlayer.NickName + "             Ping: " + PhotonNetwork.GetPing();
    }

    void UpdateIcons()
    {
        for (int i = 0; i < m_PlayerIcons.Length; i++)
        {
            m_PlayerIcons[i].gameObject.SetActive(false);
            m_PlayerScoreIcons[i].gameObject.SetActive(false);
        }

        m_CurrentPlayerIcon = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (!PhotonNetwork.PlayerList[i].IsLocal)
            {
                m_PlayerIcons[m_CurrentPlayerIcon].gameObject.SetActive(true);
                m_PlayerScoreIcons[m_CurrentPlayerIcon].gameObject.SetActive(true);
                m_PlayerText[m_CurrentPlayerIcon].text = PhotonNetwork.PlayerList[i].NickName;

                switch ((int)PhotonNetwork.PlayerList[i].CustomProperties["Specialist"])
                {
                    case 1:
                        m_PlayerIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[0];
                        m_PlayerScoreIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[0];
                        break;
                    case 2:
                        m_PlayerIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[1];
                        m_PlayerScoreIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[1];
                        break;
                    case 3:
                        m_PlayerIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[2];
                        m_PlayerScoreIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[2];
                        break;
                    case 4:
                        m_PlayerIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[3];
                        m_PlayerScoreIcons[m_CurrentPlayerIcon].sprite = m_SpecialistImages[3];
                        break;
                }

                m_CurrentPlayerIcon++;
            }
        }
    }

    public void UpdateScoreBoardIcons()
    {

    }
}
