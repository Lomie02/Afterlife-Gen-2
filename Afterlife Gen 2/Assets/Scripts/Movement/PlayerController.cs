using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

enum PlayerStance
{
    Stand = 0,
    Walk,
    Run,
    Crouch,
    Dive,
    Revive,
}

public class PlayerController : MonoBehaviour
{
    Rigidbody m_Body;
    Transform m_NewPos;

    PlayerStance m_Stance = PlayerStance.Stand;
    [SerializeField] PhotonView m_MyView;

    [SerializeField] Animator m_Anim;

    float m_PlayersOverallSpeed = 0;
    [SerializeField] float m_PlayerWalkSpeed = 2;
    [SerializeField] float m_PlayerSprintSpeed = 5;
    [SerializeField] float m_PlayerTacticalSprintSpeed = 8;

    CapsuleCollider m_PlayerCollider;

    Vector3 m_DefaultCollider = new Vector3(0, 0.8321516f, 0);
    float m_DefaultHeight = 1.712385f;

    Vector3 m_DiveCollider = new Vector3(0, 0.2867912f, 0);
    float m_DiveHeight = 0.621664f;

    bool m_IsDiving = false;

    float m_DiveWaitTimer = 0;
    float m_DiveWaitDuration = 1;

    bool m_CanMove = true;
    bool m_IsSprinting = false;
    bool m_IsTacticalSprinting = false;
    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();

        m_PlayerCollider = GetComponent<CapsuleCollider>();
        m_PlayersOverallSpeed = m_PlayerWalkSpeed;
    }

    void Update()
    {
        if (m_MyView.IsMine && m_CanMove)
        {
            float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

            Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

            m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayersOverallSpeed * Time.deltaTime);

            m_Anim.SetFloat("xPos", xPos);
            m_Anim.SetFloat("yPos", yPos);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_IsTacticalSprinting = true;
                }

                if (m_IsTacticalSprinting)
                    m_PlayersOverallSpeed = m_PlayerSprintSpeed + 1;
                else
                    m_PlayersOverallSpeed = m_PlayerSprintSpeed;

                m_IsSprinting = true;
            }
            else
            {
                m_PlayersOverallSpeed = m_PlayerWalkSpeed;
                m_IsTacticalSprinting = false;
                m_IsSprinting = false;
            }


            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
                {
                    Dive();
                }
            }

            if (m_IsDiving)
            {
                m_DiveWaitTimer += Time.deltaTime;

                if (m_DiveWaitTimer >= m_DiveWaitDuration)
                {
                    m_DiveWaitTimer = 0;

                    if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
                    {
                        m_PlayerCollider.center = m_DefaultCollider;
                        m_PlayerCollider.height = m_DefaultHeight;
                        m_IsDiving = false;
                    }
                }
            }

            m_Anim.SetBool("Sprinting", m_IsSprinting);
        }
    }

    public bool IsTacticalSprinting()
    {
        return m_IsTacticalSprinting;
    }

    void Dive()
    {
        if (m_MyView.IsMine && m_CanMove)
        {
            m_IsDiving = true;
            m_PlayerCollider.center = m_DiveCollider;
            m_PlayerCollider.height = m_DiveHeight;

            Vector3 DivePos = new Vector3(0, 0.5f, 0.5f);
            m_Body.AddForce(DivePos * 7, ForceMode.Impulse);
        }
    }

    public void SetMovement(bool _state)
    {
        m_CanMove = _state;
    }
}
