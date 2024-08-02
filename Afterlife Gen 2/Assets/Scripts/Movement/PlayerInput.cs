using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Animations.Rigging;
public class PlayerInput : MonoBehaviourPunCallbacks
{
    [SerializeField] LayerMask m_ItemLayer;

    NetworkLobby m_Network;
    [SerializeField] PhotonView m_MyView;

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
    [SerializeField] Button m_ReadyHost;
    Button m_CancelGameStart;

    //Cancel Button
    Button m_CancelButton;
    [SerializeField] Camera m_PlayersCamera;
    [SerializeField] GameObject m_SpecialistMenu;
    [SerializeField] GameObject m_PauseMenu;
    [SerializeField] GameObject m_HostGameSettings;

    NetworkLobby m_NetworkLobby;
    bool m_IsPaused = false;
    RaycastHit m_ItemCast;

    SpecialstAbility m_Ability;
    public ReadyZone m_ReadyUp;
    InventoryManager m_Inventory;

    [SerializeField] Image m_ScoreBoard;
    [SerializeField] NetworkObject m_PlayersFlashLight;
    [SerializeField] Animator m_PlayersAnimations;

    bool m_ToggleFlashLight = false;
    float m_FlashLightLerp = 0;

    ReadyZone m_ReadyDoorsHost;

    [Header("Use Items")]
    [SerializeField] Image m_UseImage;
    [SerializeField] Text m_UseText;
    ChainIKConstraint m_LightAImConstrait;

    bool m_isLighterOpen = false;
    [SerializeField] Animator m_LighterAnimator;

    // Revive Interface 
    [Header("Revive Interface")]
    [SerializeField] Slider m_ReviveProgressBar;
    [SerializeField] float m_ReviveDuration = 8f;
    [SerializeField] float m_ReviveTimer = 0f;
    void Start()
    {
        //SearchForElements();

        m_ReviveTimer = m_ReviveDuration;

        m_MyView = GetComponent<PhotonView>();
        m_ReadyUp = FindFirstObjectByType<ReadyZone>();
        m_Inventory = GetComponent<InventoryManager>();

        m_MyCamera = GetComponent<PlayerCamera>();
        m_MyController = GetComponent<PlayerController>();
        m_GameManager = FindFirstObjectByType<GameManager>();
        m_Ability = GetComponent<SpecialstAbility>();

        m_Network = FindFirstObjectByType<NetworkLobby>();
        m_HostGameSettings = GameObject.Find("MapSelection");
        m_ReadyHost = GameObject.Find("ReadyUpHost").GetComponent<Button>();

        m_PlayersFlashLight.gameObject.SetActive(false);
        m_HostGameSettings.SetActive(false);

        m_LightAImConstrait = GetComponentInChildren<ChainIKConstraint>();
        if(m_LightAImConstrait) m_LightAImConstrait.weight = 0;

        m_ResumeButton = GameObject.Find("Resume").GetComponent<Button>();
        m_LeaveButton = GameObject.Find("Leave Game").GetComponent<Button>();

        m_ResumeButton.onClick.AddListener(ResumeGame);
        m_LeaveButton.onClick.AddListener(DisconnectFromLobbyDirect);

        m_PauseMenu.SetActive(false);
        
        
        m_SpecialistMenu.SetActive(false);
        if (m_ReadyHost)
        {
            m_ReadyHost.onClick.AddListener(m_ReadyUp.ReadyUpHost);
            m_ReadyHost.onClick.AddListener(delegate { m_HostGameSettings.SetActive(false); });
            m_ReadyHost.onClick.AddListener(delegate { m_MyController.SetMovement(true); });
            m_ReadyHost.onClick.AddListener(delegate { m_MyCamera.MouseLockState(true); });
        }


    }

    public void DisconnectFromLobbyDirect()
    {
        PhotonNetwork.LeaveRoom();
        m_GameManager.ChangeScene("Main_Menu");
    }

    public void SearchForElements()
    {
        m_MyView = GetComponent<PhotonView>();
        m_ReadyUp = FindObjectOfType<ReadyZone>();
        m_Inventory = GetComponent<InventoryManager>();

        m_MyCamera = GetComponent<PlayerCamera>();
        m_MyController = GetComponent<PlayerController>();

        m_ReadyDoorsHost = GameObject.Find("ReadyDoor").GetComponent<ReadyZone>();
        m_PharmacistButton = GameObject.Find("PharmacistButton").GetComponent<Button>();
        m_TrapperButton = GameObject.Find("TrapperButton").GetComponent<Button>();

        m_ExorcistButton = GameObject.Find("ExorcistButton").GetComponent<Button>();
        m_MechanicButton = GameObject.Find("MechanicButton").GetComponent<Button>();
        m_CancelGameStart = GameObject.Find("CancelGame").GetComponent<Button>();

        m_CancelButton = GameObject.Find("Cancel").GetComponent<Button>();
        m_ResumeButton = GameObject.Find("Resume").GetComponent<Button>();

        m_LeaveButton = GameObject.Find("LeaveGame").GetComponent<Button>();
        m_CopyButton = GameObject.Find("CopyCode").GetComponent<Button>();

        m_SpecialistMenu = GameObject.Find("SpecialistMenu");
        m_PauseMenu = GameObject.Find("PauseMenu");
        m_HostGameSettings = GameObject.Find("MapSelection");

        m_GameManager = FindObjectOfType<GameManager>();
        m_Ability = GetComponent<SpecialstAbility>();
        m_Network = FindObjectOfType<NetworkLobby>();

        if (m_HostGameSettings)
            m_HostGameSettings.SetActive(false);

        if (m_PauseMenu)
            m_PauseMenu.SetActive(false);

        if (m_SpecialistMenu)
            m_SpecialistMenu.SetActive(false);

        m_LeaveButton.onClick.AddListener(delegate { PhotonNetwork.LeaveRoom(); });

        m_CancelGameStart.onClick.AddListener(delegate { m_MyController.SetMovement(true); });
        m_CancelGameStart.onClick.AddListener(delegate { m_MyCamera.MouseLockState(true); });
        m_CancelGameStart.onClick.AddListener(delegate { m_HostGameSettings.SetActive(false); });

        m_ReadyHost.onClick.AddListener(delegate { m_MyController.SetMovement(true); });
        m_ReadyHost.onClick.AddListener(delegate { m_MyCamera.MouseLockState(true); });
        m_ReadyHost.onClick.AddListener(delegate { m_HostGameSettings.SetActive(false); });
        m_ReadyHost.onClick.AddListener(m_ReadyDoorsHost.ReadyUpHost);

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
        m_PlayersFlashLight.gameObject.SetActive(false);
    }

    void LerpFlashLight(float _index)
    {
        m_FlashLightLerp = Mathf.Lerp(m_FlashLightLerp, _index, 5 * Time.deltaTime);

        m_PlayersAnimations.SetLayerWeight(1, m_FlashLightLerp);
        m_LightAImConstrait.weight = m_FlashLightLerp;
    }
    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        m_GameManager.ChangeScene("Main_Menu");
    }

    bool IsLookingAtReviveTarget()
    {
        RaycastHit m_Cast;
        if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_Cast, 2f))
        {
            if (m_Cast.collider.GetComponent<PlayerController>() != null)
                if (m_Cast.collider.GetComponent<PlayerController>().IsPlayerDowned())
                    return true;
        }
        return false;
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

            if (Input.GetKey(KeyCode.E) && IsLookingAtReviveTarget()) // revive player
            {
                m_ReviveTimer -= Time.deltaTime;
                m_ReviveProgressBar.value += Time.deltaTime;
                m_ReviveProgressBar.gameObject.SetActive(true);

                if (m_ReviveTimer <= 0)
                {
                    RaycastHit m_Cast;
                    if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_Cast, 2f))
                    {
                        if (m_Cast.collider.GetComponent<PlayerController>())
                            m_Cast.collider.GetComponent<PlayerController>().GetPlayersPhotonView().RPC("RPC_RevivePlayer", RpcTarget.All);

                    }
                }
            }
            else
            {
                m_ReviveTimer = m_ReviveDuration;
                m_ReviveProgressBar.value = 0;
                m_ReviveProgressBar.gameObject.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.T) && m_PlayersFlashLight.gameObject.activeSelf)
            {
                if (m_isLighterOpen)
                {
                    m_PlayersFlashLight.TurnOff();
                    m_isLighterOpen = false;

                }
                else
                {
                    m_PlayersFlashLight.TurnOn();
                    m_isLighterOpen = true;
                }

                m_LighterAnimator.SetBool("IsOpened", m_isLighterOpen);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (m_PlayersFlashLight.gameObject.activeSelf)
                {

                    m_PlayersFlashLight.RPC_SetObjectState(false);
                    m_PlayersFlashLight.TurnOff();
                    m_isLighterOpen = false;
                }
                else
                {
                    m_PlayersFlashLight.RPC_SetObjectState(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                m_MyController.TakeDamage(20);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_Inventory.DropItemsOnPerson();
            }

            if (Input.GetKey(KeyCode.Tab))
                m_ScoreBoard.gameObject.SetActive(true);
            else
                m_ScoreBoard.gameObject.SetActive(false);

            m_MyView.RPC("RPC_UpdatePlayerFlashlight", RpcTarget.All); //Update Players flashlight lerp over network too.

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
                if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_ItemCast, 3f))
                {
                    if (m_ItemCast.collider.GetComponent<NetworkObject>() != null)
                    {
                        m_ItemCast.collider.GetComponent<NetworkObject>().CyclePowerStage();
                    }
                }
            }

            float previousDelta = Input.mouseScrollDelta.y * 0.06f;

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

            CheckHoverItem();
        }
    }

    [PunRPC]
    public void RPC_UpdatePlayerFlashlight()
    {
        if (m_PlayersFlashLight.gameObject.activeSelf)
        {
            if (m_MyController.IsSprinting())
            {
                LerpFlashLight(0.5f);
            }
            else
            {

                LerpFlashLight(1);
            }
        }
        else
        {
            LerpFlashLight(0);
        }
    }

    void CheckHoverItem()
    {
        if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_ItemCast, 2f))
        {
            if (m_ItemCast.collider.GetComponent<NetworkObject>() != null)
            {
                m_UseImage.gameObject.SetActive(true);
                m_UseText.text = "Press [E] To Pick Up " + m_ItemCast.collider.GetComponent<NetworkObject>().GetItemsName();
                return;
            }
            else if (m_ItemCast.collider.tag == "ReadyMonitor" || m_ItemCast.collider.tag == "Door")
            {
                m_UseImage.gameObject.SetActive(true);
                m_UseText.text = "Press [E] To Interact.";
                return;
            }
            else if (m_ItemCast.collider.name == "PartPlace" || m_ItemCast.collider.name == "PartPlace_Bat")
            {
                m_UseImage.gameObject.SetActive(true);
                m_UseText.text = "Press [E] To Place Part.";
                return;
            }
            else if (m_ItemCast.collider.name == "Switch_Board")
            {
                m_UseImage.gameObject.SetActive(true);
                m_UseText.text = "Press [E] To Toggle Power.";
                return;
            }
            else if (m_ItemCast.collider.name == "Monitor")
            {
                m_UseImage.gameObject.SetActive(true);
                m_UseText.text = "Press [E] To Activate Trap.";
                return;
            }
            else if (m_ItemCast.collider.GetComponent<PlayerController>() != null)
            {
                if (m_ItemCast.collider.GetComponent<PlayerController>().IsPlayerDowned())
                {
                    m_UseImage.gameObject.SetActive(true);
                    m_UseText.text = "Hold [E] To Revive.";
                    return;
                }
            }
            else
            {
                m_UseImage.gameObject.SetActive(false);
            }
        }
    }

    void CheckForItem()
    {
        if (Physics.Raycast(m_PlayersCamera.transform.position, m_PlayersCamera.transform.forward, out m_ItemCast, 2f))
        {
            if (m_ItemCast.collider.GetComponent<NetworkObject>() != null)
            {
                if (!m_Inventory.IsCurrentSlotTaken() || m_ItemCast.collider.GetComponent<NetworkObject>().GetItemID() == ItemID.SantiyPill)
                {
                    m_Inventory.AssignItem(m_ItemCast.collider.GetComponent<NetworkObject>());
                    return;
                }
            }
            else if (m_ItemCast.collider.name == "PartPlace" || m_ItemCast.collider.name == "PartPlace_Bat")
            {
                if (m_Inventory.IsCurrentSlotTaken())
                {
                    if (m_ItemCast.collider.GetComponentInParent<GhostTrap>().CollectedPart(m_Inventory.GetCurrentItemsId()))
                    {
                        m_Inventory.DestroyCurrentItem();
                        return;
                    }
                }
            }
            else if (m_ItemCast.collider.GetComponent<Destructable_Object>() != null)
            {
                m_ItemCast.collider.GetComponent<Destructable_Object>().DestroyObject();
                return;
            }

            else if (m_ItemCast.collider.name == "Monitor")
            {
                m_ItemCast.collider.GetComponentInParent<GhostTrap>().StartTrapSequence();
                return;
            }
            else if (m_ItemCast.collider.GetComponent<PowerManager>() != null)
            {
                m_ItemCast.collider.GetComponent<PowerManager>().CyclePower();
                return;
            }
            else if (m_ItemCast.collider.tag == "ReadyMonitor" && PhotonNetwork.IsMasterClient)
            {
                m_MyCamera.MouseLockState(false);
                m_MyController.SetMovement(false);
                m_HostGameSettings.SetActive(true);
                return;
            }
            else if (m_ItemCast.collider.tag == "Door")
            {
                m_ItemCast.collider.GetComponentInParent<DoorModule>().CycleDoorState();
                return;
            }
        }
    }

    public void ResumeGame()
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

    public void SpawnNewPlayer(int _index)
    {
        m_PauseMenu.SetActive(true);

        m_MyCamera.MouseLockState(true);
        m_MyController.SetMovement(true);

        PhotonNetwork.Destroy(m_MyView);
        m_Network.SpawnPlayerByID(_index);

    }
}
