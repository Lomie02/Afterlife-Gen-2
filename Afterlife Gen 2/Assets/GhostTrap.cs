using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GhostTrap : MonoBehaviour
{
    [SerializeField] GameObject m_PlaceHolderCan;

    [SerializeField] bool m_IsRepaired = false;

    bool m_HasBattery = false;
    bool m_HasTankCan = false;
    bool m_HasSparkplug = false;

    PhotonView m_MyView;

    private void Start()
    {
        m_MyView = GetComponent<PhotonView>();
    }

    public bool CollectedPart(ItemID _itemId)
    {
        switch (_itemId)
        {
            case ItemID.TrapBattery:
                m_MyView.RPC("RPC_GotBattery", RpcTarget.All);
                return true;
            case ItemID.TankCan:
                m_MyView.RPC("RPC_ShowTank", RpcTarget.All);
                return true;
            case ItemID.SparkPlug:
                m_MyView.RPC("RPC_GotSparkPlug", RpcTarget.All);
                return true;
        }

        return false;
    }

    [PunRPC]
    public void RPC_ShowTank()
    {
        m_HasTankCan = true;
        m_PlaceHolderCan.SetActive(true);
    }

    [PunRPC]
    public void RPC_GotBattery()
    {
        m_HasBattery = true;
    }

    [PunRPC]
    public void RPC_GotSparkPlug()
    {
        m_HasSparkplug = true;
    }
}
