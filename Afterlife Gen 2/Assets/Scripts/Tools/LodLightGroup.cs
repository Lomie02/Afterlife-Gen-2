using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LodLightGroup : MonoBehaviour
{
    public GameObject[] m_LightLevels;
    LightDataPack[] m_LightingData;
    Camera m_UsersCamera;

    float m_DistanceThreshold = 15;
    float[] m_LOD_Thresholds;
    float m_LightFactor;

    public LodLightGroup()
    {
        m_LightLevels = new GameObject[3];
        m_LightingData = new LightDataPack[3];
        m_LOD_Thresholds = new float[3];
    }

    public void AssignLight(int _index, GameObject _LightObject)
    {
        m_LightLevels[_index] = _LightObject;

        m_LightingData[_index].m_LightObject = _LightObject.GetComponent<Light>();
        m_LightingData[_index].m_AdditionalLightData = _LightObject.GetComponent<HDAdditionalLightData>();
        m_LightingData[_index].m_LightObject.gameObject.SetActive(false);
    }

    void UpdateLighting(int _Level)
    {
        for (int i = 0; i < m_LightLevels.Length; i++)
            m_LightLevels[i].SetActive(false);

        if (_Level == 3) return;

        m_LightLevels[_Level].SetActive(true);
    }

    public void AssignMainCamera(Camera _camera)
    {
        m_UsersCamera = _camera;
    }

    public void CalculateThreshold()
    {
        m_LightFactor = m_DistanceThreshold / 3;

        float DistanceThres = m_DistanceThreshold;

        for (int i = 0; i < 3; i++)
        {
            m_LOD_Thresholds[i] = DistanceThres;
            DistanceThres -= m_LightFactor;
        }

        m_LOD_Thresholds[2] += 3;

        CreateLightingData();
    }

    void CreateLightingData()
    {
        for (int i = 1; i < m_LightLevels.Length; i++)
        {
            m_LightingData[i].m_LightObject.intensity = m_LightingData[i - 1].m_LightObject.intensity / 2; // Light Intensity
            m_LightingData[i].m_LightObject.range = m_LightingData[i - 1].m_LightObject.range / 2; // Light Range

            m_LightingData[i].m_AdditionalLightData.volumetricDimmer = m_LightingData[i - 1].m_AdditionalLightData.volumetricDimmer / 2; // Volumetruc Dimmer
            m_LightingData[i].m_AdditionalLightData.volumetricFadeDistance = m_LightingData[i - 1].m_AdditionalLightData.volumetricFadeDistance / 2; // Volumetric Fade Distance
            m_LightingData[i].m_AdditionalLightData.shadowFadeDistance = m_LightingData[i - 1].m_AdditionalLightData.shadowFadeDistance / 2; // Shadow Fade Distance

            m_LightingData[i].m_AdditionalLightData.fadeDistance = m_LightingData[i - 1].m_AdditionalLightData.fadeDistance / 2; // Shadow Fade Distance

            m_LightingData[i].m_AdditionalLightData.affectsVolumetric = false;
            m_LightingData[i].m_AdditionalLightData.EnableShadows(false);
            m_LightingData[i].m_LightObject.shadowStrength = 0;
        }

        ProcessLevels();
    }

    bool IsPreviousShadowLowest(LightDataPack _previousLight)
    {
        return _previousLight.m_LightObject.shadowResolution == UnityEngine.Rendering.LightShadowResolution.Low ? true : false;
    }

    public void ProcessLevels()
    {
        if (!m_UsersCamera) { Debug.LogError(gameObject.name + " Lod Light Group Missing Camera! Please assign a camera."); return; }

        float distance = Vector3.Distance(transform.position, m_UsersCamera.transform.position);

        if (distance < m_LOD_Thresholds[0] && distance >= m_LOD_Thresholds[1]) // Level 2
        {
            UpdateLighting(2);
        }
        else if (distance < m_LOD_Thresholds[1] && distance >= m_LOD_Thresholds[2]) // Level 1
        {
            UpdateLighting(1);
        }
        else if (distance < m_LOD_Thresholds[2])// Level 0
            UpdateLighting(0);
    }
}
