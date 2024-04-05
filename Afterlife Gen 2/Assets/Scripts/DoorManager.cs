using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[System.Serializable]
struct DoorAsset
{
    public string m_Name;
    public Transform m_Object;

    public Quaternion m_ClosedRotation;
    public Quaternion m_OpenRotation;
}
public class DoorManager : MonoBehaviour
{
    PhotonView m_MyView;

    [SerializeField] DoorAsset[] m_DoorsList;
    [SerializeField] float m_DoorOpenSpeed;

    bool m_isDoorOpen = false;
    bool m_CanDoorsBeOpened = true;

    bool m_DoorUpdate = false;
    [PunRPC]
    public void Start()
    {
        m_MyView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void RPC_SetDoorState(bool _state)
    {
        if (!m_CanDoorsBeOpened)
            return;
        m_isDoorOpen = _state;
        m_DoorUpdate = true;
    }

    [PunRPC]
    public void RPC_SetDoorLockState(bool _state)
    {
        m_CanDoorsBeOpened = _state;
    }

    public bool IsDoorOpen()
    {
        return m_isDoorOpen;
    }

    public void CycleDoor()
    {
        if (m_isDoorOpen)
        {
            m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, false);
        }
        else
        {
            m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, true);
        }
    }
    public void Update()
    {
        if (m_DoorUpdate)
        {
            for (int i = 0; i < m_DoorsList.Length; i++)
            {
                if (m_isDoorOpen)
                {
                    m_DoorsList[i].m_Object.rotation = Quaternion.Lerp(m_DoorsList[i].m_Object.rotation, m_DoorsList[i].m_OpenRotation, m_DoorOpenSpeed * Time.deltaTime);
                }
                else
                {
                    m_DoorsList[i].m_Object.rotation = Quaternion.Lerp(m_DoorsList[i].m_Object.rotation, m_DoorsList[i].m_ClosedRotation, m_DoorOpenSpeed * Time.deltaTime);
                }
            }
        }
    }
}
