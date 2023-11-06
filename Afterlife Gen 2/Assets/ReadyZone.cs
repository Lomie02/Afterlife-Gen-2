using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ReadyZone : MonoBehaviour
{
    [SerializeField] PhotonView m_View;
    [SerializeField] bool m_HostIsReady = false;
    [SerializeField] PositonLerp m_ReadyUpDoor;

    public bool ReadyUpHost()
    {
        if (!PhotonNetwork.IsMasterClient)
            return false;

        m_ReadyUpDoor.LerpPositions(0);
        m_HostIsReady = true;
        return true;
    }
}
