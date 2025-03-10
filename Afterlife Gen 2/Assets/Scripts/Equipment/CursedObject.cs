using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class CursedObject : MonoBehaviour
{
    [SerializeField] GhostProfile[] m_Profiles;
    [SerializeField] string m_ObjectsName;

    bool m_IsCursedObject = false;
    bool m_CursedHasBeenRemoved = false;

    int m_SelectedProfile;
    PhotonView m_MyView;

    int m_SeedForRandomSpawn = 2;
    public static Vector3 RandomNavSphere(Vector3 origin, int layermask = -1)
    {
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(5f, 15f);

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, Random.Range(5f, 15f), layermask);

        return navHit.position;
    }

    private void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            m_SelectedProfile = Random.Range(0, m_Profiles.Length);

            for (int i = 0; i < m_SeedForRandomSpawn; i++)
            {
                transform.position = RandomNavSphere(transform.position);
            }

            m_MyView.RPC("RPC_AssignProfileList", RpcTarget.Others, m_SelectedProfile);
            m_MyView.RPC("RPC_AssignCursedObject", RpcTarget.All);
        }
    }

    public void BecomeCursedObject()
    {
        m_IsCursedObject = true;
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

    public void DestroyCursedObject()
    {
        m_MyView.RPC("RPC_RemoveCurseFromObject", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_RemoveCurseFromObject()
    {
        m_IsCursedObject = false;
        m_CursedHasBeenRemoved = true;
    }

    public bool HasCursedBeenRemoved()
    {
        return m_CursedHasBeenRemoved;
    }
}
