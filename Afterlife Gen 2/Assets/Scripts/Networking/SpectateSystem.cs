using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpectateSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject[] m_SpectateCameras = new GameObject[4];
    bool m_IsSpectating = false;

    int m_CurrentPlayerSpectating = 0;
    void Start()
    {
        GameObject[] CamerasFound = GameObject.FindGameObjectsWithTag("3rdPersonCamera");
        for (int i = 0; i < CamerasFound.Length; i++)
        {
            m_SpectateCameras[i] = CamerasFound[i];
        }


        for (int i = 0; i < m_SpectateCameras.Length; i++)
        {
            m_SpectateCameras[i].SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        GameObject[] CamerasFound = GameObject.FindGameObjectsWithTag("3rdPersonCamera");
        for (int i = 0; i > m_SpectateCameras.Length; i++)
        {
            if (m_SpectateCameras[i] == null && CamerasFound[i] != null)
            {
                m_SpectateCameras[i] = CamerasFound[i];
            }
        }
    }

    public void SetSpectateMode(bool _state)
    {
        m_IsSpectating = _state;
        UpdateSpectate();
    }

    void UpdateSpectate()
    {
        m_SpectateCameras[m_CurrentPlayerSpectating].SetActive(m_IsSpectating);
        //TODO Make Cameras cycle
    }

}
