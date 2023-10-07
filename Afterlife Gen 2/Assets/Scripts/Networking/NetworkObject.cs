using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public enum ItemID
{
    EMF = 0,
    Flashlight,
    Spiritbox,
    CamCorder,
    RemPod,
    Candle,
    GhostTrap,
    SantiyPill,
}

public class NetworkObject : MonoBehaviourPunCallbacks
{
    PhotonView m_MyView;
    [SerializeField] ItemID m_ItemsId;

    [Header("Basic")]
    [SerializeField] UnityEvent m_OnTurnedOn;

    [Space]
    [SerializeField] UnityEvent m_OnTurnedOff;
    [SerializeField] UnityEvent m_OnUsed;

    // Important
    bool m_IsItemOn = false;

    private void Start()
    {
        m_MyView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void SetObjectsState(bool _state)
    {
        gameObject.SetActive(_state);
    }

    public void RPC_SetObjectState(bool _state)
    {
        m_MyView.RPC("SetObjectsState", RpcTarget.All, _state);
    }

    [PunRPC]
    public void RPC_TurnObectOn()
    {
        m_OnTurnedOn.Invoke();
        m_IsItemOn = true;
    }

    [PunRPC]
    public void RPC_TurnObectOff()
    {
        m_OnTurnedOff.Invoke();
        m_IsItemOn = false;
    }
    [PunRPC]
    public void RPC_ObjectUse()
    {
        if (m_IsItemOn)
        {
            m_OnUsed.Invoke();
        }
    }

    public void TurnOn()
    {
        m_MyView.RPC("RPC_TurnObectOn", RpcTarget.All);
        Debug.Log("Turn On");
    }

    public void TurnOff()
    {
        m_MyView.RPC("RPC_TurnObectOff", RpcTarget.All);
    }

    public void UseItem()
    {
        m_MyView.RPC("RPC_ObjectUse", RpcTarget.All);
    }

    public ItemID GetItemsID()
    {
        return m_ItemsId;
    }
}
