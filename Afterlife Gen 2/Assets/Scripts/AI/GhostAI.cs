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

public class GhostAI : MonoBehaviour
{
    [SerializeField] GhostProfile m_Profile;

    [SerializeField] PhotonView m_View;
    GhostBehaviour m_GhostsBehaviour = GhostBehaviour.Seeking;

    NavMeshAgent m_MyAgent;

    [SerializeField] GhostManager m_GhostManager;
    [SerializeField] float m_RoamingDistance = 50;

    private void Start()
    {
        m_MyAgent = GetComponent<NavMeshAgent>();
        if (PhotonNetwork.IsMasterClient)
        {
            m_MyAgent.SetDestination(RandomNavSphere(transform.position, m_RoamingDistance, -1));
        }
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

        if(Physics.Raycast(transform.position, transform.forward, out m_CastInfo, 4))
        {

            Debug.Log(m_CastInfo.collider.name);

            if (m_CastInfo.collider.GetComponent<Destructable_Object>())
                m_CastInfo.collider.GetComponent<Destructable_Object>().DestroyObject();
        }
    }

    void CheckInteractions()
    {
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
