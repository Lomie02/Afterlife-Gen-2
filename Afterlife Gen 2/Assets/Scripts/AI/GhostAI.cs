using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.Animations;
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

    [Space]
    public RuntimeAnimatorController m_Controller;
    public Avatar m_Avatar;
}

public class GhostAI : MonoBehaviour
{
    Animator m_GhostAnimation;

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

    // idling Vars
    float m_IdleDuration = 5;
    float m_IdleTimer = 0;

    Transform m_PlayerTarget; // Player to chase if ghost finds them
    List<GameObject> m_PlayersGhostCanSee;
    [SerializeField] Transform m_RayView;

    float m_ChaseDuration = 5;
    float m_ChaseTimer = 0;

    // cooldown for attacks
    float m_AttackCooldown = 3f;
    float m_AttackTimer;
    bool m_AttackIsOnCooldown = false;

    UnityEvent m_OnGhostDeath;
    GameObject m_ExfillArea;

    private void Start()
    {
        m_MyAgent = GetComponent<NavMeshAgent>();
        m_PowerManager = FindFirstObjectByType<PowerManager>();

        m_IdleDuration = Random.Range(5f, 9f);
        m_IdleTimer = m_IdleDuration;

        m_ChaseTimer = m_ChaseDuration;
        m_AttackTimer = m_AttackCooldown;

        m_GhostAnimation = GetComponent<Animator>();

        if (PhotonNetwork.IsMasterClient)
        {
            m_PlayersGhostCanSee = new List<GameObject>();
            m_View.RPC("RPC_AssignGhostKit", RpcTarget.All, 0);
            m_TimerCooldownLights = m_TimerCooldownLightsDuration;
            m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }


        // Exfill data for when ghost is killed.
        m_ExfillArea = FindAnyObjectByType<ReadyZone>().gameObject.transform.parent.gameObject;
        m_ExfillArea.SetActive(false);
        m_OnGhostDeath.AddListener(delegate { m_ExfillArea.SetActive(true); });

        StartCoroutine(CheckInteractions());
        StartCoroutine(CheckDoorInteractions());
    }

    [PunRPC]
    public void RPC_AssignGhostKit(int _index)
    {
        m_GhostKits[_index].m_GhostObject.SetActive(true);
        m_CurrentGhostKitActive = _index;

        // Set Animator up.
        m_GhostAnimation.runtimeAnimatorController = m_GhostKits[_index].m_Controller;
        m_GhostAnimation.avatar = m_GhostKits[_index].m_Avatar;

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



        if (m_AttackIsOnCooldown)
        {
            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer <= 0)
            {
                m_AttackTimer = m_AttackCooldown;
                m_AttackIsOnCooldown = false;
            }
        }

        if (m_LightInteractionOnCooldown)
        {
            m_TimerCooldownLights -= Time.deltaTime;
            if (m_TimerCooldownLights <= 0)
            {
                m_TimerCooldownLights = m_TimerCooldownLightsDuration;
                m_LightInteractionOnCooldown = false;
            }
        }

        m_GhostAnimation.SetFloat("GhostSpeed", m_MyAgent.velocity.magnitude);
    }



    void UpdateIdle()
    {
        m_IdleTimer -= Time.deltaTime;

        if (m_IdleTimer <= 0)
        {
            m_IdleDuration = Random.Range(5f, 9f);
            m_IdleTimer = m_IdleDuration;
            m_GhostsBehaviour = GhostBehaviour.Seeking;
        }
    }
    void UpdateSeekingBehaviour()
    {
        if (m_MyAgent.remainingDistance < 2)
        {
            int RandomBehaviour = Random.Range(0, 3);

            if (RandomBehaviour == 1)
            {
                m_GhostsBehaviour = GhostBehaviour.Idle; return;
            }
            else
                m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }
    }

    void CheckIfCanAttackTarget()
    {
        if (!m_PlayerTarget) return;

        RaycastHit HitDetectionInfo;

        Vector3 DirectionOfTarget = m_PlayerTarget.position - m_RayView.position;
        DirectionOfTarget = DirectionOfTarget.normalized;

        if (Physics.Raycast(m_RayView.position, DirectionOfTarget, out HitDetectionInfo, 2.5f))
        {
            if (HitDetectionInfo.collider.GetComponentInParent<PlayerController>())
            {
                HitDetectionInfo.collider.GetComponentInParent<PhotonView>().RPC("RPC_TakeDamage", HitDetectionInfo.collider.GetComponentInParent<PhotonView>().Owner, Random.Range(0.1f, 0.3f));
                m_AttackIsOnCooldown = true;
            }
        }
    }

    private IEnumerator CheckDoorInteractions()
    {
        while (true)
        {
            RaycastHit HitDetectionInfo;

            if (Physics.Raycast(m_RayView.position, m_RayView.forward, out HitDetectionInfo, 1.5f))
            {
                if (HitDetectionInfo.collider.GetComponent<DoorModule>())
                {
                    HitDetectionInfo.collider.GetComponent<DoorModule>().CycleDoorState();
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    [PunRPC]
    public void RPC_GhostDeath()
    {

    }

    IEnumerator CheckInteractions()
    {
        while (true)
        {

            // Interaction with power
            float DistanceToPowerBox = Vector3.Distance(transform.position, m_PowerManager.gameObject.transform.position);
            if (DistanceToPowerBox < 7 && m_PowerManager.GetPowerState() && !m_LightInteractionOnCooldown)
            {
                int RandomNum = Random.Range(0, 10);

                if (RandomNum == 2)
                    m_PowerManager.CyclePower();

                m_LightInteractionOnCooldown = true;
            }

            // Player detection
            if (m_PlayersGhostCanSee.Count > 0 && m_PlayerTarget == null)
                UpdatePlayerDetection();

            // If AI can attack the player
            if (!m_AttackIsOnCooldown)
                CheckIfCanAttackTarget();

            // Equipment
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

            yield return new WaitForSeconds(3f);
        }
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

        m_GhostManager = FindAnyObjectByType<GhostManager>();
        m_View.RPC("RPC_GrabGhostProfile", RpcTarget.All, _index);
    }

    [PunRPC]
    public void RPC_GrabGhostProfile(int _index)
    {
        m_Profile = m_GhostManager.GrabGhostProfile(_index);
    }

    public void PlayerHasEnteredView(GameObject _playerObject)
    {
        m_PlayersGhostCanSee.Add(_playerObject);
        m_MyAgent.SetDestination(RandomNavSphere(_playerObject.transform.position, 10, -1));
    }

    public void PlayerHasExitView(GameObject _playerObject)
    {
        m_PlayersGhostCanSee.Remove(_playerObject);
    }

    bool UpdatePlayerDetection()
    {
        RaycastHit HitDetectionInfo;

        for (int i = 0; i < m_PlayersGhostCanSee.Count; i++)
        {
            Vector3 DirectionOfTarget = m_PlayersGhostCanSee[i].transform.position - m_RayView.position;
            DirectionOfTarget = DirectionOfTarget.normalized;

            if (Physics.Raycast(m_RayView.position, DirectionOfTarget, out HitDetectionInfo, 15f))
            {
                if (HitDetectionInfo.collider.GetComponentInParent<PlayerController>())
                {
                    m_PlayerTarget = m_PlayersGhostCanSee[i].transform;
                    m_GhostsBehaviour = GhostBehaviour.Hunt;
                    return true;
                }
            }
        }

        return false;
    }

    void HuntPlayer()
    {
        if (UpdatePlayerDetection())
        {
            m_ChaseTimer = m_ChaseDuration;
        }

        m_MyAgent.SetDestination(m_PlayerTarget.position);

        m_ChaseTimer -= Time.deltaTime;
        if (m_ChaseTimer <= 0)
        {
            m_ChaseTimer = m_ChaseDuration;
            m_PlayerTarget = null;
            m_GhostsBehaviour = GhostBehaviour.Idle; return;
        }
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
