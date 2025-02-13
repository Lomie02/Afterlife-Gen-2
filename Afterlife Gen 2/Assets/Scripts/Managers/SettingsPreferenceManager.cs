using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
struct UiElement
{
    [Header("General")]
    public string m_Name;
    public UiElementType m_ElementType;

    [Header("Type")]
    public Slider m_Slider;
    public Dropdown m_Dropmenu;

    [Header("Extras")]
    public string m_DataFileName;
    public Text m_LiveValue;
}

enum UiElementType
{
    Slider = 0,
    Dropdown = 1,
    Toggle = 2,
}

public class SettingsPreferenceManager : MonoBehaviour
{
    [SerializeField] GameObject[] m_Pages;

    [SerializeField] UiElement[] m_InterfaceElement;
    public UnityEvent m_OnSettingsApplied;

    void Awake()
    {
        for (int i = 0; i < m_Pages.Length; i++)
        {
            m_Pages[i].SetActive(false);
        }

        SetUpPrefEvents(true);
    }
    void SetUpPrefEvents(bool _IncludeElementSetUp = false)
    {
        if (!_IncludeElementSetUp)
        {
            for (int i = 0; i < m_InterfaceElement.Length; i++)
            {
                switch (m_InterfaceElement[i].m_ElementType)
                {
                    case UiElementType.Slider:
                        m_InterfaceElement[i].m_Slider.onValueChanged.AddListener(LiveValueCatcher);
                        break;

                    case UiElementType.Dropdown:
                        break;
                }
            }
        }
        else
        {
            for (int i = 0; i < m_InterfaceElement.Length; i++)
            {
                switch (m_InterfaceElement[i].m_ElementType)
                {
                    case UiElementType.Slider:

                        m_InterfaceElement[i].m_Slider.onValueChanged.AddListener(LiveValueCatcher);

                        if (PlayerPrefs.HasKey(m_InterfaceElement[i].m_DataFileName))
                            m_InterfaceElement[i].m_Slider.value = PlayerPrefs.GetFloat(m_InterfaceElement[i].m_DataFileName);

                        m_InterfaceElement[i].m_LiveValue.text = Mathf.Ceil(m_InterfaceElement[i].m_Slider.value).ToString();
                        break;

                    case UiElementType.Dropdown:

                        m_InterfaceElement[i].m_Dropmenu.onValueChanged.AddListener(LiveValueCatcher);

                        if (PlayerPrefs.HasKey(m_InterfaceElement[i].m_DataFileName))
                            m_InterfaceElement[i].m_Dropmenu.value = PlayerPrefs.GetInt(m_InterfaceElement[i].m_DataFileName);
                        break;
                }
            }
        }
    }

    // Updates the live values from text without complex headaches
    void LiveValueCatcher(float _dummyIndex = 0f)
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Slider)
                m_InterfaceElement[i].m_LiveValue.text = Mathf.Ceil(m_InterfaceElement[i].m_Slider.value).ToString();
        }
    }

    void LiveValueCatcher(int _dummyIndex = 0) // int version of the 
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            // TODO: Figure out how this should work with dropdowns.
        }
    }

    // Get mouse sens
    public float FetchMouseSens()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Slider && m_InterfaceElement[i].m_Name == "MouseSens")
            {
                return m_InterfaceElement[i].m_Slider.value;
            }
        }
        return 0f;
    }

    public float FetchFieldofView()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Slider && m_InterfaceElement[i].m_Name == "Fov")
            {
                return m_InterfaceElement[i].m_Slider.value;
            }
        }
        return 0f;
    }

    public void ApplyAllDataSettings()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            switch (m_InterfaceElement[i].m_ElementType)
            {
                case UiElementType.Slider:
                    PlayerPrefs.SetFloat(m_InterfaceElement[i].m_DataFileName, m_InterfaceElement[i].m_Slider.value);
                    break;

                case UiElementType.Dropdown:
                    PlayerPrefs.SetInt(m_InterfaceElement[i].m_DataFileName, m_InterfaceElement[i].m_Dropmenu.value);
                    break;
            }
        }

        m_OnSettingsApplied.Invoke();
    }

}
