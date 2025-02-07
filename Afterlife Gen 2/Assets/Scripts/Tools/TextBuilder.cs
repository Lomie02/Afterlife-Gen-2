using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBuilder : MonoBehaviour
{
    Text m_TextObject;
    string m_OriginalTextContent;

    bool m_Displaytext = false;
    float m_TimerForDisplayDuration = 0.05f;
    int m_CurrentLetter = 0;

    void Start()
    {
        if (!m_TextObject)
            m_TextObject = GetComponent<Text>();

        m_OriginalTextContent = m_TextObject.text;
        m_TextObject.text = "";

        m_CurrentLetter = 0;
        m_Displaytext = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_Displaytext) return;

        m_TimerForDisplayDuration -= Time.deltaTime;
        if (m_TimerForDisplayDuration <= 0)
        {
            if (m_CurrentLetter >= m_OriginalTextContent.Length)
                m_Displaytext = false;

            m_TimerForDisplayDuration = 0.05f;

            m_TextObject.text += m_OriginalTextContent[m_CurrentLetter];
            m_CurrentLetter++;
        }

    }
}
