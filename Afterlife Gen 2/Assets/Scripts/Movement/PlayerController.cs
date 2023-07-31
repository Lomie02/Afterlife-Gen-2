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
    [SerializeField] float m_PlayerWalkSpeed = 5;

    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (m_MyView.IsMine)
        {
            float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

            Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

            m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayerWalkSpeed * Time.deltaTime);

            m_Anim.SetFloat("xPos", xPos);
            m_Anim.SetFloat("yPos", yPos);
        }
    }
}
