using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Animations.Rigging;

public enum TrapMode
{
    Needs_Repairs = 0,
    Needs_Power,
    ReadyForUse,
    Cooldown,
    Trapping,
}

[System.Serializable]
public class GhostData
{
    public string m_GhostType;
    public List<string> m_GhostEvidence;
}

public class GhostDatabase : MonoBehaviour
{
    public List<GhostData> m_GhostLib = new List<GhostData>();
    public GhostDatabase()
    {
        m_GhostLib.Add(new GhostData { m_GhostType = "Poltergeist", m_GhostEvidence = new List<string> { "EMF", "REMPOD", "GHOSTBOX", "LASER PROJECTOR" } });
        m_GhostLib.Add(new GhostData { m_GhostType = "Spirit", m_GhostEvidence = new List<string> { "EMF", "BLOOD TRAIL", "GHOSTBOX", "LASER PROJECTOR" } });
        m_GhostLib.Add(new GhostData { m_GhostType = "Phantom", m_GhostEvidence = new List<string> { "EMF", "BLOOD TRAIL", "FLOATING OBJECTS", "REM POD" } });
    }
}

public class GhostTrap : MonoBehaviour
{
    TrapMode m_TrapsMode = TrapMode.Needs_Repairs;
    [SerializeField] GameObject m_PlaceHolderCan;
    [SerializeField] GameObject m_BatteryPlaceholder;

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
    Light m_MoonDirectional;

    ObjectiveManager m_ObjectiveManager;

    [SerializeField] Transform m_TrapStandPosition;
    public UnityEvent m_OnExitTrap;

    [Header("Interface")]
    [SerializeField] Button m_TrapExitButton;

    [SerializeField] Dropdown m_EvidenceSelection1;
    [SerializeField] Dropdown m_EvidenceSelection2;
    [SerializeField] Dropdown m_EvidenceSelection3;
    [SerializeField] Dropdown m_EvidenceSelection4;

    [SerializeField] Button m_StartTrap;

    [SerializeField] Text m_GhostTypeText;
    public GhostDatabase m_GhostDataPack;

    private List<string> m_AllEvidenceItems = new List<string>() { "EMF","REM POD", "GHOSTBOX", "LASER PROJECTOR", "BLOOD TRAIL", "FLOATING OBJECTS"};

    private void Start()
    {
        m_GhostDataPack = new GhostDatabase();
        m_ZapTimer = m_ZapTimerDuration;
        m_MyView = GetComponent<PhotonView>();
        m_TrapStateLight.color = Color.red;
        m_TrapInterface = GetComponentInChildren<Canvas>();

        m_UseTrapButtonCollider.SetActive(false);
        m_TrapInterface.gameObject.SetActive(false);
        m_CooldownTimer = m_CooldownDuration;

        m_ZapParticle.SetActive(false);
        m_GhostObject = FindAnyObjectByType<GhostAI>();

        m_ObjectiveManager = FindFirstObjectByType<ObjectiveManager>();
        m_MoonDirectional = GameObject.Find("MoonLight").GetComponent<Light>();
        m_TrapExitButton.onClick.AddListener(m_OnExitTrap.Invoke);
        m_PowerManager = FindFirstObjectByType<PowerManager>();

        ApplyDropdownOptions(m_EvidenceSelection1);
        ApplyDropdownOptions(m_EvidenceSelection2);
        ApplyDropdownOptions(m_EvidenceSelection3);
        ApplyDropdownOptions(m_EvidenceSelection4);

        m_MyView.RPC("RPC_UpdateTrapScreenInterface", RpcTarget.All);

        StartCoroutine(UpdateGhostTrap());
    }

    [PunRPC]
    void RPC_UpdateTrapScreenInterface()
    {
        List<string> ResearchFound = new List<string>()
        {
            m_EvidenceSelection1.options[m_EvidenceSelection1.value].text,
            m_EvidenceSelection2.options[m_EvidenceSelection2.value].text,
            m_EvidenceSelection3.options[m_EvidenceSelection3.value].text,
            m_EvidenceSelection4.options[m_EvidenceSelection4.value].text
        }.Where(m_GhostEvidence => m_GhostEvidence != "UNKNOWN").ToList();

        List<string> m_PotentialGhost = m_GhostDataPack.m_GhostLib.Where(ghost => ResearchFound.All(m_GhostEvidence => ghost.m_GhostEvidence.Contains(m_GhostEvidence)))
            .Select(ghost => ghost.m_GhostType).ToList();

        m_GhostTypeText.text = (m_PotentialGhost.Count > 0 ? string.Join("\n", m_PotentialGhost) : "UNKNOWN");
    }

    void ApplyDropdownOptions(Dropdown _object)
    {
        _object.ClearOptions();
        List<string> NewOptions = new List<string>() { "UNKNOWN" };
        NewOptions.AddRange(m_AllEvidenceItems);
        _object.AddOptions(NewOptions);

        _object.onValueChanged.AddListener(delegate { m_MyView.RPC("RPC_UpdateTrapScreenInterface", RpcTarget.All); });
    }

    [PunRPC]
    public void RPC_UpdateDropdowntext(int _index)
    {

    }

    public Transform GetTrapScreenPosition()
    {
        return m_UseTrapButtonCollider.transform;
    }

    public Transform GetTrapStandingPlacement()
    {
        return m_TrapStandPosition;
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

    private IEnumerator UpdateGhostTrap()
    {
        while (true)
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

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (m_TrapsMode != TrapMode.Trapping && m_ObjectiveManager.GetCurrentObjective().m_Tag == "Trap") return;

        if (other.GetComponent<CursedObject>().IsCursedObject() && !other.GetComponent<CursedObject>().HasCursedBeenRemoved())
        {
            // Rename the object to have Cleansed at the end
            string TempName = other.GetComponent<NetworkObject>().GetItemsName();
            TempName += " (Cleansed)";

            m_MyView.RPC("RPC_CursedObjectHasBeenDestroyed", RpcTarget.MasterClient);

            other.GetComponent<NetworkObject>().RenameObject(TempName);
            other.GetComponent<CursedObject>().DestroyCursedObject();
        }
        else if (m_EnteredTheAfterlife && other.gameObject.GetComponent<GhostAI>() && m_ObjectiveManager.GetCurrentObjective().m_Tag == "Trap_Ghost")
        {
            m_ObjectiveManager.ObjectiveCompleted("Trap_Ghost");
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
        m_MoonDirectional.color = Color.red;

        for (int i = 0; i < m_Players.Length; i++)
        {
            m_Players[i].GetComponent<PlayerController>().EnterTheAfterlife();
        }

        m_ObjectiveManager.ObjectiveCompleted("Cursed_Object");
    }

    public void CalibrateTrap()
    {
        if(m_GhostObject.GetGhostProfile().m_GhostName == m_GhostTypeText.text)
        {
            m_ObjectiveManager.ObjectiveCompleted("Ghost");
            m_MyView.RPC("RPC_TrapCalibrated", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_TrapCalibrated()
    {
        m_StartTrap.interactable = true;
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
        if (!m_PowerManager) m_PowerManager = FindFirstObjectByType<PowerManager>();

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

            m_ObjectiveManager.ObjectiveCompleted("Trap");
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
