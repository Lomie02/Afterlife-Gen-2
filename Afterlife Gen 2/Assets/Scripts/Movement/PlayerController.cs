using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody m_Body;
    Transform m_NewPos;

    [SerializeField] float m_PlayerWalkSpeed = 5;
    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;    
        float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

        Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

        m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayerWalkSpeed * Time.deltaTime);
    }
}
