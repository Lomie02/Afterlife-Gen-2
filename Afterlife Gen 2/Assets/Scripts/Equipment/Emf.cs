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
    Transform m_GhostEmfLocation;

    void Start()
    {
        CursedObject[] _Temp = GameObject.FindObjectsByType<CursedObject>(FindObjectsSortMode.None);
        m_ObjectiveManager = FindFirstObjectByType<ObjectiveManager>();

        m_NetworkObject = GetComponent<NetworkObject>();
        m_GhostEmfLocation = GameObject.FindGameObjectWithTag("emf_locate").transform;

        for (int i = 0; i < _Temp.Length; i++) // Find out the cursed objects position in the list.
        {
            if (_Temp[i].IsCursedObject())
            {
                m_CursedObject = _Temp[i];
                break;
            }
        }

        m_WarningIcon.gameObject.SetActive(false);

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

                            if (m_CursedObject.GetGhostProfile().m_Evidence1 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence2 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence4 == EvidenceTypes.Emf || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.Emf && m_CursedObject)
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
                    m_DistanceToTarget = Vector3.Distance(transform.position, m_GhostEmfLocation.position);

                if (m_DistanceToTarget <= 2 && m_Ghost.IsEmfActivityActive())
                {
                    if (m_ObjectiveManager.GetCurrentObjective().m_Tag == "Ghost" && m_IsEvidence)
                        m_EmfLevel = 6;
                    else
                        m_EmfLevel = m_Ghost.GetEmfAcitivtyValue();
                }
                else
                {
                    m_EmfLevel = 0;
                }

                m_WarningIcon.gameObject.SetActive(m_EmfLevel == 6);
                m_EmfText.text = m_EmfLevel.ToString();

            }
            yield return new WaitForSeconds(1f);
        }
    }
}
