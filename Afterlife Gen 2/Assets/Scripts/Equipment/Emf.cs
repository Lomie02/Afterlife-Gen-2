using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Emf : MonoBehaviour
{
    [SerializeField] Text m_EmfText;
    [SerializeField] Image m_WarningIcon;
    [SerializeField] GhostAI m_Ghost;
    bool m_IsEvidence = false;
    NetworkObject m_NetworkObject;

    int m_EmfLevel = 0;

    void Start()
    {
        m_Ghost = FindObjectOfType<GhostAI>();
        m_NetworkObject = GetComponent<NetworkObject>();
        if (m_Ghost.GetGhostProfile().m_Evidence1 == EvidenceTypes.Emf || m_Ghost.GetGhostProfile().m_Evidence2 == EvidenceTypes.Emf || m_Ghost.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf)
        {
            m_IsEvidence = true;
        }

        m_EmfText.text = m_EmfLevel.ToString();
    }

    void Update()
    {
        if (m_NetworkObject.GetPowerState())
        {
            if (!m_Ghost)
            {
                m_Ghost = FindObjectOfType<GhostAI>();
            }

            float DistanceToGhost = Vector3.Distance(transform.position, m_Ghost.gameObject.transform.position);

            if (DistanceToGhost <= 2 && m_IsEvidence)
            {
                m_EmfLevel = 6;
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(true);
            }
            else if (DistanceToGhost > 2 && DistanceToGhost <= 4)
            {
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(false);
                m_EmfLevel = 5;
            }
            else if (DistanceToGhost > 4 && DistanceToGhost <= 6)
            {
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(false);
                m_EmfLevel = 4;
            }
            else if (DistanceToGhost > 6 && DistanceToGhost <= 8)
            {
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(false);
                m_EmfLevel = 3;
            }
            else if (DistanceToGhost > 8 && DistanceToGhost <= 10)
            {
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(false);
                m_EmfLevel = 2;
            }
            else if (DistanceToGhost > 10)
            {
                if (m_WarningIcon)
                    m_WarningIcon.gameObject.SetActive(false);
                m_EmfLevel = 1;
            }
            m_EmfText.text = m_EmfLevel.ToString();
        }
    }
}
