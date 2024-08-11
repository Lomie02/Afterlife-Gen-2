using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] Text m_SkillPointsText;
    int m_SkillPoints = 3;

    private void Start()
    {
        if (PlayerPrefs.HasKey("skillpoints"))
        {
            m_SkillPoints = PlayerPrefs.GetInt("skillpoints");
        }

        m_SkillPointsText.text = "Skill Points: " + m_SkillPoints.ToString();
    }
    public void SubtractPoints(int _cost)
    {
        m_SkillPoints -= _cost;
        if (m_SkillPointsText)
        {
            m_SkillPointsText.text = "Skill Points: " + m_SkillPoints.ToString();
        }
        PlayerPrefs.SetInt("skillpoints", m_SkillPoints);
    }

    public int GetSkillPoints()
    {
        return m_SkillPoints;
    }
}
