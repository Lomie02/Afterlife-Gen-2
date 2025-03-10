using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;

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
    Ghostbox_T9,
    Useable,
    TankCan,
    SparkPlug,
    TrapBattery,
    TeddyBear,
    Urn,
    Locket,
    Cranium,
    Doll,
    Perma_Item,
    Laser_Projector,
}

public class NetworkObject : MonoBehaviourPunCallbacks
{
    [SerializeField] string m_ItemsName;

    public PhotonView m_MyView;
    [SerializeField] ItemID m_ItemsId;
    Rigidbody m_ItemsBody;

    [Header("Basic")]
    [SerializeField] UnityEvent m_OnTurnedOn;

    [Space]
    [SerializeField] UnityEvent m_OnTurnedOff;
    [SerializeField] UnityEvent m_OnUsed;

    [Header("Weight Layers")]
    [SerializeField] int m_WeightLayer = 2;
    // Important
    public bool m_IsItemOn = false;

    float m_Threshold = 0.05f;
    float m_IdleTime = 0;
    float m_IdleDuration = 2;

    public ItemID GetItemID()
    {
        return m_ItemsId;
    }

    private static byte[] JoinBytes(params byte[][] _arrays)
    {
        byte[] rv = new byte[_arrays.Sum(a => a.Length)];
        int offset = 0;
        foreach (byte[] array in _arrays)
        {
            System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
        }
        return rv;
    }

    public void RenameObject(string _name)
    {
        m_ItemsName = _name;
    }

    public string GetItemsName()
    {
        return m_ItemsName;
    }
    private void Start()
    {
        m_ItemsBody = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();

        if (m_ItemsBody != null)
        {
            if (!m_ItemsBody.isKinematic)
                StartCoroutine(CheckVelocity());
        }
    }

    IEnumerator CheckVelocity()
    {
        while (true)
        {
            if (m_ItemsBody.linearVelocity.magnitude < m_Threshold && m_ItemsBody.angularVelocity.magnitude > m_Threshold)
            {
                m_IdleTime += Time.deltaTime;
                if (m_IdleTime >= m_IdleDuration && !m_ItemsBody.isKinematic)
                {
                    m_ItemsBody.isKinematic = true;
                }
            }
            else
            {
                m_IdleTime = 0f;
                if (m_ItemsBody.isKinematic)
                {
                    m_ItemsBody.isKinematic = false;
                }
            }


            yield return new WaitForSeconds(1.5f);
        }
    }

    [PunRPC]
    public void SetObjectsState(bool _state)
    {
        gameObject.SetActive(_state);
    }

    public void RPC_SetObjectState(bool _state)
    {
        m_MyView.RPC("SetObjectsState", RpcTarget.AllBuffered, _state);
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

    [PunRPC]
    public void RPC_SetBodyState(bool _state)
    {
        if (_state)
        {
            m_ItemsBody.isKinematic = false;
        }
        else
        {
            m_ItemsBody.isKinematic = true;
        }
    }

    public void TurnOn()
    {
        m_MyView.RPC("RPC_TurnObectOn", RpcTarget.All);
    }

    public void TurnOff()
    {
        m_MyView.RPC("RPC_TurnObectOff", RpcTarget.All);
    }

    public void UseItem()
    {
        m_MyView.RPC("RPC_ObjectUse", RpcTarget.All);
    }

    public bool GetPowerState()
    {
        return m_IsItemOn;
    }

    public void SetPowerState(bool _state)
    {
        if (_state)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    public void SetBodysState(bool _state)
    {
        m_MyView.RPC("RPC_SetBodyState", RpcTarget.All, _state);
    }
    public void CyclePowerStage()
    {
        if (m_IsItemOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
    public int GetLayerWeight()
    {
        return m_WeightLayer;
    }


}
