using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBuilder : MonoBehaviour
{
    Text m_TextObject;
    string m_OriginalTextContent;

    bool m_Displaytext = false;
    int m_CurrentLetter = 0;

    void Start()
    {
        if (!m_TextObject)
            m_TextObject = GetComponent<Text>();

        m_OriginalTextContent = m_TextObject.text;
        m_TextObject.text = "";

        m_CurrentLetter = 0;
        m_Displaytext = true;

        StartCoroutine(UpdateTextLetters());
    }

    // Update is called once per frame
    IEnumerator UpdateTextLetters()
    {
        while (m_Displaytext)
        {
            if (m_CurrentLetter == m_OriginalTextContent.Length - 1)
                m_Displaytext = false; this.enabled = false; yield return null;

            m_TextObject.text += m_OriginalTextContent[m_CurrentLetter];
            m_CurrentLetter++;

            yield return new WaitForSeconds(0.1f);
        }
    }
}
