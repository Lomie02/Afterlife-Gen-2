using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Photon.Pun;

enum MovementType
{
    Four_Directional = 0,
    Omni_Directional,
}
enum PlayerStance
{
    Stand = 0,
    Walk,
    Run,
    Crouch,
    Dive,
    Downed,
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] MovementType m_MovementMode = MovementType.Four_Directional;

    [Space]

    [SerializeField] Volume m_PostProcessing;
    ColorAdjustments m_Colour;

    Rigidbody m_Body;
    Transform m_NewPos;
    [SerializeField] float m_PlayerHealth = 100;
    [SerializeField] float m_PossesionMeter = 0;

    [SerializeField] Slider m_HealthBar;
    [SerializeField] Slider m_PossessionBar;

    PlayerStance m_Stance = PlayerStance.Stand;
    [SerializeField] PhotonView m_MyView;

    [SerializeField] Animator[] m_BodyAnimations;

    float m_PlayersOverallSpeed = 0;
    [SerializeField] float m_PlayerWalkSpeed = 2;
    [SerializeField] float m_PlayerSprintSpeed = 5;
    [SerializeField] float m_PlayerTacticalSprintSpeed = 8;

    CapsuleCollider m_PlayerCollider;

    Vector3 m_DefaultCollider = new Vector3(0, 0.8321516f, 0);
    float m_DefaultHeight = 1.712385f;

    Vector3 m_DiveCollider = new Vector3(0, 0.2867912f, 0);
    float m_DiveHeight = 0.621664f;

    bool m_IsDiving = false;

    float m_DiveWaitTimer = 0;
    float m_DiveWaitDuration = 1;

    bool m_CanMove = true;
    bool m_IsSprinting = false;
    bool m_IsTacticalSprinting = false;

    GhostAI m_Ghost;

    float m_AnimationLerpSpeed = 4;
    float m_AnimXPos;
    float m_AnimYPos;

    float m_CrouchLerpSpeed = 2;
    float m_CrouchLerpAmount;
    bool m_IsCrouched = false;

    // Sliding Mechanics
    bool m_IsSliding;
    float m_SlideTimer = 0;
    [SerializeField]float m_SlideDuration = 0;
    [SerializeField]float m_SlidePower = 10;

    // Player Downed
    bool m_IsDowned = false;
    float m_BleedoutTimer;
    float m_BleedoutDuration = 20;

    void Start()
    {
        m_Body = GetComponent<Rigidbody>();
        m_MyView = GetComponent<PhotonView>();
        m_Ghost = FindAnyObjectByType<GhostAI>();

        m_PlayerCollider = GetComponent<CapsuleCollider>();
        m_PlayersOverallSpeed = m_PlayerWalkSpeed;

        m_HealthBar.value = m_PlayerHealth;
        m_PossessionBar.value = m_PossesionMeter;

        m_SlideTimer = m_SlideDuration;
        m_BleedoutTimer = m_BleedoutDuration;
    }

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


        for (int i = 0; i < m_BodyAnimations.Length; i++)
        {
            m_BodyAnimations[i].SetFloat("xPos", m_AnimXPos);
            m_BodyAnimations[i].SetFloat("yPos", m_AnimYPos);
        }
    }
    void Update()
    {
        if (m_MyView.IsMine && m_CanMove && !m_IsDowned)
        {
            float xPos = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            float yPos = Input.GetAxisRaw("Vertical") * Time.deltaTime;

            Vector3 MoveV = transform.right * xPos + transform.forward * yPos;

            m_Body.MovePosition(transform.position + MoveV.normalized * m_PlayersOverallSpeed * Time.deltaTime);

            ConvertMovementForAnimation(xPos, yPos);

            // Emotes

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                m_BodyAnimations[0].SetInteger("IsEmoting", 0);
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

                    if (Input.GetKeyDown(KeyCode.Space) && Input.GetKeyDown(KeyCode.W) && Input.GetKey(KeyCode.LeftShift)) // tactical Sprint code
                    {
                        m_IsTacticalSprinting = true;
                    }

                    if (Input.GetKey(KeyCode.LeftShift) && !m_IsCrouched) // Omni directional movement code
                    {

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
            }


            if (Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 1
            {
                m_BodyAnimations[0].SetInteger("IsEmoting", 1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 2
            {
                m_BodyAnimations[0].SetInteger("IsEmoting", 2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && !Input.GetKeyDown(KeyCode.W)) // Emote Dance 3
            {
                m_BodyAnimations[0].SetInteger("IsEmoting", 3);
            }

            for (int i = 0; i < m_BodyAnimations.Length; i++)
            {
                m_BodyAnimations[i].SetBool("TacSprint", m_IsTacticalSprinting);
            }

            if (!m_IsSprinting)
            {
                if (Input.GetKey(KeyCode.C))
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

            m_BodyAnimations[0].SetLayerWeight(4, m_CrouchLerpAmount);

            if (m_IsDiving)
            {
                m_DiveWaitTimer += Time.deltaTime;

                if (m_DiveWaitTimer >= m_DiveWaitDuration)
                {
                    m_DiveWaitTimer = 0;

                    if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
                    {
                        m_PlayerCollider.center = m_DefaultCollider;
                        m_PlayerCollider.height = m_DefaultHeight;
                        m_IsDiving = false;
                    }
                }
            }


            for (int i = 0; i < m_BodyAnimations.Length; i++)
            {
                m_BodyAnimations[i].SetBool("Sprinting", m_IsSprinting);
            }
        }


        if (m_Ghost)
        {
            float DistanceToGhost = Vector3.Distance(transform.position, m_Ghost.transform.position);
            if (DistanceToGhost < 10)
            {
                m_PossesionMeter += 1 * Time.deltaTime;
            }
        }
        else
        {
            m_Ghost = FindAnyObjectByType<GhostAI>();
        }

        if (m_IsDowned)
        {
            m_BleedoutTimer -= Time.deltaTime;
            if (m_BleedoutTimer <= 0)
            {
                m_IsDowned = false;
                //TODO: Send player to spectate
                m_MyView.RPC("RPC_RevivePlayer", RpcTarget.All);
                m_BleedoutTimer = m_BleedoutDuration;
            }
        }

        m_PossessionBar.value = m_PossesionMeter;
    }

    void StartSliding()
    {
        if (m_IsSliding)
            return;

        m_PlayerCollider.center = new Vector3(0, 0.3788545f, 0);
        m_PlayerCollider.height = 0.8057906f;

        m_BodyAnimations[0].SetTrigger("Slide");
        m_IsSliding = true;
        m_Body.AddForce(transform.forward * m_SlidePower, ForceMode.Impulse);
    }

    bool m_IsTacSprinting()
    {
        return m_IsTacticalSprinting;
    }

    public void TakeDamage(float _damageAmount)
    {
        m_PlayerHealth -= _damageAmount;
        m_PlayerHealth = Mathf.Clamp(m_PlayerHealth, 0, 100);

        m_HealthBar.value = m_PlayerHealth;
        CheckHealth();
    }

    public void RestoreHealth(float _amountRetored)
    {
        m_PlayerHealth += _amountRetored;
        m_HealthBar.value = m_PlayerHealth;
    }

    public void ResetHealth()
    {
        m_PlayerHealth = 100;
        m_HealthBar.value = m_PlayerHealth;
    }

    void CheckHealth()
    {
        if(!m_MyView.IsMine)

        if (m_PlayerHealth <= 50)
        {
            m_PostProcessing.profile.TryGet(out m_Colour);

            m_Colour.colorFilter.value = Color.red;
        }

        if (m_PlayerHealth <= 0)
        {
            m_MyView.RPC("RPC_EnterDownedStance", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_EnterDownedStance()
    {
        if (!m_MyView.IsMine)
            return;

        m_IsDowned = true;
        m_BodyAnimations[0].SetBool("IsDowned", m_IsDowned);
        m_PlayerHealth = 100;
    }

    public bool IsTacticalSprinting()
    {
        return m_IsTacticalSprinting;
    }

    void Dive()
    {
        if (m_MyView.IsMine && m_CanMove)
        {
            m_IsDiving = true;
            m_PlayerCollider.center = m_DiveCollider;
            m_PlayerCollider.height = m_DiveHeight;

            Vector3 DivePos = new Vector3(0, 0.5f, 0.5f);
            m_Body.AddForce(DivePos * 7, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void RPC_RevivePlayer() // Revived by another player
    {
        m_IsDowned = false;
        m_BodyAnimations[0].SetBool("IsDowned", m_IsDowned);
    }

    public void SetMovement(bool _state)
    {
        m_CanMove = _state;
    }
}
