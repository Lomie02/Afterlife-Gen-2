using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum SkinwalkerBehaviour
{
    Follow = 0,
    Hide,
    Wait,
}

public class Skinwalker : MonoBehaviour
{
    SkinwalkerBehaviour m_Behavour;
    NavMeshAgent m_NavMeshBody;
    Transform m_PlayerToStalk;
    Camera m_PlayersCamera;

    float m_SkinWalkerWaitTimer;
    [SerializeField] float m_SkinWalkerWaitDuration = 5;
    [SerializeField] float m_RegularStalkSpeed = 1f;

    [SerializeField] GameObject[] m_Skins;

    Animator m_BodyAnimations;

    void Start()
    {
        m_NavMeshBody = GetComponent<NavMeshAgent>();
        m_SkinWalkerWaitTimer = m_SkinWalkerWaitDuration;

        m_NavMeshBody.stoppingDistance = 2f;
        for (int i = 0; i < m_Skins.Length; i++)
        {
            m_Skins[i].SetActive(false);
        }
    }

    public void AssignTarget(Transform _player, Camera _playersCamera, int _skin)
    {
        m_PlayerToStalk = _player;
        m_PlayersCamera = _playersCamera;

        m_Skins[_skin].SetActive(true);

        m_BodyAnimations = GetComponentInChildren<Animator>();
        gameObject.SetActive(false);
    }

    public void SummonSkinwalker()
    {
        gameObject.SetActive(true);
        m_Behavour = SkinwalkerBehaviour.Follow;
    }

    private void Update()
    {
        switch (m_Behavour)
        {
            case SkinwalkerBehaviour.Follow:
                FollowSkinwalkersTarget();
                m_BodyAnimations.SetBool("Sprinting", false);

                m_NavMeshBody.speed = m_RegularStalkSpeed;
                m_NavMeshBody.isStopped = false;
                break;

            case SkinwalkerBehaviour.Wait:

                m_NavMeshBody.isStopped = true;

                break;
        }

        if (m_NavMeshBody.isStopped)
        {
            m_BodyAnimations.SetFloat("yPos", 0);
        }
        else
        {
            m_BodyAnimations.SetFloat("yPos", m_NavMeshBody.velocity.z);
        }

        if (IsPlayerLookingAtMe())
            m_Behavour = SkinwalkerBehaviour.Wait;
        else
            m_Behavour = SkinwalkerBehaviour.Follow;
    }

    void HideFromPlayersView()
    {
        Vector3 DirToPlayer = transform.position - m_PlayerToStalk.position;
        Vector3 NewPos = transform.position + DirToPlayer;

        m_NavMeshBody.SetDestination(RandomNavSphere(NewPos, 20f, -1));
        m_Behavour = SkinwalkerBehaviour.Hide;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    void FollowSkinwalkersTarget()
    {
        m_NavMeshBody.SetDestination(m_PlayerToStalk.position);
    }

    bool IsPlayerLookingAtMe()
    {
        Vector3 viewPos = m_PlayersCamera.WorldToViewportPoint(transform.position);
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
        {
            return true;
        }
        else
            return false; // Player has not seen me
    }
}