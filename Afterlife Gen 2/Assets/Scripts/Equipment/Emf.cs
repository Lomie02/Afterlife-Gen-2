using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Emf : MonoBehaviour
{
    [SerializeField] Text m_EmfText;
    [SerializeField] Image m_WarningIcon;
    [SerializeField] CursedObject m_CursedObject;
    bool m_IsEvidence = false;
    NetworkObject m_NetworkObject;

    int m_EmfLevel = 0;

    [System.Obsolete]
    void Start()
    {
        CursedObject[] _Temp = GameObject.FindObjectsByType<CursedObject>(FindObjectsSortMode.None);


        m_NetworkObject = GetComponent<NetworkObject>();

        for (int i = 0; i < _Temp.Length; i++) // Find out the cursed objects position in the list.
        {
            if (_Temp[i].IsCursedObject())
            {
                m_CursedObject = _Temp[i];
                break;
            }
        }

        if (m_CursedObject.GetGhostProfile().m_Evidence1 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence2 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf)
        {
            m_IsEvidence = true;
        }

        m_EmfText.text = m_EmfLevel.ToString();
    }

    [System.Obsolete]

    void Update()
    {
        if (m_NetworkObject.GetPowerState())
        {
            if (!m_CursedObject)
            {
                CursedObject[] _Temp = GameObject.FindObjectsByType<CursedObject>(FindObjectsSortMode.None);

                for (int i = 0; i < _Temp.Length; i++) // Find out the cursed objects position in the list.
                {
                    if (_Temp[i].IsCursedObject())
                    {
                        m_CursedObject = _Temp[i];
                        break;
                    }
                }
            }

            float DistanceToGhost = Vector3.Distance(transform.position, m_CursedObject.gameObject.transform.position);

            if (DistanceToGhost <= 2 && m_IsEvidence && m_CursedObject.IsCursedObject())
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
