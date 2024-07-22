using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PowerManager : MonoBehaviour
{
    [SerializeField] GameObject m_LightParent;

    [SerializeField] Light m_PowerLight;

    Light[] m_LightsToPower;
    bool m_IsPowerOn = false;

    GhostTrap m_GhostTrap;
    PhotonView m_MyView;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_LightsToPower = m_LightParent.GetComponentsInChildren<Light>();

        m_GhostTrap = FindFirstObjectByType<GhostTrap>();
        if (m_IsPowerOn)
            m_MyView.RPC("RPC_SetPowerState", RpcTarget.AllBuffered, true);
        else
            m_MyView.RPC("RPC_SetPowerState", RpcTarget.AllBuffered, false);
    }

    [PunRPC]
    public void RPC_SetPowerState(bool _state)
    {
        m_IsPowerOn = _state;

        if (m_IsPowerOn)
            m_PowerLight.color = Color.green;
        else
            m_PowerLight.color = Color.red;

        for (int i = 0; i < m_LightsToPower.Length; i++)
            m_LightsToPower[i].gameObject.SetActive(_state);

        m_GhostTrap.UpdateTrapsMode();
    }

    public bool GetPowerState()
    {
        return m_IsPowerOn;
    }

    public void CyclePower()
    {
        if (m_IsPowerOn)
            m_MyView.RPC("RPC_SetPowerState", RpcTarget.AllBuffered, false);
        else
            m_MyView.RPC("RPC_SetPowerState", RpcTarget.AllBuffered, true);
    }
}
