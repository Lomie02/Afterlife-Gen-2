using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class WorldCameraSetter : MonoBehaviour
{
    Canvas m_WorldCanvas;
    GameObject[] m_CamerasFound;
    private void Start()
    {
        m_WorldCanvas = GetComponent<Canvas>();
        m_CamerasFound = GameObject.FindGameObjectsWithTag("PlayerCamera");

        for (int i = 0; i < m_CamerasFound.Length; i++)
        {
            if (m_CamerasFound[i].GetComponent<PhotonView>().IsMine)
            {
                m_WorldCanvas.worldCamera = m_CamerasFound[i].GetComponent<Camera>();
            }
        }

    }
}
