using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialistIconSwapper : MonoBehaviour
{
    [SerializeField] Image m_MainSprite;

    [SerializeField] Sprite[] m_Icons;
    void Start()
    {
        m_MainSprite = GetComponent<Image>();
    }

    public void ChangeIcon(int _index)
    {
        if (_index < m_Icons.Length)
        {
            m_MainSprite.sprite = m_Icons[_index];
        }
    }
}
