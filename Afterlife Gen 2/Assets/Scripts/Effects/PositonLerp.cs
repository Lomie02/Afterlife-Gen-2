using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositonLerp : MonoBehaviour
{
    [SerializeField] Vector3[] m_LerpPositions;
    int m_DesiredPositon = 0;
    bool m_IsLerping = false;

    void Update()
    {
        if (!m_IsLerping)
            return;

        if (transform.parent)
            transform.localPosition = Vector3.Lerp(transform.localPosition, m_LerpPositions[m_DesiredPositon], 5 * Time.deltaTime);
        else
            transform.position = Vector3.Lerp(transform.position, m_LerpPositions[m_DesiredPositon], 5 * Time.deltaTime);

        if (transform.localPosition == m_LerpPositions[m_DesiredPositon] && transform.parent || transform.position == m_LerpPositions[m_DesiredPositon] && !transform.parent)
        {
            m_IsLerping = false;
        }
    }

    public void LerpPositions(int _index)
    {
        m_DesiredPositon = _index;
        m_IsLerping = true;
    }
}
