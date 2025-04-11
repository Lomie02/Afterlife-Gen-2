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
    Patrol,
    Investigate,
    Chase,
    Search,
    Attack,
    Trap,
}

[System.Serializable]
struct GhostModelKit
{
    public string m_GhostName;
    public GameObject m_GhostObject;

    public Material[] m_GhostsMaterial;

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
    GhostBehaviour m_GhostsBehaviour = GhostBehaviour.Patrol;

    NavMeshAgent m_MyAgent;
    [SerializeField] GhostManager m_GhostManager;
    [SerializeField] float m_RoamingDistance = 50;

    [Header("Evidence Objects")]
    [SerializeField] GameObject m_GhostOrbs;
    [SerializeField] GameObject BloodTrails;

    bool m_LightInteractionOnCooldown = false;
    float m_TimerCooldownLights;
    float m_TimerCooldownLightsDuration = 4;

    bool m_IsGhostRevealingTrueForm = false;
    int m_CurrentGhostKitActive = 0;

    PlayerController[] m_PlayersInLobby;

    // idling Vars
    float m_IdleDuration = 5;
    float m_IdleTimer = 0;

    Transform m_PlayerTarget; // Player to chase if ghost finds them

    float m_ChaseDuration = 5;
    float m_ChaseTimer = 0;

    // cooldown for attacks
    float m_AttackCooldown = 2f;
    float m_AttackTimer;
    bool m_AttackIsOnCooldown = false;

    UnityEvent m_OnGhostDeath;
    GameObject m_ExfillArea;

    [SerializeField] Transform m_GhostEmfLocation;
    bool m_GiveEmfActivity;

    int m_EmfLevel;
    float m_EmfActivityTimer;
    float m_EmfActivityDuration;

    float m_GhostInteractionTimer;
    float m_GhostInteractionDuration;

    GameObject[] m_PatrolZones;

    float m_SearchTimer;
    float m_SearchDuration = 5f;

    float m_AttackRange = 1.7f;
    float m_EyeSightRange = 10f;
    float m_GhostFieldofView = 90f;

    Vector3 m_lastKnownSighting;
    int m_CurrentPatrolZone;
    private void Start()
    {
        m_MyAgent = GetComponent<NavMeshAgent>();
        m_PowerManager = FindFirstObjectByType<PowerManager>();

        m_IdleDuration = Random.Range(5f, 9f);
        m_IdleTimer = m_IdleDuration;

        m_ChaseTimer = m_ChaseDuration;
        m_AttackTimer = m_AttackCooldown;

        m_GhostInteractionDuration = Random.Range(5, 10);
        m_GhostInteractionTimer = m_GhostInteractionDuration;

        m_GhostAnimation = GetComponent<Animator>();
        m_GhostEmfLocation = GameObject.FindGameObjectWithTag("emf_locate").transform;

        m_EmfActivityDuration = Random.Range(5, 10);

        m_EmfActivityTimer = m_EmfActivityDuration;

        m_PatrolZones = GameObject.FindGameObjectsWithTag("TrapSpawn");
        m_PlayersInLobby = FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (PhotonNetwork.IsMasterClient)
        {
            m_View.RPC("RPC_AssignGhostKit", RpcTarget.All, 0);
            m_TimerCooldownLights = m_TimerCooldownLightsDuration;
            m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }


        // Exfill data for when ghost is killed.
        m_ExfillArea = FindAnyObjectByType<ReadyZone>().gameObject.transform.parent.gameObject;
        m_ExfillArea.SetActive(false);
        m_OnGhostDeath.AddListener(delegate { m_ExfillArea.SetActive(true); });

        StartCoroutine(CheckInteractions());

        PhotonAnimatorView animatiorview = GetComponent<PhotonAnimatorView>();

        for (int i = 0; i < m_GhostAnimation.layerCount; i++)
        {
            animatiorview.SetLayerSynchronized(i, PhotonAnimatorView.SynchronizeType.Discrete);
        }

        for (int i = 0; i < m_GhostAnimation.parameterCount; i++)
        {
            PhotonAnimatorView.ParameterType type = PhotonAnimatorView.ParameterType.Float;

            if (m_GhostAnimation.parameters[i].type == AnimatorControllerParameterType.Float)
            {
                type = PhotonAnimatorView.ParameterType.Float;
            }
            else if (m_GhostAnimation.parameters[i].type == AnimatorControllerParameterType.Bool)
            {
                type = PhotonAnimatorView.ParameterType.Bool;
            }
            else if (m_GhostAnimation.parameters[i].type == AnimatorControllerParameterType.Trigger)
            {
                type = PhotonAnimatorView.ParameterType.Trigger;
            }
            else if (m_GhostAnimation.parameters[i].type == AnimatorControllerParameterType.Int)
            {
                type = PhotonAnimatorView.ParameterType.Int;
            }


            animatiorview.SetParameterSynchronized(m_GhostAnimation.parameters[i].name, type, PhotonAnimatorView.SynchronizeType.Discrete);
        }
    }

    [PunRPC]
    public void RPC_AssignGhostKit(int _index)
    {
        m_GhostKits[_index].m_GhostObject.SetActive(true);
        m_CurrentGhostKitActive = _index;

        // Set Animator up.
        m_GhostAnimation.runtimeAnimatorController = m_GhostKits[_index].m_Controller;
        m_GhostAnimation.avatar = m_GhostKits[_index].m_Avatar;

        // Evidence Objects Assignments
        m_GhostOrbs.SetActive(ContainsEvidence(EvidenceTypes.GhostOrb));
        BloodTrails.SetActive(ContainsEvidence(EvidenceTypes.Bloodtrail));

        if (!m_IsGhostRevealingTrueForm)
            for (int i = 0; i < m_GhostKits[_index].m_GhostsMaterial.Length; i++)
                m_GhostKits[_index].m_GhostsMaterial[i].SetFloat("_AfterlifeForm", 0f); // false
        else
            for (int i = 0; i < m_GhostKits[_index].m_GhostsMaterial.Length; i++)
                m_GhostKits[_index].m_GhostsMaterial[i].SetFloat("_AfterlifeForm", 1f); // True

    }

    public void EnteredAfterlifeRealm()
    {
        m_View.RPC("RPC_ShowGhostTrueForm", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_ShowGhostTrueForm()
    {
        m_IsGhostRevealingTrueForm = true;
        for (int i = 0; i < m_GhostKits[m_CurrentGhostKitActive].m_GhostsMaterial.Length; i++)
            m_GhostKits[m_CurrentGhostKitActive].m_GhostsMaterial[i].SetFloat("_AfterlifeForm", 1f); // True

        m_GhostOrbs.SetActive(false);
        BloodTrails.SetActive(false);
    }

    void Update() // Entire AI Is going to only be controlled on the Hosts side. 
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        CheckPlayerDetection();

        switch (m_GhostsBehaviour)
        {
            case GhostBehaviour.Idle:
                UpdateIdle();
                break;

            case GhostBehaviour.Patrol:
                PatrolArea();
                break;

            case GhostBehaviour.Investigate:
                InvestigateArea();
                break;

            case GhostBehaviour.Chase:
                ChasePlayer();
                break;

            case GhostBehaviour.Search:
                SearchForPlayer();
                break;

            case GhostBehaviour.Attack:


                break;

            case GhostBehaviour.Trap:
                break;
        }

        if (m_GiveEmfActivity)
        {
            m_EmfActivityTimer -= Time.deltaTime;

            if (m_EmfActivityTimer <= 0)
            {
                m_View.RPC("RPC_StopEmf", RpcTarget.All);
            }
        }

        if (m_AttackIsOnCooldown)
        {
            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer <= 0)
            {
                m_AttackTimer = m_AttackCooldown;
                m_AttackIsOnCooldown = false;
                m_GhostsBehaviour = GhostBehaviour.Chase;
                m_MyAgent.isStopped = false;
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

        m_GhostInteractionTimer -= Time.deltaTime;

        if (m_GhostInteractionTimer <= 0)
        {
            GhostActivityEmf();

            m_GhostEmfLocation.transform.position = transform.position;

            m_GhostInteractionDuration = Random.Range(20, 30);
            m_GhostInteractionTimer = m_GhostInteractionDuration;
        }

        m_GhostAnimation.SetFloat("GhostSpeed", m_MyAgent.velocity.magnitude);
    }

    void PatrolArea()
    {
        if (!m_MyAgent.pathPending && m_MyAgent.remainingDistance < 0.5f)
        {
            GoToNextPatrolZone();
        }
    }

    void InvestigateArea()
    {
        if (m_MyAgent.remainingDistance < 1f)
        {
            m_GhostsBehaviour = GhostBehaviour.Search;
            m_SearchTimer = m_SearchDuration;
        }
    }

    void GoToNextPatrolZone()
    {
        m_MyAgent.destination = m_PatrolZones[m_CurrentPatrolZone].transform.position;
        m_CurrentPatrolZone = (m_CurrentPatrolZone + 1) % m_PatrolZones.Length;
    }

    void ChasePlayer()
    {
        m_MyAgent.destination = m_PlayerTarget.position;

        if (Vector3.Distance(transform.position, m_PlayerTarget.position) < m_EyeSightRange * 1.5f)
        {
            m_GhostsBehaviour = GhostBehaviour.Search;
            m_SearchTimer = m_SearchDuration;
            m_lastKnownSighting = m_PlayerTarget.position;
        }
    }

    void SearchForPlayer()
    {
        m_MyAgent.destination = m_lastKnownSighting;

        if (m_MyAgent.remainingDistance < 1f)
        {
            m_SearchTimer -= Time.deltaTime;
            if (m_SearchTimer <= 0)
            {
                m_GhostsBehaviour = GhostBehaviour.Patrol;
                GoToNextPatrolZone();
            }
        }
    }

    void CheckPlayerDetection()
    {
        for (int i = 0; i < m_PlayersInLobby.Length; i++)
        {

            Vector3 direction = m_PlayersInLobby[i].transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, direction);

            if (direction.magnitude <= m_EyeSightRange && angle <= m_GhostFieldofView / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out hit, m_EyeSightRange))
                {
                    if (hit.collider.CompareTag("Player") && !hit.collider.GetComponent<PlayerController>().IsPlayerDead())
                    {
                        m_PlayerTarget = hit.collider.gameObject.transform;
                        m_GhostsBehaviour = GhostBehaviour.Chase;
                    }
                }
            }

            if (m_AttackIsOnCooldown) return;

            if (direction.magnitude <= m_EyeSightRange && angle <= m_GhostFieldofView / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out hit, m_AttackRange))
                {
                    if (hit.collider.CompareTag("Player") && !hit.collider.GetComponent<PlayerController>().IsPlayerDead())
                    {
                        PhotonView playerView = hit.collider.GetComponent<PhotonView>();

                        playerView.RPC("RPC_TakeDamage", playerView.Owner, 0.25f);
                        m_MyAgent.isStopped = true;
                        m_GhostAnimation.SetTrigger("Attack");
                        m_AttackIsOnCooldown = true;
                    }
                }
            }
        }
    }

    void UpdateIdle()
    {
        m_IdleTimer -= Time.deltaTime;

        if (m_IdleTimer <= 0)
        {
            m_IdleDuration = Random.Range(5f, 9f);
            m_IdleTimer = m_IdleDuration;
            m_GhostsBehaviour = GhostBehaviour.Patrol;
        }
    }

    void ComparePlayerPossesionLevels()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.gameObject.GetComponent<NetworkObject>() && ContainsEvidence(EvidenceTypes.FloatingObjects))
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            other.gameObject.GetComponent<Rigidbody>().useGravity = false;
            other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.Impulse);

        }

    }

    /// <summary>
    /// Check if Ghost Profile contains the given evidence.
    /// </summary>
    /// <param name="_evidenceType"></param>
    /// <returns></returns>
    public bool ContainsEvidence(EvidenceTypes _evidenceType)
    {
        if (m_Profile.m_Evidence1 == _evidenceType)
        {
            return true;
        }
        else if (m_Profile.m_Evidence2 == _evidenceType)
        {
            return true;
        }
        else if (m_Profile.m_Evidence3 == _evidenceType)
        {
            return true;
        }
        else if (m_Profile.m_Evidence4 == _evidenceType)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.gameObject.GetComponent<NetworkObject>() && ContainsEvidence(EvidenceTypes.FloatingObjects))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.GetComponent<DoorModule>())
        {
            collision.gameObject.GetComponent<DoorModule>().CycleDoorState();
        }
    }

    /// <summary>
    /// Is The emf active currently
    /// </summary>
    /// <returns></returns>
    public bool IsEmfActivityActive()
    {
        return m_GiveEmfActivity;
    }

    /// <summary>
    /// Get the current level of the EMF
    /// </summary>
    /// <returns></returns>
    public int GetEmfAcitivtyValue()
    {
        return m_EmfLevel;
    }


    void GhostActivityEmf()
    {

        m_EmfLevel = Random.Range(0, 5);
        m_GiveEmfActivity = true;

        m_EmfActivityDuration = Random.Range(5, 10);
        m_EmfActivityTimer = m_EmfActivityDuration;

        m_View.RPC("RPC_EmfLevel", RpcTarget.Others, m_EmfLevel);
    }

    [PunRPC]
    public void RPC_EmfLevel(int _index)
    {
        m_EmfLevel = _index;
        m_GiveEmfActivity = true;
    }

    [PunRPC]
    public void RPC_StopEmf()
    {
        m_EmfActivityTimer = m_EmfActivityDuration;
        m_GiveEmfActivity = false;
        m_EmfLevel = 0;
    }
    [PunRPC]
    public void RPC_GhostDeath()
    {
        gameObject.SetActive(false);
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
