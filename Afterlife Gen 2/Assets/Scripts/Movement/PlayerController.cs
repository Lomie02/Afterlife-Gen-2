using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;

enum MovementType
{
    Four_Directional = 0,
    Omni_Directional,
}
enum PlayerStance //TODO 
{
    Stand = 0,
    Walk,
    Run,
    Crouch,
    Dive,
    Downed,
}

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] MovementType m_MovementMode = MovementType.Four_Directional;

    [Space]

    [SerializeField] Volume m_PostProcessing;
    ColorAdjustments m_Colour;
    public Rigidbody m_Body;

    [SerializeField] float m_PlayerHealth = 1;
    [SerializeField] float m_PossesionMeter = 0;

    [SerializeField] Image m_HealthBar;
    [SerializeField] Image m_PossessionBar;

    [SerializeField] PhotonView m_MyView;
    [SerializeField] Animator m_BodyAnimations;

    float m_PlayersOverallSpeed = 0;
    [SerializeField] float m_PlayerWalkSpeed = 2;
    [SerializeField] float m_PlayerSprintSpeed = 5;

    bool m_IsInTrapStance;
    public CapsuleCollider m_PlayerCollider;

    Vector3 m_DefaultCollider = new Vector3(0, 0.8321516f, 0);
    float m_DefaultHeight = 1.712385f;

    Vector3 m_DiveCollider = new Vector3(0, 0.2867912f, 0);
    float m_DiveHeight = 0.621664f;

    bool m_CanMove = true;
    bool m_IsSprinting = false;
    bool m_IsTacticalSprinting = false;

    GhostAI m_Ghost;

    float m_AnimationLerpSpeed = 10;
    float m_AnimXPos;
    float m_AnimYPos;

    [SerializeField] float m_CrouchLerpSpeed = 2;
    float m_CrouchLerpAmount;
    bool m_IsCrouched = false;

    // Sliding Mechanics
    bool m_IsSliding;
    float m_SlideTimer = 0;
    [SerializeField] float m_SlideDuration = 0;
    [SerializeField] float m_SlidePower = 10;

    [Header("Downed Stance")]
    bool m_IsDowned = false;
    float m_BleedoutTimer;
    float m_BleedoutDuration = 20;


    [SerializeField] Rigidbody[] m_RagdollBodys;
    [SerializeField] Collider[] m_RagdollColliders;
    bool m_isDead = false;

    [SerializeField] Camera m_PlayersCamera;
    [SerializeField] Camera m_SpectateCamera;
    SpectateSystem m_SpectateSystem;

    // Stamina
    [SerializeField] float m_StaminaAmount = 100;
    [SerializeField] float m_StaminaUsageRate = 10;
    [SerializeField] Slider m_StaminaBar;
    [SerializeField] int m_SkinWalkerModelToUse = 0;
    bool m_isRecoveryingStamina = false;

    Skinwalker m_SkinWalkerDemon;
    SpecialstAbility m_SpecialistAbility;

    // Downed Flare
    [SerializeField] GameObject m_DownedFlareObject;
    GameManager m_GameManager;
    int m_PlayersDeadInGame = 0;

    [Header("View Models")]
    [SerializeField] Renderer m_BodyMaterial;
    [SerializeField] GameObject m_FirstPersonViewModel;
    [Space]
    [SerializeField] GameObject m_FirstPersonMaskedItems;
    [SerializeField] RawImage m_FIrstPersonRenderTexture;

    [Header("Interface Related")]
    [SerializeField] GameObject m_MainHudObject;
    [SerializeField] GameObject m_SpectateObject;
    [Space]
    [SerializeField] GameObject m_BleedoutObject;
    [SerializeField] Text m_BleedoutText;

    [Space]

    [SerializeField] GameObject m_ObjectiveItem;
    [SerializeField] Text m_ObjectiveTitle;
    [SerializeField] Text m_ObjectiveText;

    [Header("Afterlife Realm")]
    [SerializeField] GameObject m_AfterlifeFadeIn;
    [SerializeField] LayerMask m_AfterlifeRealmMask;

    bool m_ReplensishHealth = false;

    ObjectiveManager m_ObjectiveManager;

    PlayerInput m_PlayerInput;
    InventoryManager m_InventoryManager;
    bool m_IsEmoting = false;

    public AnimationEventWatcher m_ThirdPersonCameraWatcher;
    [Header("Sound Effects")]
    [SerializeField] AudioSource m_SoundEffects;
    [SerializeField] AudioClip m_DoorBash;

    [Header("Foot Steps")]
    public AudioSource m_FootStepSounds;
    [Space]
    [SerializeField] AudioClip[] m_FootStepWood;
    [SerializeField] AudioClip[] m_FootStepConcrete;
    [SerializeField] AudioClip[] m_FootStepDirt;
    [SerializeField] AudioClip[] m_FootStepCarpet;

    bool m_SpinToGhost = false;
    float m_PlayerLerpSpeedFinal = 0;

    bool m_DisableMouseUpdates = false;

    Vector3 m_GhostDirection;
    private void Awake()
    {
        GetComponentInChildren<RigBuilder>().Build();
    }
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        m_SpectateSystem = FindAnyObjectByType<SpectateSystem>();
        m_SpecialistAbility = GetComponent<SpecialstAbility>();
        m_PlayerInput = GetComponent<PlayerInput>();

        m_InventoryManager = GetComponent<InventoryManager>();
        m_DefaultCollider = m_PlayerCollider.center;
        m_DefaultHeight = m_PlayerCollider.height;

        m_GameManager = FindAnyObjectByType<GameManager>();
        m_DownedFlareObject.SetActive(false);

        if (!m_MyView.IsMine) // Submit Camera only if its not mine
        {
            m_SpectateSystem.SubmitCamera(m_SpectateCamera);
            m_SpectateCamera.gameObject.SetActive(false);
            m_PlayersCamera.gameObject.SetActive(false);
        }
        else if (m_MyView.IsMine && PhotonNetwork.PlayerListOthers.Length == 0) // Playing Solo
        {

            m_SpectateSystem.SubmitCamera(m_SpectateCamera);
            m_SpectateCamera.gameObject.SetActive(false);
        }
        else
            m_SpectateCamera.gameObject.SetActive(false);

        if (m_MyView.IsMine)
        {
            m_ObjectiveManager = FindAnyObjectByType<ObjectiveManager>();

            if (m_ObjectiveManager != null)
            {
                m_ObjectiveManager.SubmitObjectiveInterface(m_ObjectiveItem, m_ObjectiveTitle, m_ObjectiveText);
            }
            else
                m_ObjectiveItem.SetActive(false);

            m_SkinWalkerDemon = FindAnyObjectByType<Skinwalker>();

            if (m_SkinWalkerDemon)
                m_SkinWalkerDemon.AssignTarget(transform, m_PlayersCamera, m_SkinWalkerModelToUse);
        }

        //========================================= 

        m_Ghost = FindAnyObjectByType<GhostAI>();

        m_PlayerCollider = GetComponent<CapsuleCollider>();
        m_PlayersOverallSpeed = m_PlayerWalkSpeed;

        m_HealthBar.fillAmount = m_PlayerHealth;
        m_PossessionBar.fillAmount = m_PossesionMeter;

        m_BleedoutObject.SetActive(false);

        m_SlideTimer = m_SlideDuration;
        m_BleedoutTimer = m_BleedoutDuration;

        for (int i = 0; i < m_RagdollColliders.Length; i++)
            Physics.IgnoreCollision(m_RagdollColliders[i], m_PlayerCollider, true);

        DisablePlayerCollision();
        SetRagdoll(false);
    }



    public void EnterTheAfterlife()
    {
        if (!m_MyView.IsMine) return;
        m_AfterlifeFadeIn.SetActive(true);

        m_PlayersCamera.cullingMask = m_AfterlifeRealmMask;

    }

    public PhotonView GetPlayersPhotonView()
    {
        return m_MyView;
    }
    public bool IsPlayerDowned()
    {
        return m_IsDowned;
    }

    void DisablePlayerCollision()
    {
        GameObject[] PlayerColliders = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < PlayerColliders.Length; i++)
            Physics.IgnoreCollision(m_PlayerCollider, PlayerColliders[i].GetComponent<Collider>());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) // Get Rid of player collision
    {
        base.OnPlayerEnteredRoom(newPlayer);
        DisablePlayerCollision();
    }

    /// <summary>
    ///  Converts object position on axis into single values for animation blending.
    /// </summary>
    /// <param name="_xPos"></param>
    /// <param name="_yPos"></param>
    void ConvertMovementForAnimation(float _xPos, float _yPos)
    {
        if (_xPos > 0) // X Conversion
        {
            m_AnimXPos = Mathf.Lerp(m_AnimXPos, 1, m_AnimationLerpSpeed * Time.deltaTime);
        }
        else if (_xPos < 0)
        {
            m_AnimXPos = Mathf.Lerp(m_AnimXPos, -1, m_AnimationLerpSpeed * Time.deltaTime);
        }
        else
        {
            m_AnimXPos = Mathf.Lerp(m_AnimXPos, 0, m_AnimationLerpSpeed * Time.deltaTime);
        }

        if (_yPos > 0) // Y Conversion
        {
            m_AnimYPos = Mathf.Lerp(m_AnimYPos, 1, m_AnimationLerpSpeed * Time.deltaTime);
        }
        else if (_yPos < 0)
        {
            m_AnimYPos = Mathf.Lerp(m_AnimYPos, -1, m_AnimationLerpSpeed * Time.deltaTime);
        }
        else
        {
            m_AnimYPos = Mathf.Lerp(m_AnimYPos, 0, m_AnimationLerpSpeed * Time.deltaTime);
        }


        m_BodyAnimations.SetFloat("xPos", m_AnimXPos);
        m_BodyAnimations.SetFloat("yPos", m_AnimYPos);
    }

    void Update()
    {
        if (!m_MyView.IsMine)
            return;

        if (m_CanMove && !m_IsDowned && !m_isDead && !m_IsInTrapStance)
            UpdateMovement();

        if (!m_ReplensishHealth)
            UpdatePossession();

        if (m_ReplensishHealth)
            UpdateReplen();

        if (m_IsDowned)
            UpdateDownedState();

        if (m_SpinToGhost)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_GhostDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);

            if (Vector3.Dot(transform.forward, m_GhostDirection) >= 0.98f)
            {
                m_SpinToGhost = false;
            }
        }
    }

    /// <summary>
    /// Plays Random footstep sound based on physics material name.
    /// </summary>
    /// <param name="_type"></param>
    public void PlayFootStepAudio(string _type)
    {
        if (_type == "Wood")
        {
            m_FootStepSounds.clip = m_FootStepWood[Random.Range(0, m_FootStepWood.Length)];
            m_FootStepSounds.Play();
        }
        else if (_type == "Concrete")
        {
            m_FootStepSounds.clip = m_FootStepConcrete[Random.Range(0, m_FootStepConcrete.Length)];
            m_FootStepSounds.Play();
        }
        else if (_type == "Dirt")
        {
            m_FootStepSounds.clip = m_FootStepDirt[Random.Range(0, m_FootStepDirt.Length)];
            m_FootStepSounds.Play();
        }
        else if (_type == "Carpet")
        {
            m_FootStepSounds.clip = m_FootStepCarpet[Random.Range(0, m_FootStepCarpet.Length)];
            m_FootStepSounds.Play();
        }
    }

    /// <summary>
    /// Can the player move
    /// </summary>
    /// <returns></returns>
    public bool CanPlayerMove()
    {
        return m_CanMove;
    }

    void UpdateReplen()
    {
        m_PlayerHealth += 0.1f * Time.deltaTime;
        m_HealthBar.fillAmount = m_PlayerHealth;

        m_PossesionMeter -= 0.1f;

        if (m_PossesionMeter <= 0.2f && m_PlayerHealth >= 0.5f)
            m_ReplensishHealth = false;
    }

    void UpdateMovement() // Players overall movement systems.
    {
        float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

        Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

        m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayerLerpSpeedFinal * Time.fixedDeltaTime);

        ConvertMovementForAnimation(xPos, yPos);

        // Emotes

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) && m_IsEmoting)
        {
            m_BodyAnimations.SetInteger("IsEmoting", 0);
            m_PlayerInput.SetLighterBone(true);
            m_PlayerInput.ToggleInverseK(true);

            m_InventoryManager.SetAllItemStates(true);
            m_InventoryManager.ToggleInverseK(true);

            m_IsEmoting = false;
        }

        // Players Movement 

        switch (m_MovementMode)
        {
            case MovementType.Four_Directional: // 4 Directional movement 

                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && !m_IsCrouched)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        m_IsTacticalSprinting = true;
                    }

                    if (m_IsTacticalSprinting)
                        m_PlayersOverallSpeed = m_PlayerSprintSpeed + 1;
                    else
                        m_PlayersOverallSpeed = m_PlayerSprintSpeed;

                    m_IsSprinting = true;
                }
                else
                {
                    m_PlayersOverallSpeed = m_PlayerWalkSpeed;
                    m_IsTacticalSprinting = false;
                    m_IsSprinting = false;
                }
                break;

            case MovementType.Omni_Directional:

                if (Input.GetKey(KeyCode.LeftShift) && !m_IsCrouched && !m_isRecoveryingStamina) // Omni directional movement code
                {

                    m_PlayersOverallSpeed = m_PlayerSprintSpeed;

                    m_IsSprinting = true;
                }
                else
                {
                    m_PlayersOverallSpeed = m_PlayerWalkSpeed;
                    m_IsSprinting = false;
                }
                break;
        }


        if (m_isRecoveryingStamina) // recovering stamina when full drained
        {
            m_StaminaAmount += m_StaminaUsageRate / 2 * Time.deltaTime;

            if (m_StaminaAmount >= 100)
            {
                m_StaminaAmount = 100;
                m_isRecoveryingStamina = false;
            }
        }
        else
        {
            if (m_IsSprinting)
            {
                m_StaminaAmount -= m_StaminaUsageRate * Time.deltaTime;

                if (m_StaminaAmount <= 0)
                {
                    m_isRecoveryingStamina = true;
                    m_StaminaAmount = 0;
                }
            }
            else
            {
                m_StaminaAmount += m_StaminaUsageRate * Time.deltaTime;
            }
        }

        m_StaminaAmount = Mathf.Clamp(m_StaminaAmount, 0, 100);
        m_StaminaBar.value = m_StaminaAmount;

        m_StaminaBar.gameObject.SetActive(m_StaminaAmount != 100 ? true : false);

        UpdateEmotes();

        m_BodyAnimations.SetBool("TacSprint", m_IsTacticalSprinting);
        m_HealthBar.fillAmount = Mathf.Lerp(m_HealthBar.fillAmount, m_PlayerHealth, 5 * Time.deltaTime);

        if (!m_IsSprinting)
        {
            if (Input.GetKey(KeyCode.LeftControl) || m_IsInTrapStance)
            {
                m_CrouchLerpAmount = Mathf.Lerp(m_CrouchLerpAmount, 1, m_CrouchLerpSpeed * Time.deltaTime);
                m_PlayerCollider.center = new Vector3(0, 0.3788545f, 0);
                m_PlayerCollider.height = 0.8057906f;
                m_IsCrouched = true;

            }
            else
            {
                m_CrouchLerpAmount = Mathf.Lerp(m_CrouchLerpAmount, 0, m_CrouchLerpSpeed * Time.deltaTime);
                m_PlayerCollider.center = m_DefaultCollider;
                m_PlayerCollider.height = m_DefaultHeight;
                m_IsCrouched = false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.C)) // Slide 
            {
                StartSliding();
            }

        }

        m_PlayerLerpSpeedFinal = Mathf.Lerp(m_PlayerLerpSpeedFinal, m_PlayersOverallSpeed, 2 * Time.deltaTime);

        if (m_IsSliding)
        {
            m_SlideTimer -= Time.deltaTime;

            if (m_SlideTimer <= 0)
            {
                m_PlayerCollider.center = m_DefaultCollider;
                m_PlayerCollider.height = m_DefaultHeight;

                m_IsSliding = false;
                m_SlideTimer = m_SlideDuration;
            }
        }

        m_BodyAnimations.SetLayerWeight(1, m_CrouchLerpAmount);
        m_BodyAnimations.SetBool("Sprinting", m_IsSprinting);

        switch (m_SpecialistAbility.GetSpecialistType())
        {
            case SpecialistSelected.Exterminator:

                break;
            case SpecialistSelected.Pharmacist:
                UpdateHealingAura();
                break;
            case SpecialistSelected.Trapper:

                break;
            case SpecialistSelected.Cultist:

                break;
        }

        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (collision.gameObject.GetComponent<DoorModule>() && m_IsSprinting &&
            m_SpecialistAbility.GetSpecialistType() == SpecialistSelected.Exterminator
            && !collision.gameObject.GetComponent<DoorModule>().GetDoorState())
        {
            ContactPoint point = collision.contacts[0];
            Vector3 pointNormalDir = point.normal;

            float DotPro = Vector3.Dot(pointNormalDir, -collision.gameObject.transform.up);

            if (DotPro > 0.9f)
            {
                collision.gameObject.GetComponent<DoorModule>().CycleDoorState();
                m_BodyAnimations.SetTrigger("ShoulderBash");
                if (m_ThirdPersonCameraWatcher)
                    m_ThirdPersonCameraWatcher.DoorBashCamAnim();
                m_BodyAnimations.SetLayerWeight(7, 1);

                m_SoundEffects.clip = m_DoorBash;
                m_SoundEffects.Play();
            }
        }
    }

    [PunRPC]
    public void RPC_SpinToPlayer()
    {
        m_SpinToGhost = true;

        m_GhostDirection = (m_Ghost.transform.position - transform.position).normalized;
    }

    public void ResetWeightLayer(int _index)
    {
        m_BodyAnimations.SetLayerWeight(_index, 0);
    }

    public void RestorePossesion() // Pharmacist Specialist
    {
        m_PossesionMeter -= 0.6f;
        m_PossessionBar.fillAmount = m_PossesionMeter;

        m_PossesionMeter = Mathf.Clamp(m_PossesionMeter, 0, 1);
        m_PlayerHealth += 0.1f;

        CheckHealth();
    }
    void UpdateDownedState()
    {
        m_BleedoutTimer -= Time.deltaTime;
        int ConvertedBleedoutTime = (int)m_BleedoutTimer;

        m_BleedoutText.text = "Bleedout Time: " + ConvertedBleedoutTime.ToString();
        if (m_BleedoutTimer <= 0)
        {
            if (PhotonNetwork.PlayerListOthers.Length == 0)
                m_SpectateSystem.SetSpectateMode(true, true);
            else
                m_SpectateSystem.SetSpectateMode(true, false);

            m_PlayersCamera.gameObject.SetActive(false);
            m_MainHudObject.SetActive(false);
            m_SpectateObject.SetActive(true);

            m_MyView.RPC("RPC_CheckIfGameShouldEnd", RpcTarget.MasterClient);
            m_MyView.RPC("RPC_PlayerDeath", RpcTarget.All);
            m_BleedoutTimer = m_BleedoutDuration;
        }
    }

    [PunRPC]
    public void RPC_CheckIfGameShouldEnd()
    {
        m_PlayersDeadInGame++;

        if (m_PlayersDeadInGame >= PhotonNetwork.PlayerList.Length)
        {
            GetComponent<PlayerExperienceManager>().DisplayXpScreenOnNextLoadUp();
            GetComponent<PlayerExperienceManager>().MissionFailed();

            m_GameManager.ChangeNetworkScene("Afterlife_Corp");
        }
    }


    [PunRPC]
    public void RPC_CheckIfAllPlayersHaveExtracted()
    {

        if (m_PlayersDeadInGame >= PhotonNetwork.PlayerList.Length)
        {
            GetComponent<PlayerExperienceManager>().DisplayXpScreenOnNextLoadUp();
            GetComponent<PlayerExperienceManager>().MissionFailed();

            m_GameManager.ChangeNetworkScene("Afterlife_Corp");
        }
    }
    void UpdateHealingAura()
    {
    }

    void UpdatePossession()
    {
        if (m_Ghost)
        {
            float DistanceToGhost = Vector3.Distance(transform.position, m_Ghost.transform.position);
            if (DistanceToGhost < 10)
            {
                m_PossesionMeter += 0.01f * Time.deltaTime;


                Vignette m_Venette;
                if (m_PostProcessing.profile.TryGet(out m_Venette))
                    m_Venette.intensity.value = Mathf.Lerp(m_PossessionBar.fillAmount, m_PossesionMeter, Time.deltaTime);
            }
        }
        else
        {
            m_Ghost = FindAnyObjectByType<GhostAI>();
        }

        if (m_PossesionMeter >= 0.4f)
        {
            if (m_SkinWalkerDemon)
                m_SkinWalkerDemon.SummonSkinwalker();
        }

        m_PossesionMeter = Mathf.Clamp(m_PossesionMeter, 0, 1);
        m_PossessionBar.fillAmount = m_PossesionMeter;
    }

    void UpdateEmotes() // Add emotes here.
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 1
        {
            m_BodyAnimations.SetInteger("IsEmoting", 1);
            m_PlayerInput.SetLighterBone(false);
            m_PlayerInput.ToggleInverseK(false);

            m_InventoryManager.SetAllItemStates(false);
            m_InventoryManager.ToggleInverseK(false);
            m_IsEmoting = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 2
        {
            m_BodyAnimations.SetInteger("IsEmoting", 2);
            m_PlayerInput.SetLighterBone(false);
            m_PlayerInput.ToggleInverseK(false);

            m_InventoryManager.SetAllItemStates(false);
            m_InventoryManager.ToggleInverseK(false);
            m_IsEmoting = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 3
        {
            m_BodyAnimations.SetInteger("IsEmoting", 3);
            m_PlayerInput.SetLighterBone(false);
            m_PlayerInput.ToggleInverseK(false);

            m_InventoryManager.SetAllItemStates(false);
            m_InventoryManager.ToggleInverseK(false);

            m_IsEmoting = true;
        }
    }

    void StartSliding()
    {
        if (m_IsSliding)
            return;

        m_PlayerCollider.center = new Vector3(0, 0.3788545f, 0);
        m_PlayerCollider.height = 0.8057906f;

        m_BodyAnimations.SetTrigger("Slide");
        m_IsSliding = true;
        m_Body.AddForce(transform.forward * m_SlidePower, ForceMode.Impulse);
    }

    void SetRagdoll(bool _state) // Sets players ragdoll mode
    {
        m_BodyAnimations.enabled = !_state;

        for (int i = 0; i < m_RagdollBodys.Length; i++)
        {
            m_RagdollBodys[i].isKinematic = !_state;

            if (_state)
                m_RagdollBodys[i].AddForce(Vector3.up * 5, ForceMode.Impulse);

            m_RagdollBodys[i].useGravity = _state;
        }
        for (int i = 0; i < m_RagdollColliders.Length; i++)
            m_RagdollColliders[i].enabled = _state;

    }

    public void SetTrapStance(bool _state, Transform _trapPosition)
    {
        m_IsInTrapStance = _state;

        if (m_IsInTrapStance)
        {
            m_BodyAnimations.SetLayerWeight(1, 1);
            m_PlayerCollider.center = new Vector3(0, 0.3788545f, 0);
            m_PlayerCollider.height = 0.8057906f;

            m_PlayerInput.ToggleInverseK(false);
            m_PlayerInput.SetLighterBone(false);

            m_InventoryManager.SetAllItemStates(false);
            m_InventoryManager.ToggleInverseK(false);
        }
        else
        {
            m_BodyAnimations.SetLayerWeight(1, 0);
            m_PlayerCollider.center = m_DefaultCollider;
            m_PlayerCollider.height = m_DefaultHeight;

            m_PlayerInput.SetLighterBone(true);
            m_PlayerInput.ToggleInverseK(true);

            m_InventoryManager.SetAllItemStates(true);
            m_InventoryManager.ToggleInverseK(true);
        }


        transform.position = _trapPosition.position;
        Vector3 worldNormal = _trapPosition.transform.TransformDirection(Vector3.forward);
        transform.rotation = Quaternion.LookRotation(worldNormal, Vector3.up);
    }

    public bool m_IsTacSprinting()
    {
        return m_IsTacticalSprinting;
    }

    [PunRPC]
    public void RPC_TakeDamage(float _damageAmount)
    {
        m_PlayerHealth -= _damageAmount;
        m_PlayerHealth = Mathf.Clamp(m_PlayerHealth, 0, 1);

        m_PlayerLerpSpeedFinal = m_PlayerLerpSpeedFinal / 2;
        CheckHealth();
    }


    [PunRPC]
    public void RPC_RestoreHealth(float _amountRetored)
    {
        m_PlayerHealth += _amountRetored;
        m_HealthBar.fillAmount = m_PlayerHealth;
        CheckHealth();
    }

    [PunRPC]
    public void RPC_PharmacistsAbility()
    {
        m_ReplensishHealth = true;
    }

    [PunRPC]
    public void RPC_RestoreSanity(float _amountRetored)
    {
        m_PossesionMeter -= _amountRetored;
        m_PossessionBar.fillAmount = m_PossesionMeter;
        CheckHealth();
    }

    public void ResetHealth()
    {
        m_PlayerHealth = 1;
        m_HealthBar.fillAmount = m_PlayerHealth;
    }

    void CheckHealth()
    {
        if (!m_MyView.IsMine) return;

        if (m_PossesionMeter >= 0.8f)
            m_PlayerHealth = 0f;

        if (m_PlayerHealth <= 0.5f)
        {
            m_PostProcessing.profile.TryGet(out m_Colour);
            m_Colour.colorFilter.value = Color.red;
        }
        else
        {
            m_PostProcessing.profile.TryGet(out m_Colour);
            m_Colour.colorFilter.value = Color.white;
        }

        if (m_PlayerHealth <= 0)
            m_MyView.RPC("RPC_EnterDownedStance", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_PlayerDeath()
    {
        SetRagdoll(true);
        m_DownedFlareObject.SetActive(false);
        gameObject.GetComponent<PlayerCamera>().enabled = false;
        m_IsDowned = false;
        m_isDead = true;
        gameObject.GetComponent<InventoryManager>().DropItemsOnPerson();
        this.enabled = false;
    }

    [PunRPC]
    public void RPC_EnterDownedStance()
    {
        if (!m_MyView.IsMine)
            return;

        m_DownedFlareObject.SetActive(true);

        m_BleedoutObject.SetActive(true);
        m_BodyAnimations.SetInteger("IsEmoting", 0);
        m_BodyAnimations.SetBool("IsDowned", m_IsDowned);
        m_IsDowned = true;
    }

    public bool IsSprinting()
    {
        return m_IsSprinting;
    }

    void Dive()
    {
        if (m_MyView.IsMine && m_CanMove)
        {
            m_PlayerCollider.center = m_DiveCollider;
            m_PlayerCollider.height = m_DiveHeight;

            Vector3 DivePos = new Vector3(0, 0.5f, 0.5f);
            m_Body.AddForce(DivePos * 7, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void RPC_RevivePlayer() // Revived by another player
    {
        m_BleedoutObject.SetActive(false);

        m_IsDowned = false;
        m_PlayerHealth = 100;
        m_BodyAnimations.SetBool("IsDowned", m_IsDowned);
    }

    public bool IsPlayerDead()
    {
        return m_isDead;
    }
    public void SetMovement(bool _state)
    {
        m_CanMove = _state;
    }
}
