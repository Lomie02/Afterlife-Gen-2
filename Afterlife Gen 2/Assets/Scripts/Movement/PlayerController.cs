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

    CapsuleCollider m_PlayerCollider;

    Vector3 m_DefaultCollider = new Vector3(0, 0.8321516f, 0);
    float m_DefaultHeight = 1.712385f;

    Vector3 m_DiveCollider = new Vector3(0, 0.2867912f, 0);
    float m_DiveHeight = 0.621664f;

    bool m_IsDiving = false;

    float m_DiveWaitTimer = 0;
    float m_DiveWaitDuration = 1;
    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();

        m_PlayerCollider = GetComponent<CapsuleCollider>();
        m_PlayersOverallSpeed = m_PlayerWalkSpeed;
    }

    void Update()
    {
        if (m_MyView.IsMine)
        {
            float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

            Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

            m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayersOverallSpeed * Time.deltaTime);

            m_Anim.SetFloat("xPos", xPos);
            m_Anim.SetFloat("yPos", yPos);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_PlayersOverallSpeed = m_PlayerSprintSpeed;
            }
            else
            {
                m_PlayersOverallSpeed = m_PlayerWalkSpeed;
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
                        Debug.Log("Hit Floor");
                    }
                }
            }
        }
    }

    void Dive()
    {
        if (m_MyView.IsMine)
        {
            m_IsDiving = true;
            m_PlayerCollider.center = m_DiveCollider;
            m_PlayerCollider.height = m_DiveHeight;

            Vector3 DivePos = new Vector3(0, 0.5f, 0.5f);
            m_Body.AddForce(DivePos * 7, ForceMode.Impulse);
        }
    }
}
