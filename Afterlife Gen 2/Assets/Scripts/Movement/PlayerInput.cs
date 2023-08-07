using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInput : MonoBehaviourPunCallbacks
{
    NetworkLobby m_Network;
    PhotonView m_MyView;

    PlayerCamera m_MyCamera;
    PlayerController m_MyController;

    [SerializeField] Button m_Test;
    [SerializeField] Button m_Test1;

    [SerializeField] GameObject m_SpecialistMenu;
    void Start()
    {
        m_Network = FindObjectOfType<NetworkLobby>();
        m_MyView = GetComponent<PhotonView>();

        m_MyCamera = GetComponent<PlayerCamera>();
        m_MyController = GetComponent<PlayerController>();

        m_Test = GameObject.Find("PharmacistButton").GetComponent<Button>();
        m_Test1 = GameObject.Find("TrapperButton").GetComponent<Button>();
        m_SpecialistMenu = GameObject.Find("SpecialistMenu");

        if (m_SpecialistMenu)
        {
            m_SpecialistMenu.SetActive(false);
        }

        if (m_MyView.IsMine)
        {
            m_Test.onClick.AddListener(delegate { SpawnNewPlayer(1); });
            m_Test1.onClick.AddListener(delegate { SpawnNewPlayer(2); });
        }
    }

    void Update()
    {
        if (m_MyView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                m_SpecialistMenu.SetActive(true);

                m_MyCamera.MouseLockState(false);
                m_MyController.SetMovement(false);
            }

        }
    }

    void SpawnNewPlayer(int _index)
    {
        m_Network.SpawnPlayerByID(_index);
        PhotonNetwork.Destroy(m_MyView);

        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }
}
