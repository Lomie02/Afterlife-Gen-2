using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    Rigidbody m_Body;
    Transform m_NewPos;

    [SerializeField] PhotonView m_MyView;

    [SerializeField] Animator m_Anim;

    float m_PlayersOverallSpeed = 0;
    [SerializeField] float m_PlayerWalkSpeed = 2;
    [SerializeField] float m_PlayerSprintSpeed = 5;

    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();

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

        }
    }
}
