using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformParent : MonoBehaviour
{
    [SerializeField] Transform m_ParentTransform;

    bool m_LockRotationToParent = false;
    void LateUpdate()
    {
        transform.position = m_ParentTransform.position;

        if (m_LockRotationToParent)
        {
            transform.rotation = m_ParentTransform.rotation;
        }

    }
}
