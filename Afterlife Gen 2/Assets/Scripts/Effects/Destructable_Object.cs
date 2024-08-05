using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Destructable_Object : MonoBehaviour
{
    [SerializeField] GameObject m_FixedObject;
    [SerializeField] GameObject m_DestroyedObject;
    [SerializeField] GameObject m_BrokenPieces;

    [SerializeField] Collider m_HitBoxForDetection;

    PhotonView m_MyView;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        m_FixedObject.SetActive(true);
        m_DestroyedObject.SetActive(false);
        m_BrokenPieces.SetActive(false);
    }

    public void DestroyObject()
    {
        m_MyView.RPC("RPC_DestroyWall", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_DestroyWall()
    {
        m_HitBoxForDetection.enabled = false;

        m_FixedObject.SetActive(false);
        m_DestroyedObject.SetActive(true);
        m_BrokenPieces.SetActive(true);
    }
}
