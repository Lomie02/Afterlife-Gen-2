using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum TrapMode
{
    Needs_Repairs = 0,
    Needs_Power,
    ReadyForUse,
    Cooldown,
    Trapping,
}

public class GhostTrap : MonoBehaviour
{
    TrapMode m_TrapsMode = TrapMode.Needs_Repairs;
    [SerializeField] GameObject m_PlaceHolderCan;
    [SerializeField] GameObject m_BatteryPlaceholder;
    [SerializeField] bool m_IsRepaired = false;

    bool m_HasBattery = false;
    bool m_HasTankCan = false;

    PhotonView m_MyView;
    GhostAI m_GhostObject;

    [Header("Colliders")]
    [SerializeField] GameObject m_OxygenTankCollider;
    [SerializeField] GameObject m_BatteryCollider;
    [SerializeField] GameObject m_UseTrapButtonCollider;

    [SerializeField] Light m_TrapStateLight;

    PowerManager m_PowerManager;
    Canvas m_TrapInterface;
    [SerializeField] Text m_PowerStatus;

    [Header("Trap Stats")]
    [SerializeField] float m_CooldownDuration = 10f;
    float m_CooldownTimer;
    [SerializeField] GameObject m_ZapParticle;

    // Trap zaping

    bool m_EnteredTheAfterlife = false;
    float m_ZapTimer;
    float m_ZapTimerDuration = 5;

    private void Start()
    {

        m_ZapTimer = m_ZapTimerDuration;
        m_MyView = GetComponent<PhotonView>();
        m_TrapStateLight.color = Color.red;
        m_TrapInterface = GetComponentInChildren<Canvas>();
        m_PowerManager = FindFirstObjectByType<PowerManager>();

        m_UseTrapButtonCollider.SetActive(false);
        m_TrapInterface.gameObject.SetActive(false);
        m_CooldownTimer = m_CooldownDuration;

        m_ZapParticle.SetActive(false);
        m_GhostObject = FindAnyObjectByType<GhostAI>();
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
        }

        return false;
    }

    [PunRPC]
    public void RPC_ShowTank()
    {
        m_HasTankCan = true;
        m_OxygenTankCollider.SetActive(false);
        m_PlaceHolderCan.SetActive(true);
        CheckTrapPartsList();
    }

    private void Update()
    {
        switch (m_TrapsMode)
        {
            case TrapMode.Cooldown:
                m_CooldownTimer -= Time.deltaTime;

                if (m_CooldownTimer <= 0)
                {
                    m_CooldownTimer = m_CooldownDuration;

                    if (m_PowerManager.GetPowerState())
                        m_MyView.RPC("RPC_SetTrapsState", RpcTarget.All, TrapMode.ReadyForUse);
                    else
                        m_MyView.RPC("RPC_SetTrapsState", RpcTarget.All, TrapMode.Needs_Power);
                }
                break;

            case TrapMode.Trapping:

                m_ZapTimer -= Time.deltaTime;

                if (m_ZapTimer <= 0)
                {
                    m_ZapTimer = m_ZapTimerDuration;
                    m_TrapsMode = TrapMode.Cooldown;
                    m_ZapParticle.SetActive(false);
                }

                break;

        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (m_TrapsMode != TrapMode.Trapping) return;

        if (other.GetComponent<CursedObject>().IsCursedObject() && !other.GetComponent<CursedObject>().HasCursedBeenRemoved())
        {
            // Rename the object to have Cleansed at the end
            string TempName = other.GetComponent<NetworkObject>().GetItemsName();
            TempName += " (Cleansed)";

            m_MyView.RPC("RPC_CursedObjectHasBeenDestroyed", RpcTarget.MasterClient);

            other.GetComponent<NetworkObject>().RenameObject(TempName);
            other.GetComponent<CursedObject>().DestroyCursedObject();
        }
        else if (m_EnteredTheAfterlife && other.gameObject.GetComponent<GhostAI>())
        {
            m_MyView.RPC("RPC_GhostCaptured", RpcTarget.MasterClient);
            PhotonNetwork.Destroy(other.gameObject);
        }
    }

    [PunRPC]
    public void RPC_GhostCaptured()
    {
        // Spawn the escape portal.
    }

    [PunRPC]
    public void RPC_CursedObjectHasBeenDestroyed()
    {
        if (!m_GhostObject) m_GhostObject = FindAnyObjectByType<GhostAI>();

        m_GhostObject.EnteredAfterlifeRealm();

        GameObject[] m_Players = GameObject.FindGameObjectsWithTag("Player");

        m_EnteredTheAfterlife = true;
        for (int i = 0; i < m_Players.Length; i++)
        {
            m_Players[i].GetComponent<PlayerController>().EnterTheAfterlife();
        }
    }


    [PunRPC]
    public void RPC_GotBattery()
    {
        m_HasBattery = true;
        m_BatteryCollider.SetActive(false);
        m_BatteryPlaceholder.SetActive(true);
        CheckTrapPartsList();
    }

    public void UpdateTrapsMode()
    {
        if (!m_PowerManager.GetPowerState())
            m_TrapsMode = TrapMode.Needs_Power;
        else
            m_TrapsMode = TrapMode.ReadyForUse;

        m_MyView.RPC("RPC_SetTrapsState", RpcTarget.All, m_TrapsMode);
    }

    [PunRPC]
    public void RPC_SetTrapsState(TrapMode _state)
    {
        m_TrapsMode = _state;

        UpdateTrapInterface();
    }

    void CheckTrapPartsList()
    {
        if (IsTrapFullyRepaird())
        {
            m_TrapStateLight.color = Color.green;
            m_UseTrapButtonCollider.SetActive(true);
            m_TrapInterface.gameObject.SetActive(true);

            if (!m_PowerManager.GetPowerState())
                m_TrapsMode = TrapMode.Needs_Power;
            else
                m_TrapsMode = TrapMode.Cooldown;

            m_UseTrapButtonCollider.SetActive(true);
            UpdateTrapInterface();
        }
    }

    void UpdateTrapInterface()
    {
        switch (m_TrapsMode)
        {
            case TrapMode.Needs_Power:
                m_PowerStatus.text = "POWER REQUIRED";
                m_PowerStatus.color = Color.red;
                break;

            case TrapMode.ReadyForUse:
                m_PowerStatus.text = "READY";
                m_PowerStatus.color = Color.green;
                break;

            case TrapMode.Trapping:
                m_PowerStatus.text = "CHARGING TRAP";
                m_PowerStatus.color = Color.yellow;
                break;
        }
    }

    bool IsTrapFullyRepaird()
    {
        return m_HasBattery && m_HasTankCan;
    }

    public void StartTrapSequence()
    {
        if (m_TrapsMode != TrapMode.Trapping || m_TrapsMode != TrapMode.Needs_Power)
            m_MyView.RPC("RPC_StartTrapSeq", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_StartTrapSeq()
    {
        //TODO: Make trap charge up & destroy the cursed object
        m_ZapParticle.SetActive(true);

        m_TrapsMode = TrapMode.Trapping;
        UpdateTrapInterface();
    }
}
