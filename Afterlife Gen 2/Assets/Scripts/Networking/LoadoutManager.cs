using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    [SerializeField] GameObject[] m_SpecialistInfo;
    [SerializeField] GameObject[] m_EquipedIcon;
    [SerializeField] GameObject[] m_SpecialistModels;

    int m_CurrentSelected = 0;
    void Start()
    {
        if (PlayerPrefs.HasKey("Selected_specialist"))
        {
            m_CurrentSelected = PlayerPrefs.GetInt("Selected_specialist");
        }

        UpdateLoadout();
    }

    public void SelectSpecialist(int _index)
    {
        m_CurrentSelected = _index;
        PlayerPrefs.SetInt("Selected_specialist", m_CurrentSelected);
        UpdateLoadout();
    }

    public void UpdateLoadout()
    {
        for (int i = 0; i < m_SpecialistModels.Length; i++)
        {
            m_SpecialistModels[i].SetActive(false);
            m_EquipedIcon[i].SetActive(false);
            m_SpecialistInfo[i].SetActive(false);
        }

        m_EquipedIcon[m_CurrentSelected].SetActive(true);
        m_SpecialistModels[m_CurrentSelected].SetActive(true);
        m_SpecialistInfo[m_CurrentSelected].SetActive(true);
    }

}
