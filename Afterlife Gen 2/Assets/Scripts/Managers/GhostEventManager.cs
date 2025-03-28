using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;
using Unity.VisualScripting;

public class GhostEventManager : MonoBehaviourPunCallbacks
{
    [SerializeField] UniversalEvent[] m_GhostEvents;
    float m_GhostEventRefreshRate = 20;
    float m_GhostEventRefreshTimer;
    private void Start()
    {
        m_GhostEvents = GetComponentsInChildren<UniversalEvent>();

        for (int i = 0; i < m_GhostEvents.Length; i++)
        {
            m_GhostEvents[i].gameObject.SetActive(false);
        }

        m_GhostEventRefreshRate = Random.Range(10, 25);
        m_GhostEventRefreshTimer = m_GhostEventRefreshRate;

        if (PhotonNetwork.IsMasterClient)
            SetUpRandomEvents();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

    }

    public void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        m_GhostEventRefreshTimer -= Time.deltaTime;

        if (m_GhostEventRefreshTimer <= 0)
        {
            RefreshEvents();
            m_GhostEventRefreshRate = Random.Range(10, 25);
            m_GhostEventRefreshTimer = m_GhostEventRefreshRate;
        }
    }

    public void ClearEvents()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        for (int i = 0; i < m_GhostEvents.Length; i++)
        {
            m_GhostEvents[i].ClearEvent();
        }
    }

    public void SetUpRandomEvents()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < m_GhostEvents.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                m_GhostEvents[i].RestoreEvent();
            }
        }
    }

    public void RefreshEvents()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < m_GhostEvents.Length; i++)
        {
            m_GhostEvents[i].ClearEvent();

            if (Random.Range(0, 2) == 1)
            {
                m_GhostEvents[i].RestoreEvent();
            }
        }
    }

}
