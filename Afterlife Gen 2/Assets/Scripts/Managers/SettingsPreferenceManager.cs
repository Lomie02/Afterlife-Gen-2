using UnityEngine;
using UnityEngine.UI;


public class SettingsPreferenceManager : MonoBehaviour
{
    [SerializeField] GameObject[] m_Pages;
    void Start()
    {
        for (int i = 0; i < m_Pages.Length; i++)
        {
            m_Pages[i].SetActive(false);
        }

        GetSettingsPrefs();
    }

    void GetSettingsPrefs()
    {

    }

}
