using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

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
    [SerializeField] UnityEvent m_OnSpecialistEnded;

    [SerializeField] Image m_SpecialistIcon;

    [Header("Charge Rate")]
    [SerializeField] float m_SpecialistChargeRate = 0.02f;
    bool m_SpecialistIsReady = false;
    float m_SpecialistAmount = 0f;

    PhotonView m_View;
    Text m_PressToActivate;
    SpecialistIconSwapper m_SpecialistSwapper;

    PlayerController[] m_PlayersInGame;
    float m_AuraTimer = 0;
    float m_AuraDuration = 10;

    // Exterminator Ability
    [Header("Exterminator Data")]
    [SerializeField] float m_NightVisionDuration = 10f;
    float m_NightTimer = 0;

    [SerializeField] Volume m_PlayersVolume;
    [Space]
    [SerializeField] VolumeProfile m_NightVisionProfile;
    [SerializeField] VolumeProfile m_DefaultProfile;

    bool m_AbilityInUse;

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
            m_OnSpecialistActivated.Invoke();
            SpecialistAbilityToUse();
            ResetAbility();
        }
    }

    void SpecialistAbilityToUse()
    {
        switch (m_SpecialistMode)
        {
            case SpecialistSelected.Exterminator:

                m_NightTimer = m_NightVisionDuration;
                m_AbilityInUse = true;
                m_PlayersVolume.profile = m_NightVisionProfile;

                break;
            case SpecialistSelected.Pharmacist:

                m_AbilityInUse = true;
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
        if (!m_View.IsMine || !m_AbilityInUse) return;

        switch (m_SpecialistMode)
        {
            case SpecialistSelected.Exterminator: // Exterminator Ability Update Loop

                m_NightTimer -= Time.deltaTime;
                if (m_NightTimer <= 0)
                {
                    m_AbilityInUse = false;
                    m_PlayersVolume.profile = m_DefaultProfile;

                    m_OnSpecialistEnded.Invoke();
                }
                break;

            case SpecialistSelected.Pharmacist: // Pharmacist Ability Update

                m_AuraTimer -= Time.deltaTime;
                if (m_AuraTimer <= 0)
                {
                    m_AbilityInUse = false;
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
                break;

            case SpecialistSelected.Trapper:
                break;

            case SpecialistSelected.Cultist:
                break;
        }
    }

    public SpecialistSelected GetSpecialistType()
    {
        return m_SpecialistMode;
    }
}
