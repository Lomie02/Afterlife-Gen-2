using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Photon.Pun;
public class ModelHider : MonoBehaviour
{
    PhotonView m_MyView;

    [SerializeField] GameObject m_BodyModel;
    [SerializeField] GameObject m_LegsModel;
    [SerializeField] GameObject m_ArmsModel;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        if (m_MyView.IsMine)
        {
            SetBodyToShadowsOnly(m_BodyModel, ShadowCastingMode.ShadowsOnly);
            SetBodyToShadowsOnly(m_LegsModel, ShadowCastingMode.Off);
        }
        else
        {
            m_LegsModel.SetActive(false);
            m_ArmsModel.SetActive(false);
        }
    }

    void SetBodyToShadowsOnly(GameObject _target, ShadowCastingMode _mask)
    {
        foreach (Transform Child in _target.transform)
        {
            if (Child.GetComponent<SkinnedMeshRenderer>())
            {
                Child.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = _mask;
            }
            else if(Child.GetComponent<MeshRenderer>())
            {
                Child.GetComponent<MeshRenderer>().shadowCastingMode = _mask;
            }
        }
    }
}
