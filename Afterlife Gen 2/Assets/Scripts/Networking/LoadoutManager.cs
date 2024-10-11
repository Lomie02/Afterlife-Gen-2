using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct LoadoutSpecialistProfile //TODO: Decide if this system would be better off being implemented
{
    public string m_SpecialistName;
    public string m_Description;

    public GameObject m_PlayerModel;
}

public class LoadoutManager : MonoBehaviour
{
    [SerializeField] GameObject[] m_SpecialistInfo;
    [SerializeField] GameObject[] m_EquipedIcon;
    [SerializeField] GameObject[] m_SpecialistModels;

    int m_CurrentSelected = 0;

    [SerializeField] Text m_SpecialistLevel;

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

        int ConvertedLevel;
        switch (m_CurrentSelected)
        {
            case 0: // Exterminators Level
                ConvertedLevel = (int)PlayerPrefs.GetFloat("specialist_level_exterminator");
                m_SpecialistLevel.text = "Specialist Level: " + ConvertedLevel.ToString();
                break;

            case 1: // Get Pharmacist Level
                ConvertedLevel = (int)PlayerPrefs.GetFloat("specialist_level_pharm");
                m_SpecialistLevel.text = "Specialist Level: " + ConvertedLevel.ToString();

                break;

            case 2: // Get Trapper Level
                ConvertedLevel = (int)PlayerPrefs.GetFloat("specialist_level_trapper");
                m_SpecialistLevel.text = "Specialist Level: " + ConvertedLevel.ToString();
                break;

            case 3: // Get Cultist Level
                ConvertedLevel = (int)PlayerPrefs.GetFloat("specialist_level_cultist");
                m_SpecialistLevel.text = "Specialist Level: " + ConvertedLevel.ToString();
                break;
        }

        m_EquipedIcon[m_CurrentSelected].SetActive(true);
        m_SpecialistModels[m_CurrentSelected].SetActive(true);
        m_SpecialistInfo[m_CurrentSelected].SetActive(true);
    }

}
