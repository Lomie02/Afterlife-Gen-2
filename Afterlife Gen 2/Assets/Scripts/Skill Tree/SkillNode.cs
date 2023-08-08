using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour
{
    [Header("General")]
    [SerializeField] string m_NodeName;
    [SerializeField] string m_Description;
    [Space]
    [SerializeField] float m_SkillCost = 100f;

    [Header("Nodes")]
    [SerializeField] SkillNode m_PreviousNode;
    [SerializeField] SkillNode[] m_NextNodes;

    Button m_MyButton;
    public bool m_HasBaughtMe = false;
    public bool m_IsAllowedToBuy = false;

    [SerializeField] float m_Money;

    private void Start()
    {
        m_MyButton = GetComponent<Button>();

        PlayerPrefs.DeleteAll();

        if (PlayerPrefs.HasKey(m_NodeName + "buy"))
        {
            int convert = PlayerPrefs.GetInt(m_NodeName + "buy");

            if (convert == 0)
            {
                m_HasBaughtMe = false;
            }
            else
            {
                m_HasBaughtMe = true;

                for (int i = 0; i < m_NextNodes.Length; i++)
                {
                    m_NextNodes[i].m_IsAllowedToBuy = true;
                    m_NextNodes[i].UpdateNode();
                }
            }
        }
        m_MyButton.onClick.AddListener(BuyNode);

        if (m_PreviousNode != null)
        {
            m_IsAllowedToBuy = true;
        }
        UpdateNode();
    }

    public void UpdateNode()
    {
        if (m_IsAllowedToBuy)
        {
            m_MyButton.interactable = true;
        }
        else
        {
            m_MyButton.interactable = false;
        }
    }

    public string GetNodeName()
    {
        return m_NodeName;
    }

    public string GetNodeDescription()
    {
        return m_Description;
    }

    public bool GetNodePurchaseState()
    {
        return m_HasBaughtMe;
    }

    public void BuyNode()
    {
        if (m_Money >= m_SkillCost && !m_HasBaughtMe)
        {
            m_HasBaughtMe = true;
            PlayerPrefs.SetInt(m_NodeName + "buy", 1);
            m_Money -= m_SkillCost;
        }
    }
}
