using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

enum SpecialistSelected
{
    Pharmacist = 0,
    Exorcist,
    Trapper,
    Mechanic,
}
public class SpecialstAbility : MonoBehaviour
{
    [SerializeField] SpecialistSelected m_SpecialistMode = SpecialistSelected.Pharmacist;
    [Space]
    [SerializeField] UnityEvent m_OnSpecialistActivated;
    [SerializeField] Image m_SpecialistIcon;

    float m_SpecialistChargeRate = 0.02f;
    bool m_SpecialistIsReady = false;
    float m_SpecialistAmount = 0f;

    PhotonView m_View;
    Text m_PressToActivate;
    SpecialistIconSwapper m_SpecialistSwapper;
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

            switch (m_SpecialistMode)
            {
                case SpecialistSelected.Pharmacist:
                    m_SpecialistSwapper.ChangeIcon(0);
                    break;

                case SpecialistSelected.Exorcist:
                    m_SpecialistSwapper.ChangeIcon(1);
                    break;

                case SpecialistSelected.Trapper:
                    m_SpecialistSwapper.ChangeIcon(2);
                    break;

                case SpecialistSelected.Mechanic:
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
            ResetAbility();
        }
    }

    private void FixedUpdate()
    {
        if (m_View.IsMine && !m_SpecialistIsReady)
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
}
