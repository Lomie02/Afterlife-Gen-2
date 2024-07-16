using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CursedObject : MonoBehaviour
{
    [SerializeField] GhostProfile[] m_Profiles;
    [SerializeField] string m_ObjectsName;
    [SerializeField] bool m_IsCursedObject = false;

    int m_SelectedProfile;
    PhotonView m_MyView;

    private void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            m_SelectedProfile = Random.Range(0,m_Profiles.Length);
            m_MyView.RPC("RPC_AssignProfileList", RpcTarget.Others, m_SelectedProfile);
            m_MyView.RPC("RPC_AssignCursedObject", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_AssignProfileList(int _index)
    {
        m_SelectedProfile = _index;
    }

    [PunRPC]
    public void RPC_AssignCursedObject()
    {
        m_IsCursedObject = true;
    }
    public bool IsCursedObject()
    {
        return m_IsCursedObject;
    }

    public GhostProfile GetGhostProfile()
    {
        return m_Profiles[m_SelectedProfile];
    }
}
