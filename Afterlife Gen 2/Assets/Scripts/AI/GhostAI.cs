using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
enum GhostBehaviour
{
    Idle = 0,
    Seeking,
    Hunt,
    Attack,
    Trap,
}

[System.Serializable]
struct GhostModelKit
{
    public string m_GhostName;
    public GameObject m_GhostObject;

    public Material m_GhostsMaterial;
}

public class GhostAI : MonoBehaviour
{
    [SerializeField] GhostModelKit[] m_GhostKits;
    [Space]

    [SerializeField] GhostProfile m_Profile;
    PowerManager m_PowerManager;

    [SerializeField] PhotonView m_View;
    GhostBehaviour m_GhostsBehaviour = GhostBehaviour.Seeking;

    NavMeshAgent m_MyAgent;
    [SerializeField] GhostManager m_GhostManager;
    [SerializeField] float m_RoamingDistance = 50;

    bool m_LightInteractionOnCooldown = false;
    float m_TimerCooldownLights;
    float m_TimerCooldownLightsDuration = 4;

    bool m_IsGhostRevealingTrueForm = false;
    int m_CurrentGhostKitActive = 0;

    private void Start()
    {
        m_MyAgent = GetComponent<NavMeshAgent>();
        m_PowerManager = FindFirstObjectByType<PowerManager>();
        if (PhotonNetwork.IsMasterClient)
        {
            m_View.RPC("RPC_AssignGhostKit", RpcTarget.All, 0);
            m_TimerCooldownLights = m_TimerCooldownLightsDuration;
            m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }
    }

    [PunRPC]
    public void RPC_AssignGhostKit(int _index)
    {
        m_GhostKits[_index].m_GhostObject.SetActive(true);
        m_CurrentGhostKitActive = _index;

        if (!m_IsGhostRevealingTrueForm)
            m_GhostKits[_index].m_GhostsMaterial.SetFloat("_AfterlifeForm", 0f); // false
        else
            m_GhostKits[_index].m_GhostsMaterial.SetFloat("_AfterlifeForm", 1f); // True
    }

    public void EnteredAfterlifeRealm()
    {
        m_View.RPC("RPC_ShowGhostTrueForm", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_ShowGhostTrueForm()
    {
        m_IsGhostRevealingTrueForm = true;
        m_GhostKits[m_CurrentGhostKitActive].m_GhostsMaterial.SetFloat("_AfterlifeForm", 1f); // True
    }

    void Update() // Entire AI Is going to only be controlled on the Hosts side. 
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        switch (m_GhostsBehaviour)
        {
            case GhostBehaviour.Idle:
                UpdateIdle();
                break;

            case GhostBehaviour.Seeking:
                UpdateSeekingBehaviour();
                break;

            case GhostBehaviour.Hunt:
                HuntPlayer();
                break;

            case GhostBehaviour.Attack:
                break;

            case GhostBehaviour.Trap:
                break;
        }

        CheckInteractions();

        if (m_LightInteractionOnCooldown)
        {
            m_TimerCooldownLights -= Time.deltaTime;
            if (m_TimerCooldownLights <= 0)
            {
                m_TimerCooldownLights = m_TimerCooldownLightsDuration;
                m_LightInteractionOnCooldown = false;
            }
        }
    }

    void UpdateIdle()
    {

    }

    void UpdateSeekingBehaviour()
    {
        if (m_MyAgent.remainingDistance < 2)
        {
            m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }

        RaycastHit m_CastInfo;

        if (Physics.Raycast(transform.position, transform.forward, out m_CastInfo, 4))
        {

            Debug.Log(m_CastInfo.collider.name);

            if (m_CastInfo.collider.GetComponent<Destructable_Object>())
                m_CastInfo.collider.GetComponent<Destructable_Object>().DestroyObject();
        }
    }

    void CheckInteractions()
    {
        float Distance = Vector3.Distance(transform.position, m_PowerManager.gameObject.transform.position);
        if (Distance < 12 && m_PowerManager.GetPowerState() && !m_LightInteractionOnCooldown)
        {
            int RandomNum = Random.Range(0, 5);

            if (RandomNum == 2)
                m_PowerManager.CyclePower();

            m_LightInteractionOnCooldown = true;
        }


        switch (m_Profile.m_Evidence1)
        {
            case EvidenceTypes.SpiritBox:
                CheckSpiritBox();
                break;

            case EvidenceTypes.Emf:
                CheckEmf();
                break;

            case EvidenceTypes.Writing:
                CheckWriting();
                break;

            case EvidenceTypes.LaserProjector:
                break;

            case EvidenceTypes.BloodyHandprints:
                break;

            case EvidenceTypes.GhostOrb:
                break;

            case EvidenceTypes.AudioSensor:
                break;

            case EvidenceTypes.FreezingTemps:
                break;

            case EvidenceTypes.FloatingObjects:
                break;

            case EvidenceTypes.RemPod:
                break;
        }

        //TODO: Add rest of evidence
    }

    public GhostProfile GetGhostProfile()
    {
        return m_Profile;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    public void SetGhostProfile(int _index)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        m_GhostManager = FindObjectOfType<GhostManager>();
        m_View.RPC("RPC_GrabGhostProfile", RpcTarget.All, _index);
    }

    [PunRPC]
    public void RPC_GrabGhostProfile(int _index)
    {
        m_Profile = m_GhostManager.GrabGhostProfile(_index);
    }

    void HuntPlayer()
    {

    }

    void CheckSpiritBox()
    {

    }

    void CheckEmf()
    {

    }

    void StartGhostEvent()
    {

    }

    void CheckWriting()
    {

    }

    public GhostProfile GetGhostCurrentProfile()
    {
        return m_Profile;
    }
}
