using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public enum RealtimeLightMode
{
    UltraOptimized = 0,
    HighOptimized,
    MediumOptimized,
    Off,
}

[System.Serializable]
public struct LightDataPack
{
    public Light m_LightObject;
    public HDAdditionalLightData m_AdditionalLightData;
}

public class RealtimeLightLoader : MonoBehaviour
{
    RealtimeLightMode m_OptimizationMode = RealtimeLightMode.HighOptimized;
    public LayerMask m_RealTimeCasterLayers;
    Light[] m_RealtimeLights;
    HDAdditionalLightData[] m_AdditionalLights;

    public PhotonView m_PhotonView;

    public LightDataPack[] m_Lights;
    float m_MaxDistanceFromCamera = 10;

    float m_FramesCheckLimit = 5;
    float m_FramesPassed;

    public int m_RealtimeShadowsLimit = 5;
    public int m_CurrentRealtimeShadowsActive = 0;

    Camera m_PlayersCamera;
    public SettingsPreferenceManager m_SettingsPreferenceManager;
    LodLightGroup[] m_LightGroups;

    void Start()
    {
        // Collect all Realtime Light data

        m_PhotonView = GetComponent<PhotonView>();
        m_PlayersCamera = GetComponentInChildren<Camera>();

        if (!m_PhotonView.IsMine) this.enabled = false;

        m_SettingsPreferenceManager = GetComponentInChildren<SettingsPreferenceManager>(true);

        GrabLights();

        if (m_SettingsPreferenceManager)
        {
            UpdateData();
            m_SettingsPreferenceManager.m_OnSettingsApplied.AddListener(UpdateData);
        }

        StartCoroutine(UpdateLightShadowData());
    }


    void UpdateData()
    {
        m_OptimizationMode = m_SettingsPreferenceManager.FetchLoaderMode();
        m_MaxDistanceFromCamera = m_SettingsPreferenceManager.FetchLoaderDistance();

        UpdateLightsData();
    }

    void GrabLights()
    {
        m_RealtimeLights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        m_Lights = new LightDataPack[m_RealtimeLights.Length];

        for (int i = 0; i < m_RealtimeLights.Length; i++)
        {
            m_Lights[i].m_LightObject = m_RealtimeLights[i];
            m_Lights[i].m_AdditionalLightData = m_RealtimeLights[i].GetComponent<HDAdditionalLightData>();
        }

        SetUpLevelOfDetailLightGroups();


        UpdateLightsData();
    }

    void SetUpLevelOfDetailLightGroups()
    {
        for (int i = 0; i < m_RealtimeLights.Length; i++)
        {
            LodLightGroup LightGroupParent = m_RealtimeLights[i].transform.transform.parent.AddComponent<LodLightGroup>();
            LightGroupParent.AssignLight(0, m_RealtimeLights[i].gameObject);

            for (int j = 0; j < 2; j++)
            {
                GameObject Temp = GameObject.Instantiate(m_RealtimeLights[i].gameObject, m_RealtimeLights[i].transform.position, m_RealtimeLights[i].transform.rotation);

                Temp.transform.parent = m_RealtimeLights[i].transform.parent;
                Temp.name = m_RealtimeLights[i].name + " LOD Light " + j.ToString();

                LightGroupParent.AssignLight(j + 1, Temp.gameObject);
            }

            LightGroupParent.AssignMainCamera(m_PlayersCamera);
            LightGroupParent.CalculateThreshold();

        }

        m_LightGroups = FindObjectsByType<LodLightGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void UpdateLightsData()
    {
        float ViewDistance = 0;

        switch (m_OptimizationMode)
        {
            case RealtimeLightMode.UltraOptimized:
                ViewDistance = 5f;
                break;

            case RealtimeLightMode.HighOptimized:
                ViewDistance = 10f;
                break;

            case RealtimeLightMode.MediumOptimized:
                ViewDistance = 15f;
                break;

            case RealtimeLightMode.Off:
                ViewDistance = 50;
                break;

        }


        // Update lights in scene Data
        for (int i = 0; i < m_Lights.Length; i++)
        {
            if (m_Lights[i].m_LightObject.type != LightType.Directional)
            {
                if (m_OptimizationMode == RealtimeLightMode.UltraOptimized)
                    m_Lights[i].m_AdditionalLightData.affectsVolumetric = false;

                m_Lights[i].m_LightObject.shadows = LightShadows.Soft;
                m_Lights[i].m_AdditionalLightData.fadeDistance = ViewDistance;
                m_Lights[i].m_AdditionalLightData.volumetricFadeDistance = ViewDistance;
                m_Lights[i].m_AdditionalLightData.shadowFadeDistance = ViewDistance;
            }
        }
    }

    private IEnumerator UpdateLightShadowData()
    {
        while (true)
        {

            if (m_OptimizationMode == RealtimeLightMode.Off) yield return null;

            for (int i = 0; i < m_Lights.Length; i++)
            {
                if (m_Lights[i].m_LightObject != null && m_Lights[i].m_LightObject.type != LightType.Directional)
                {
                    float distanceFromCamera = Vector3.Distance(transform.position, m_Lights[i].m_LightObject.transform.position);

                    if (distanceFromCamera <= m_MaxDistanceFromCamera)
                    {
                        if (m_OptimizationMode == RealtimeLightMode.UltraOptimized) // Is opitmise mode on
                            m_Lights[i].m_LightObject.enabled = true;

                        m_Lights[i].m_LightObject.shadows = LightShadows.Soft;
                        m_Lights[i].m_LightObject.shadowResolution = LightShadowResolution.Medium;
                    }
                    else
                    {
                        if (m_OptimizationMode == RealtimeLightMode.UltraOptimized) // Is opitmise mode on
                            m_Lights[i].m_LightObject.enabled = false;


                        m_Lights[i].m_LightObject.shadows = LightShadows.None;
                        m_Lights[i].m_LightObject.shadowResolution = LightShadowResolution.Low;
                    }
                }
            }

            for (int i = 0; i < m_LightGroups.Length; i++)
            {
                m_LightGroups[i].ProcessLevels();
            }

            // Light culling

            for (int i = 0; i < m_Lights.Length; i++)
            {
                m_Lights[i].m_LightObject.gameObject.SetActive(IsLightCulled(m_Lights[i]));
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private bool IsLightInView(Light _light)
    {
        Vector3 lightPos = _light.transform.position;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_PlayersCamera);

        return GeometryUtility.TestPlanesAABB(planes, new Bounds(lightPos, Vector2.zero));
    }

    private bool IsLightCulled(LightDataPack _Light)
    {
        RaycastHit[] RaycastHits;
        RaycastHits = Physics.RaycastAll(transform.position, _Light.m_LightObject.transform.position - transform.position, m_MaxDistanceFromCamera);

        foreach (RaycastHit Temp in RaycastHits)
        {
            if (!Temp.collider.gameObject.isStatic)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
