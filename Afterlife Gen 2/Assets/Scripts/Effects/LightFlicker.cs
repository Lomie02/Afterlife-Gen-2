using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] bool m_IsEnabled = true;

    Light m_LightObject;
    float m_DefaultIntensity = 0;
    float m_LowIntensity;

    void Start()
    {
        m_LightObject = GetComponent<Light>();
        m_DefaultIntensity = m_LightObject.intensity;
        m_LowIntensity = m_DefaultIntensity / 2;

        StartCoroutine(UpdateFlickerEffect());
    }

    IEnumerator UpdateFlickerEffect()
    {
        while (true)
        {
            m_LightObject.intensity = m_DefaultIntensity + UnityEngine.Random.Range(-2, 2f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetFlicker(bool _state)
    {
        m_IsEnabled = _state;
    }
}
