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
    GhostBehaviour m_GhostsBehaviour = GhostBehaviour.Seeking;

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

    [SerializeField] Transform m_GhostEmfLocation;
    bool m_GiveEmfActivity;

    int m_EmfLevel;
    float m_EmfActivityTimer;
    float m_EmfActivityDuration;

    float m_GhostInteractionTimer;
    float m_GhostInteractionDuration;
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

    void RandomGhostEvent()
    {

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

            if (m_PlayersGhostCanSee.Count != 0)
                m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
            else
                m_MyAgent.SetDestination(m_PlayersGhostCanSee[Random.Range(0, m_PlayersGhostCanSee.Count)].transform.position);
        }
    }

    GameObject GetPlayerWithHighestPossesion()
    {
        if (m_PlayersGhostCanSee.Count == 0) return null;

        return gameObject;
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

    void ComparePlayerPossesionLevels()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.tag == "Player")
            m_PlayersGhostCanSee.Add(other.gameObject);

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
        else if(m_Profile.m_Evidence4 == _evidenceType)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.tag == "Player")
            m_PlayersGhostCanSee.Remove(other.gameObject);

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
