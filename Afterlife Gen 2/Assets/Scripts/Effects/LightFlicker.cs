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
            if (m_IsEnabled)
            {
                int rand = Random.Range(0, 8);

                if (rand == 2 || rand == 4)
                {
                    m_LightObject.intensity = m_LowIntensity;
                }
                else
                {
                    m_LightObject.intensity = m_DefaultIntensity;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SetFlicker(bool _state)
    {
        m_IsEnabled = _state;
    }
}
