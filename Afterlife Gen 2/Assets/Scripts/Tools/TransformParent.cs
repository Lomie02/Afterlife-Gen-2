using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformParent : MonoBehaviour
{
    [SerializeField] Transform m_ParentTransform;
    bool m_LockRotationToParent = false;

    public bool m_LockY = true;
    public bool m_LockZ = true;
    public bool m_LockX = true;
    Vector3 m_NewPosition;

    void LateUpdate()
    {
        if (!m_LockX && !m_LockY && !m_LockZ) return;

        if (m_LockX)
            m_NewPosition.x = m_ParentTransform.position.x;
        else
            m_NewPosition.x = transform.position.x;

        if (m_LockY)
            m_NewPosition.y = m_ParentTransform.position.y;
        else
            m_NewPosition.y = transform.position.y;

        if (m_LockZ)
            m_NewPosition.z = m_ParentTransform.position.z;
        else
            m_NewPosition.z = transform.position.z;

        transform.position = m_NewPosition;

        if (m_LockRotationToParent)
        {
            transform.rotation = m_ParentTransform.rotation;
        }

    }
}
