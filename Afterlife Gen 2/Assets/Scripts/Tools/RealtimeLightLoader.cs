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

    void Start()
    {
        // Collect all Realtime Light data

        if (!GetComponent<PhotonView>().IsMine) this.enabled = false;

        GrabLights();

        StartCoroutine(UpdateShadows());
    }

    void GrabLights()
    {
        m_RealtimeLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
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

                    float smoothFactor = Mathf.Clamp01((m_MaxDistanceFromCamera - distanceFromCamera) / m_MaxDistanceFromCamera);
                    light.shadowStrength = Mathf.Lerp(0, 1, smoothFactor);
                }

                light.shadows = LightShadows.None;
                yield return new WaitForSeconds(0.5f);
            }

        }

    }
}
