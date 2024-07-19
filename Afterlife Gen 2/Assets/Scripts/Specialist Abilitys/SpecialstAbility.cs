using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using System;

public enum SpecialistSelected
{
    Exterminator = 0,
    Pharmacist,
    Trapper,
    Cultist,
}
public class SpecialstAbility : MonoBehaviour
{
    [SerializeField] SpecialistSelected m_SpecialistMode = SpecialistSelected.Exterminator;
    [Space]
    [SerializeField] UnityEvent m_OnSpecialistActivated;
    [SerializeField] Image m_SpecialistIcon;

    float m_SpecialistChargeRate = 0.02f;
    bool m_SpecialistIsReady = false;
    float m_SpecialistAmount = 0f;

    PhotonView m_View;
    Text m_PressToActivate;
    SpecialistIconSwapper m_SpecialistSwapper;

    bool m_IsHealingAura = false;

    PlayerController[] m_PlayersInGame;
    float m_AuraTimer = 0;
    float m_AuraDuration = 10;

    private void Start()
    {
        m_View = GetComponent<PhotonView>();

        if (m_View.IsMine)
        {
            m_SpecialistIcon = GameObject.Find("Specialist_Icon").GetComponent<Image>();
            m_SpecialistIcon.fillAmount = m_SpecialistAmount;
            m_SpecialistIcon.color = Color.white;
            m_PressToActivate = m_SpecialistIcon.gameObject.GetComponentInChildren<Text>();
            m_SpecialistSwapper = m_SpecialistIcon.gameObject.GetComponentInChildren<SpecialistIconSwapper>();

            m_PressToActivate.gameObject.SetActive(false);
            m_AuraTimer = m_AuraDuration;
            switch (m_SpecialistMode)
            {
                case SpecialistSelected.Exterminator:
                    m_SpecialistSwapper.ChangeIcon(0);
                    break;

                case SpecialistSelected.Pharmacist:
                    m_SpecialistSwapper.ChangeIcon(1);
                    break;

                case SpecialistSelected.Trapper:
                    m_SpecialistSwapper.ChangeIcon(2);
                    break;

                case SpecialistSelected.Cultist:
                    m_SpecialistSwapper.ChangeIcon(3);
                    break;
            }
        }
    }

    void ResetAbility()
    {
        m_SpecialistIsReady = false;
        m_SpecialistIcon.fillAmount = 0;

        m_PressToActivate.gameObject.SetActive(false);
        m_SpecialistIcon.color = Color.white;
        m_SpecialistAmount = 0;
    }

    public void UseAbility()
    {
        if (m_SpecialistIsReady)
        {
            SpecialistAbilityToUse();
            ResetAbility();
        }
    }

    void SpecialistAbilityToUse()
    {
        switch (m_SpecialistMode)
        {
            case SpecialistSelected.Exterminator:

                break;
            case SpecialistSelected.Pharmacist:
                BeginHealingAura();
                break;
            case SpecialistSelected.Trapper:

                break;
            case SpecialistSelected.Cultist:

                break;
        }
    }

    void BeginHealingAura()
    {
        m_IsHealingAura = true;

        if (m_PlayersInGame.Length == 0)
        {
            m_PlayersInGame = FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID);
        }

    }

    private void FixedUpdate()
    {
        if (!m_View.IsMine) return;

        if (!m_SpecialistIsReady)
        {
            m_SpecialistAmount += m_SpecialistChargeRate * Time.deltaTime;

            m_SpecialistIcon.fillAmount = m_SpecialistAmount;
            if (m_SpecialistAmount >= 1)
            {
                m_SpecialistIsReady = true;
                m_SpecialistIcon.color = Color.green;
                m_PressToActivate.gameObject.SetActive(true);
            }
        }

    }

    private void Update()
    {
        if (!m_View.IsMine) return;

        if (m_IsHealingAura)
        {
            m_AuraTimer -= Time.deltaTime;
            if(m_AuraTimer <= 0)
            {
                m_IsHealingAura = false;
                m_AuraTimer = m_AuraDuration;
            }

            for (int i = 0; i < m_PlayersInGame.Length; i++)
            {
                if (Vector3.Distance(m_PlayersInGame[i].transform.position, transform.position) <= 5)
                {
                    m_PlayersInGame[i].GetComponent<PhotonView>().RPC("RPC_RestoreHealth", RpcTarget.All, 10f * Time.deltaTime);
                    m_PlayersInGame[i].GetComponent<PhotonView>().RPC("RPC_RestoreSanity", RpcTarget.All, 0.2f * Time.deltaTime);
                }

            }
        }
    }

    public SpecialistSelected GetSpecialistType()
    {
        return m_SpecialistMode;
    }
}
