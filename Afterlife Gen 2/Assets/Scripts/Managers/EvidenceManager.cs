using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceManager : MonoBehaviour
{
    [Header("General")]
    GhostAI m_GhostObject;
    GhostManager m_GhostManager;

    [Header("User Interface")]
    [SerializeField] Dropdown m_Evidence01;
    [SerializeField] Dropdown m_Evidence02;
    [SerializeField] Dropdown m_Evidence03;
    void Start()
    {
        m_GhostManager = FindAnyObjectByType<GhostManager>();
        m_GhostObject = FindAnyObjectByType<GhostAI>();
    }

    /*
        0 - No Evidence
        1 - SpiritBox
        2 - EMF
        3 - Rem
        4 - Camcorder
     
     */

    public void CompareProfiles()
    {
        switch (m_Evidence01.value)
        {
            case 0:
                break;

            case 1:

                break;
        }

    }
}
