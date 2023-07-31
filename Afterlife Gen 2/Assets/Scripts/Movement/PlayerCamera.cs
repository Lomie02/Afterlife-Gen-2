using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Camera m_PlayersCamera;
    [SerializeField] float m_Sens = 100;

    [SerializeField] Transform m_Body;

    PhotonView m_MyView;
    float m_Xview;
    float m_Yview;

    public void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        if (!m_MyView.IsMine)
        {
            m_PlayersCamera.gameObject.SetActive(false);
        }

        MouseLockState(true);
    }

    void Update()
    {
        if (m_MyView.IsMine)
        {
            float xPos = Input.GetAxis("Mouse X") * m_Sens * Time.deltaTime;
            float yPos = Input.GetAxis("Mouse Y") * m_Sens * Time.deltaTime;

            m_Xview += xPos;
            m_Yview -= yPos;
            m_Body.Rotate(Vector3.up, xPos);

            m_Yview = Mathf.Clamp(m_Yview, -30, 90);

            m_PlayersCamera.transform.localEulerAngles = new Vector3(m_Yview, 0, 0);
        }
    }

    public void MouseLockState(bool _state)
    {
        if (!m_MyView.IsMine)
            return;

        if (_state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
