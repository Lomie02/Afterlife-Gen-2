using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpectateSystem : MonoBehaviourPunCallbacks
{
    int m_CurrentPlayerSpectating = 0;
    int m_PrevousCPlayerCamera = 0;

    //=============================== New system test stuff
    public List<Camera> m_CameraList;

    public void SetSpectateMode(bool _state, bool _IsSolo)
    {
        if (_IsSolo)
        {
            m_CurrentPlayerSpectating = 0;
        }
        UpdateSpectate();
    }

    public void CyclePlayer() // Cycle through the specating Screens
    {
        if (m_CameraList.Count > 1)
        {
            m_PrevousCPlayerCamera = m_CurrentPlayerSpectating;
            m_CurrentPlayerSpectating++;

            if (m_CurrentPlayerSpectating >= m_CameraList.Count)
            {
                m_CurrentPlayerSpectating = 0;
            }

            UpdateSpectate();
        }
    }

    void UpdateSpectate()
    {
        m_CameraList[m_PrevousCPlayerCamera].gameObject.SetActive(false);
        m_CameraList[m_CurrentPlayerSpectating].gameObject.SetActive(true);
    }

    public void SubmitCamera(Camera _cameraObject) // Submit cameras to the Specator System
    {
        m_CameraList.Add(_cameraObject);
    }

}
