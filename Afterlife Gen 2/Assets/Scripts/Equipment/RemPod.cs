using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RemPod : MonoBehaviour
{
    [SerializeField] PhotonView m_MyView;
    [SerializeField] Light[] m_RemPodLights;
    GhostAI m_Ghost;

    NetworkObject m_NetworkObject;
    bool m_IsEvidence = false;

    ObjectiveManager m_ObjectiveManager;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_ObjectiveManager = FindAnyObjectByType<ObjectiveManager>();

        AttemptToFindGhost();
        m_NetworkObject = GetComponent<NetworkObject>();

    }

    void AttemptToFindGhost()
    {
        m_Ghost = FindAnyObjectByType<GhostAI>();

        if (m_Ghost != null)
        {

            if (m_Ghost.GetGhostProfile().m_Evidence1 == EvidenceTypes.RemPod || m_Ghost.GetGhostProfile().m_Evidence2 == EvidenceTypes.RemPod || m_Ghost.GetGhostProfile().m_Evidence3 == EvidenceTypes.RemPod || m_Ghost.GetGhostProfile().m_Evidence4 == EvidenceTypes.RemPod)
            {
                m_IsEvidence = true;
            }
        }
    }

    void Update()
    {
        if (m_NetworkObject.GetPowerState() && m_Ghost != null)
        {
            if (!m_Ghost)
            {
                AttemptToFindGhost();
            }

            float DistanceToGhost = Vector3.Distance(transform.position, m_Ghost.gameObject.transform.position);

            if (DistanceToGhost < 5 && m_IsEvidence && m_ObjectiveManager.GetCurrentObjective().m_Tag == "Ghost")
            {
                ShowLights(5);
            }
            else if (DistanceToGhost > 10 && DistanceToGhost < 15)
            {
                ShowLights(4);
            }
            else if (DistanceToGhost > 15 && DistanceToGhost < 20)
            {
                ShowLights(3);
            }
            else if (DistanceToGhost > 20 && DistanceToGhost < 25)
            {
                ShowLights(2);
            }
            else if (DistanceToGhost > 25 && DistanceToGhost < 30)
            {
                ShowLights(1);
            }
        }
    }

    void HideAllLights()
    {
        for (int i = 0; i < m_RemPodLights.Length; i++)
        {
            m_RemPodLights[i].gameObject.SetActive(false);
        }
    }

    void ShowLights(int _lightsToShow)
    {
        HideAllLights();

        for (int i = 0; i < _lightsToShow; i++)
        {
            m_RemPodLights[i].gameObject.SetActive(true);
        }
    }
}
