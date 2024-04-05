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


    // ===== List of objects to Directly SetToShadowOnly
    [Header("ShadowOnly")]
    [SerializeField] GameObject[] m_ObjectsToShadowOnly;

    [Header("Layer Change")]
    [SerializeField] LayerMask m_LayerToChangeTo;
    [SerializeField] GameObject[] m_ObjectsToChangeLayer;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        if (m_MyView.IsMine)
        {
            m_LegsModel.SetActive(true);
            
            SetBodyToShadowsOnly(m_BodyModel, ShadowCastingMode.ShadowsOnly);
            SetBodyToShadowsOnly(m_LegsModel, ShadowCastingMode.Off);

            for (int i = 0; i < m_ObjectsToShadowOnly.Length; i++)
                SetDirectToShadowsOnly(m_ObjectsToShadowOnly[i], ShadowCastingMode.ShadowsOnly);

            for (int i = 0; i < m_ObjectsToChangeLayer.Length; i++)
                ChangeObjectLayer(m_ObjectsToChangeLayer[i], m_LayerToChangeTo);

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
            else if (Child.GetComponent<MeshRenderer>())
            {
                Child.GetComponent<MeshRenderer>().shadowCastingMode = _mask;
            }
        }
    }


    // Directly Set a objects shadow casting
    void SetDirectToShadowsOnly(GameObject _target, ShadowCastingMode _mask)
    {

        if (_target.GetComponent<SkinnedMeshRenderer>())
        {
            _target.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = _mask;
        }
        else if (_target.GetComponent<MeshRenderer>())
        {
            _target.GetComponent<MeshRenderer>().shadowCastingMode = _mask;
        }
    }

    //Change a ObjectsRenderLayer;

    void ChangeObjectLayer(GameObject _Target, LayerMask _mask)
    {
        _Target.layer = _mask;
    }
}
