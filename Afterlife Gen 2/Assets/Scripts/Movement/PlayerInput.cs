using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInput : MonoBehaviourPunCallbacks
{
    [SerializeField] LayerMask m_ItemLayer;

    NetworkLobby m_Network;
    PhotonView m_MyView;

    GameManager m_GameManager;
    PlayerCamera m_MyCamera;
    PlayerController m_MyController;

    // Specialist Selections
    Button m_PharmacistButton;
    Button m_TrapperButton;
    Button m_ExorcistButton;
    Button m_MechanicButton;

    Button m_ResumeButton;
    Button m_LeaveButton;
    Button m_CopyButton;

    //Cancel Button
    Button m_CancelButton;
    [SerializeField] Camera m_PlayersCamera;
    [SerializeField] GameObject m_SpecialistMenu;
    [SerializeField] GameObject m_PauseMenu;

    NetworkLobby m_NetworkLobby;
    bool m_IsPaused = false;
    RaycastHit m_ItemCast;

    SpecialstAbility m_Ability;
    ReadyZone m_ReadyUp;
    InventoryManager m_Inventory;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_ReadyUp = FindObjectOfType<ReadyZone>();
        m_Inventory = GetComponent<InventoryManager>();

        m_MyCamera = GetComponent<PlayerCamera>();
        m_MyController = GetComponent<PlayerController>();

        m_PharmacistButton = GameObject.Find("PharmacistButton").GetComponent<Button>();
        m_TrapperButton = GameObject.Find("TrapperButton").GetComponent<Button>();

        m_ExorcistButton = GameObject.Find("ExorcistButton").GetComponent<Button>();
        m_MechanicButton = GameObject.Find("MechanicButton").GetComponent<Button>();

        m_CancelButton = GameObject.Find("Cancel").GetComponent<Button>();
        m_ResumeButton = GameObject.Find("Resume").GetComponent<Button>();

        m_LeaveButton = GameObject.Find("Leave Game").GetComponent<Button>();
        m_CopyButton = GameObject.Find("CopyCode").GetComponent<Button>();

        m_SpecialistMenu = GameObject.Find("SpecialistMenu");
        m_PauseMenu = GameObject.Find("PauseMenu");

        m_GameManager = FindObjectOfType<GameManager>();
        m_Ability = GetComponent<SpecialstAbility>();
        m_Network = FindObjectOfType<NetworkLobby>();

        if (m_PauseMenu)
        {
            m_PauseMenu.SetActive(false);
        }

        if (m_SpecialistMenu)
        {
            m_SpecialistMenu.SetActive(false);
        }

        if (m_MyView.IsMine)
        {
            m_ResumeButton.onClick.AddListener(ResumeGame);
            m_LeaveButton.onClick.AddListener(delegate { PhotonNetwork.LeaveRoom(); });
            m_LeaveButton.onClick.AddListener(delegate { m_GameManager.ChangeScene("Main_Menu"); });

            m_PharmacistButton.onClick.AddListener(delegate { SpawnNewPlayer(1); });
            m_TrapperButton.onClick.AddListener(delegate { SpawnNewPlayer(2); });

            m_ExorcistButton.onClick.AddListener(delegate { SpawnNewPlayer(3); });
            m_MechanicButton.onClick.AddListener(delegate { SpawnNewPlayer(4); });

            m_CopyButton.onClick.AddListener(m_NetworkLobby.CopyCode);
            m_CancelButton.onClick.AddListener(CancelSpecialistSelection);
        }
    }

    void Update()
    {
        if (m_MyView.IsMine)
        {
            // Change specialist
            if (Input.GetKeyDown(KeyCode.H) && !m_IsPaused)
            {
                m_SpecialistMenu.SetActive(true);

                m_MyCamera.MouseLockState(false);
                m_MyController.SetMovement(false);
            }

            // Testing only
            if (Input.GetKeyDown(KeyCode.R))
            {
                m_ReadyUp.ReadyUpHost();
            }

            // bring up pause menu
            if (Input.GetKeyDown(KeyCode.Escape) && m_PauseMenu && !m_IsPaused)
            {
                m_PauseMenu.SetActive(true);
                m_IsPaused = true;

                m_MyCamera.MouseLockState(false);
                m_MyController.SetMovement(false);
            }

            // Use ultimate
            if (Input.GetKeyDown(KeyCode.Q))
            {
                m_Ability.UseAbility();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_ItemCast, 5f))
                {
                    if (m_ItemCast.collider.GetComponent<NetworkObject>() != null)
                    {
                        m_ItemCast.collider.GetComponent<NetworkObject>().CyclePowerStage();
                    }
                }
            }

            float previousDelta = Input.mouseScrollDelta.y * 0.1f;

            if (Input.mouseScrollDelta.y != previousDelta)
            {
                m_Inventory.CycleInvetory();
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) && m_Inventory.IsCurrentSlotTaken())
            {
                m_Inventory.CycleCurrentItemsPower();
            }

            if (Input.GetKeyDown(KeyCode.G) && m_Inventory.IsCurrentSlotTaken())
            {
                m_Inventory.DropItem();
            }

            // Item Pick up/ Interactions
            if (Input.GetKeyDown(KeyCode.E))
            {
                CheckForItem();
            }
        }
    }

    void CheckForItem()
    {
        if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_ItemCast, 5f))
        {
            if (m_ItemCast.collider.GetComponent<NetworkObject>() != null)
            {
                if (!m_Inventory.IsCurrentSlotTaken())
                {
                    m_Inventory.AssignItem(m_ItemCast.collider.GetComponent<NetworkObject>());
                }
            }
            if (m_ItemCast.collider.tag == "ReadyMonitor")
            {
                m_MyCamera.MouseLockState(false);
                m_MyController.SetMovement(false);
            }
        }
    }


    void ResumeGame()
    {
        m_PauseMenu.SetActive(false);
        m_IsPaused = false;

        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }
    void CancelSpecialistSelection()
    {
        m_SpecialistMenu.SetActive(false);
        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }

    void SpawnNewPlayer(int _index)
    {
        m_PauseMenu.SetActive(true);

        m_Network.SpawnPlayerByID(_index);
        PhotonNetwork.Destroy(m_MyView);

        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);
    }
}
