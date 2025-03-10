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

public class RealtimeLightLoader : MonoBehaviour
{
    RealtimeLightMode m_OptimizationMode;

    Light[] m_RealtimeLights;
    MeshRenderer[] m_MeshRenderersInScene;
    float m_MaxDistanceFromCamera = 10;

    float m_FramesCheckLimit = 5;
    float m_FramesPassed;

    public int m_RealtimeShadowsLimit = 5;
    public int m_CurrentRealtimeShadowsActive = 0;

    public SettingsPreferenceManager m_SettingsPreferenceManager;

    void Start()
    {
        // Collect all Realtime Light data

        m_SettingsPreferenceManager = GetComponentInChildren<SettingsPreferenceManager>(true);

        UpdateData();

        m_SettingsPreferenceManager.m_OnSettingsApplied.AddListener(UpdateData);
        GrabLights();

        StartCoroutine(UpdateShadows());
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
        m_MeshRenderersInScene = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        UpdateLightsData();
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

        if (m_MeshRenderersInScene.Length != 0)
        {
            foreach (MeshRenderer mesh in m_MeshRenderersInScene)
                mesh.shadowCastingMode = ShadowCastingMode.On;
        }

        // Update lights in scene Data
        foreach (Light light in m_RealtimeLights)
        {
            if (light.type != LightType.Directional && light.shadows != LightShadows.None)
            {
                if (m_OptimizationMode == RealtimeLightMode.UltraOptimized)
                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = false;
                light.GetComponent<HDAdditionalLightData>().fadeDistance = ViewDistance;
                light.GetComponent<HDAdditionalLightData>().volumetricFadeDistance = ViewDistance;
                light.GetComponent<HDAdditionalLightData>().shadowFadeDistance = ViewDistance;

                if (m_OptimizationMode == RealtimeLightMode.UltraOptimized || m_OptimizationMode == RealtimeLightMode.HighOptimized)
                    light.GetComponent<HDAdditionalLightData>().shadowUpdateMode = ShadowUpdateMode.OnDemand;
            }
        }
    }

    private IEnumerator UpdateShadows()
    {
        while (true)
        {
            foreach (Light light in m_RealtimeLights)
            {
                if (!light || light.type == LightType.Directional || light.shadows == LightShadows.None) continue;

                float distanceFromCamera = Vector3.Distance(transform.position, light.transform.position);
                if (distanceFromCamera <= m_MaxDistanceFromCamera && !Physics.Linecast(transform.position, light.transform.position) && light.GetComponent<Renderer>().isVisible)
                {
                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = true;

                    float smoothFactor = Mathf.Clamp01((m_MaxDistanceFromCamera - distanceFromCamera) / m_MaxDistanceFromCamera);
                    light.shadowStrength = Mathf.Lerp(0, 1, smoothFactor);

                    light.GetComponent<HDAdditionalLightData>().volumetricDimmer = Mathf.Lerp(0, 1, smoothFactor);

                    switch (m_OptimizationMode)
                    {
                        case RealtimeLightMode.UltraOptimized:
                            light.GetComponent<HDAdditionalLightData>().RequestShadowMapRendering();
                            break;
                        case RealtimeLightMode.HighOptimized:
                            light.GetComponent<HDAdditionalLightData>().shadowUpdateMode = ShadowUpdateMode.EveryFrame;
                            break;
                    }

                }
                else
                {

                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = false;

                    if (light.type != LightType.Directional && m_OptimizationMode == RealtimeLightMode.UltraOptimized)
                    {
                        if (m_OptimizationMode != RealtimeLightMode.UltraOptimized && light.shadows != LightShadows.None)
                            light.GetComponent<HDAdditionalLightData>().shadowUpdateMode = ShadowUpdateMode.OnDemand;
                    }
                }
            }

            if (m_OptimizationMode == RealtimeLightMode.UltraOptimized)
            {
                foreach (MeshRenderer mesh in m_MeshRenderersInScene)
                {
                    float distanceFromCamera = Vector3.Distance(transform.position, mesh.transform.position);

                    if (distanceFromCamera <= m_MaxDistanceFromCamera)
                    {
                        mesh.shadowCastingMode = ShadowCastingMode.On;
                    }
                    else
                    {
                        mesh.shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

        }

    }
}
