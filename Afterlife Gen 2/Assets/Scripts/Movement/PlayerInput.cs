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

    // Specialist Selections
    Button m_PharmacistButton;
    Button m_TrapperButton;
    Button m_ExorcistButton;
    Button m_MechanicButton;

    //Cancel Button
    Button m_CancelButton;

    [SerializeField] GameObject m_SpecialistMenu;
    void Start()
    {
        m_Network = FindObjectOfType<NetworkLobby>();
        m_MyView = GetComponent<PhotonView>();

        m_MyCamera = GetComponent<PlayerCamera>();
        m_MyController = GetComponent<PlayerController>();

        m_PharmacistButton = GameObject.Find("PharmacistButton").GetComponent<Button>();
        m_TrapperButton = GameObject.Find("TrapperButton").GetComponent<Button>();

        m_ExorcistButton = GameObject.Find("ExorcistButton").GetComponent<Button>();
        m_MechanicButton = GameObject.Find("MechanicButton").GetComponent<Button>();

        m_CancelButton = GameObject.Find("Cancel").GetComponent<Button>();

        m_SpecialistMenu = GameObject.Find("SpecialistMenu");

        if (m_SpecialistMenu)
        {
            m_SpecialistMenu.SetActive(false);
        }

        if (m_MyView.IsMine)
        {
            m_PharmacistButton.onClick.AddListener(delegate { SpawnNewPlayer(1); });
            m_TrapperButton.onClick.AddListener(delegate { SpawnNewPlayer(2); });

            m_ExorcistButton.onClick.AddListener(delegate { SpawnNewPlayer(3); });
            m_MechanicButton.onClick.AddListener(delegate { SpawnNewPlayer(4); });

            m_CancelButton.onClick.AddListener(CancelSpecialistSelection);
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
    void CancelSpecialistSelection()
    {
        m_SpecialistMenu.SetActive(false);
        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }

    void SpawnNewPlayer(int _index)
    {
        m_Network.SpawnPlayerByID(_index);
        PhotonNetwork.Destroy(m_MyView);

        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }
}
