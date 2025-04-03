using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorModule : MonoBehaviour
{
    PhotonView m_MyView;
    Animator m_DoorAnimations;

    [SerializeField] AudioSource m_DoorAudio;
    [SerializeField] bool m_IsDoorLocked = false;

    bool m_IsDoorOpen = false;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_DoorAnimations = GetComponent<Animator>();

        if (PhotonNetwork.IsMasterClient)
            SetDoorRandomState();
    }

    /// <summary>
    /// Set door state over remote call
    /// </summary>
    /// <param name="_state"></param>
    [PunRPC]
    public void RPC_SetDoorState(bool _state)
    {
        m_IsDoorOpen = _state;
        m_DoorAnimations.SetBool("IsOpen", _state);
        m_DoorAudio.Play();
    }

    public bool GetDoorState()
    {
        return m_IsDoorOpen;
    }
    public void SetDoorRandomState()
    {
        if (m_IsDoorLocked) return;

        int RandomNum = Random.Range(0, 2);

        switch (RandomNum)
        {
            case 0:
                m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, false);
                break;
            case 1:
                m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, true);
                break;
            case 2:
                m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, false);
                break;
        }
    }

    public void LockDoorShut()
    {
        m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, false);
        m_MyView.RPC("RPC_LockDoor", RpcTarget.All, true);
    }

    public void UnlockDoor()
    {
        m_MyView.RPC("RPC_LockDoor", RpcTarget.All, false);
    }

    [PunRPC]
    public void RPC_LockDoor(bool _state)
    {
        m_IsDoorLocked = _state;
    }

    public void CycleDoorState()
    {
        if (m_IsDoorLocked) return;

        if (m_IsDoorOpen)
        {
            m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, false);
        }
        else
        {
            m_MyView.RPC("RPC_SetDoorState", RpcTarget.All, true);
        }
    }

    public bool IsDoorLocked()
    {
        return m_IsDoorLocked
    }
}
