using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformOffset : MonoBehaviour
{
    [SerializeField] Vector3 m_OffsetPosition;
    [SerializeField] Transform m_TargetParent;
    void LateUpdate()
    {
        transform.position = m_TargetParent.position;
        transform.rotation = m_TargetParent.rotation;

        transform.localPosition += m_OffsetPosition;
    }
}
