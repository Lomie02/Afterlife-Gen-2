using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
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

    Volume m_PostProcessingWorld;
    [SerializeField] UiElement[] m_InterfaceElement;
    public UnityEvent m_OnSettingsApplied;
    float m_Fps;

    [Header("System Details")]
    [SerializeField] Text m_Processor;
    [SerializeField] Text m_GraphicsCard;
    [SerializeField] Text m_VirtualRam;

    int m_TotalVirtualRam;
    long m_VirtualRamUsedInMemory;

    void Awake()
    {
        m_Pages[0].SetActive(true);

        for (int i = 1; i < m_Pages.Length; i++)
        {
            m_Pages[i].SetActive(false);
        }

        if (PlayerPrefs.HasKey("settings_fps"))
        {
            StartCoroutine(DisplayFps());
        }

        SetUpPrefEvents(true);

        m_PostProcessingWorld = GameObject.FindGameObjectWithTag("WorldPost").GetComponent<Volume>();

        UpdateLocalData();
        m_OnSettingsApplied.AddListener(UpdateLocalData);

    }

    void UpdateLocalData()
    {
        QualitySettings.SetQualityLevel(FetchGraphicSettings());

        if (QualitySettings.GetQualityLevel() == 3)
        {
            m_PostProcessingWorld.enabled = false;
        }
        else
        {
            m_PostProcessingWorld.enabled = true;
        }


        var m_Pipeline = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
        QualitySettings.vSyncCount = FetchVirtualSync();

        HDAdditionalLightData[] m_LightsInScene = FindObjectsByType<HDAdditionalLightData>(FindObjectsSortMode.None);

        foreach (HDAdditionalLightData LightData in m_LightsInScene)
        {
            switch (FetchShadowQuality())
            {
                case 0: // Ultra
                    LightData.EnableShadows(true);
                    LightData.SetShadowResolution(4069);
                    break;

                case 1: // High
                    LightData.EnableShadows(true);
                    LightData.SetShadowResolution(2048);
                    break;

                case 2: // Medium
                    LightData.EnableShadows(true);
                    LightData.SetShadowResolution(1024);
                    break;

                case 3: // Low

                    LightData.EnableShadows(true);
                    LightData.SetShadowResolution(520);
                    break;

                case 4:
                    LightData.EnableShadows(false);
                    break;
            }
        }

        switch (FetchWindowRes())
        {
            case 0: // 960 x 540
                Screen.SetResolution(960, 540, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 1:
                Screen.SetResolution(1280, 720, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 2:
                Screen.SetResolution(1920, 1080, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 3:
                Screen.SetResolution(2048, 1080, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 4:
                Screen.SetResolution(2560, 1440, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 5:
                Screen.SetResolution(3840, 2160, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
            case 6:
                Screen.SetResolution(4096, 2160, (FullScreenMode)FetchWindowMode(), Screen.mainWindowDisplayInfo.refreshRate);
                break;
        }


    }

    IEnumerator DisplayFps()
    {
        while (true)
        {
            m_Fps = 1.0f / Time.deltaTime;
            Debug.Log("FPS: " + Mathf.Ceil(m_Fps));
            yield return new WaitForSeconds(0.5f);
        }
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
                        m_InterfaceElement[i].m_Dropmenu.onValueChanged.AddListener(LiveValueCatcher);
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

        UpdateSystemDetails();
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

        UpdateSystemDetails();
        m_OnSettingsApplied.Invoke();
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

    public int FetchGraphicSettings()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "Graphics")
            {
                return m_InterfaceElement[i].m_Dropmenu.value;
            }
        }
        return 0;
    }

    public int FetchWindowRes()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "ScreenRes")
            {
                return m_InterfaceElement[i].m_Dropmenu.value;
            }
        }
        return 0;
    }

    public float FetchLoaderDistance()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Slider && m_InterfaceElement[i].m_Name == "LoaderDistance")
            {
                return m_InterfaceElement[i].m_Slider.value;
            }
        }
        return 0;
    }

    public RealtimeLightMode FetchLoaderMode()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "LoaderMode")
            {
                switch (m_InterfaceElement[i].m_Dropmenu.value)
                {
                    case 0:
                        return RealtimeLightMode.UltraOptimized;
                    case 1:
                        return RealtimeLightMode.HighOptimized;
                    case 2:
                        return RealtimeLightMode.MediumOptimized;
                    case 3:
                        return RealtimeLightMode.Off;

                }
            }
        }
        return 0;
    }

    public int FetchWindowMode()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "ScreenMode")
            {
                return m_InterfaceElement[i].m_Dropmenu.value;
            }
        }
        return 0;
    }

    public int FetchVirtualSync()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "VirtualSync")
            {
                return m_InterfaceElement[i].m_Dropmenu.value;
            }
        }
        return 0;
    }

    public int FetchShadowQuality()
    {
        for (int i = 0; i < m_InterfaceElement.Length; i++)
        {
            if (m_InterfaceElement[i].m_ElementType == UiElementType.Dropdown && m_InterfaceElement[i].m_Name == "ShadowQuality")
            {
                return m_InterfaceElement[i].m_Dropmenu.value;
            }
        }
        return 0;
    }

    public void UpdateSystemDetails()
    {
        m_Processor.text = SystemInfo.processorType.ToString();
        m_GraphicsCard.text = SystemInfo.graphicsDeviceName.ToString();

        m_TotalVirtualRam = SystemInfo.graphicsMemorySize;
        m_VirtualRamUsedInMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        m_VirtualRam.text = "VRAM: " + m_VirtualRamUsedInMemory + " MB / " + m_TotalVirtualRam + " MB";
    }

}
