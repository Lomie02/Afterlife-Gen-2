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

    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_Ghost = FindObjectOfType<GhostAI>();

        m_NetworkObject = GetComponent<NetworkObject>();

        if (m_Ghost.GetGhostProfile().m_Evidence1 == EvidenceTypes.RemPod || m_Ghost.GetGhostProfile().m_Evidence2 == EvidenceTypes.RemPod || m_Ghost.GetGhostProfile().m_Evidence3 == EvidenceTypes.RemPod)
        {
            m_IsEvidence = true;
        }

        StartCoroutine(UpdateRemPods());

    }

    IEnumerator UpdateRemPods()
    {
        while (true)
        {

            if (m_NetworkObject.GetPowerState())
            {
                if (!m_Ghost)
                {
                    m_Ghost = FindObjectOfType<GhostAI>();
                }

                float DistanceToGhost = Vector3.Distance(transform.position, m_Ghost.gameObject.transform.position);

                if (DistanceToGhost < 5 && m_IsEvidence)
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

            yield return new WaitForSeconds(5f);
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
