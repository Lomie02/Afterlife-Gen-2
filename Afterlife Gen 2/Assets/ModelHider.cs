using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            SetBodyToShadowsOnly();
        }
        else
        {
            m_LegsModel.SetActive(false);
            m_ArmsModel.SetActive(false);
        }
    }

    void SetBodyToShadowsOnly()
    {
        foreach (Transform Child in m_BodyModel.transform)
        {
            if (Child.GetComponent<SkinnedMeshRenderer>())
            {
                Child.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            else if(Child.GetComponent<MeshRenderer>())
            {
                Child.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
