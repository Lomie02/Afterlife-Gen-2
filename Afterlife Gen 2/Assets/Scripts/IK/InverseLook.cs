using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseLook : MonoBehaviour
{
    [SerializeField, Range(0, 1)] float m_Weight;
    [SerializeField] Transform m_Bone;
    [SerializeField] Transform m_Target;

    [SerializeField] Animator m_Animator;
    private void OnAnimatorIK(int layerIndex)
    {
        if (m_Target)
        {
            m_Animator.SetLookAtWeight(m_Weight);
            m_Animator.SetLookAtPosition(m_Target.position);
        }
    }
}
