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
    ObjectiveManager m_ObjectiveManager;
    int m_EmfLevel = 0;

    GhostAI m_Ghost;

    float m_DistanceToTarget;

    void Start()
    {
        CursedObject[] _Temp = GameObject.FindObjectsByType<CursedObject>(FindObjectsSortMode.None);
        m_ObjectiveManager = FindFirstObjectByType<ObjectiveManager>();

        m_NetworkObject = GetComponent<NetworkObject>();

        for (int i = 0; i < _Temp.Length; i++) // Find out the cursed objects position in the list.
        {
            if (_Temp[i].IsCursedObject())
            {
                m_CursedObject = _Temp[i];
                break;
            }
        }

        m_Ghost = FindFirstObjectByType<GhostAI>(); 

        if (m_Ghost.GetGhostProfile().m_Evidence1 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence2 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf && m_CursedObject)
        {
            m_IsEvidence = true;
        }

        m_EmfText.text = m_EmfLevel.ToString();

        StartCoroutine(UpdateEmf());
    }


    public IEnumerator UpdateEmf()
    {
        while (true)
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

                            if (m_CursedObject.GetGhostProfile().m_Evidence1 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence2 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf && m_CursedObject)
                            {
                                m_IsEvidence = true;
                            }

                            break;
                        }
                    }
                }

                if (m_ObjectiveManager.GetCurrentObjective().m_Tag == "Cursed_Object")
                    m_DistanceToTarget = Vector3.Distance(transform.position, m_CursedObject.gameObject.transform.position);
                else
                    m_DistanceToTarget = Vector3.Distance(transform.position, m_Ghost.gameObject.transform.position);

                if (m_DistanceToTarget <= 2 && m_IsEvidence && m_CursedObject.IsCursedObject())
                {
                    m_EmfLevel = 6;
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(true);
                }
                else if (m_DistanceToTarget > 2 && m_DistanceToTarget <= 4)
                {
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(false);
                    m_EmfLevel = 5;
                }
                else if (m_DistanceToTarget > 4 && m_DistanceToTarget <= 6)
                {
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(false);
                    m_EmfLevel = 4;
                }
                else if (m_DistanceToTarget > 6 && m_DistanceToTarget <= 8)
                {
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(false);
                    m_EmfLevel = 3;
                }
                else if (m_DistanceToTarget > 8 && m_DistanceToTarget <= 10)
                {
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(false);
                    m_EmfLevel = 2;
                }
                else if (m_DistanceToTarget > 10)
                {
                    if (m_WarningIcon)
                        m_WarningIcon.gameObject.SetActive(false);
                    m_EmfLevel = 1;
                }
                m_EmfText.text = m_EmfLevel.ToString();
            }

            yield return new WaitForSeconds(5f);
        }
    }
}
