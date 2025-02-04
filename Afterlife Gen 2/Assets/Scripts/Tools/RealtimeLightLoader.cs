using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class RealtimeLightLoader : MonoBehaviour
{
    Light[] m_RealtimeLights;
    MeshRenderer[] m_MeshRenderersInScene;
    float m_MaxDistanceFromCamera = 10;

    float m_FramesCheckLimit = 5;
    float m_FramesPassed;

    public int m_RealtimeShadowsLimit = 5;
    public int m_CurrentRealtimeShadowsActive = 0;

    [System.Obsolete]
    void Start()
    {
        // Collect all Realtime Light data

        if (!GetComponent<PhotonView>().IsMine) this.enabled = false;

        GrabLights();

        StartCoroutine(UpdateShadows());
    }

    [System.Obsolete]
    void GrabLights()
    {
        m_RealtimeLights = FindObjectsOfType<Light>();
        m_MeshRenderersInScene = FindObjectsOfType<MeshRenderer>();

        foreach (Light light in m_RealtimeLights)
        {
            if (light.type != LightType.Directional)
            {
                light.shadows = LightShadows.None;
                light.GetComponent<HDAdditionalLightData>().affectsVolumetric = false;
                light.GetComponent<HDAdditionalLightData>().volumetricFadeDistance = 10f;
            }
        }
    }

    private IEnumerator UpdateShadows()
    {
        while (true)
        {
            foreach (Light light in m_RealtimeLights)
            {
                if (!light || light.type == LightType.Directional) continue;

                float distanceFromCamera = Vector3.Distance(transform.position, light.transform.position);
                if (distanceFromCamera <= m_MaxDistanceFromCamera)
                {
                    if (light.shadows == LightShadows.None && m_CurrentRealtimeShadowsActive < m_RealtimeShadowsLimit)
                    {
                        light.shadows = LightShadows.Hard;
                        m_CurrentRealtimeShadowsActive++;
                    }


                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = true;

                    float smoothFactor = Mathf.Clamp01((m_MaxDistanceFromCamera - distanceFromCamera) / m_MaxDistanceFromCamera);
                    light.shadowStrength = Mathf.Lerp(0, 1, smoothFactor);

                    light.GetComponent<HDAdditionalLightData>().volumetricDimmer = Mathf.Lerp(0, 1, smoothFactor);
                }
                else
                {

                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = false;

                    if (light.shadows == LightShadows.Hard && light.type != LightType.Directional)
                    {
                        light.shadows = LightShadows.None;
                        m_CurrentRealtimeShadowsActive--;
                    }
                }
            }

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

            yield return new WaitForSeconds(0.5f);

        }

    }
}
