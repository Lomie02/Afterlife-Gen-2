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

    float m_MaxDistanceFromCamera = 5f;

    float m_FramesCheckLimit = 5;
    float m_FramesPassed;

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
                    light.shadows = LightShadows.Soft;
                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = true;

                    float smoothFactor = Mathf.Clamp01((m_MaxDistanceFromCamera - distanceFromCamera) / m_MaxDistanceFromCamera);
                    light.shadowStrength = Mathf.Lerp(0, 1, smoothFactor);

                    light.GetComponent<HDAdditionalLightData>().volumetricDimmer = Mathf.Lerp(0, 1, smoothFactor);
                }
                else
                {
                    light.GetComponent<HDAdditionalLightData>().affectsVolumetric = false;
                    light.shadows = LightShadows.None;
                }

                yield return new WaitForSeconds(0.5f);
            }

        }

    }
}
