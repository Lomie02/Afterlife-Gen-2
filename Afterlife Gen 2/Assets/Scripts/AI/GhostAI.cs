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
    [SerializeField] PhotonView m_View;
    GhostBehaviour m_GhostsBehaviour = GhostBehaviour.Seeking;

    NavMeshAgent m_MyAgent;

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
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    void HuntPlayer()
    {

    }
}
