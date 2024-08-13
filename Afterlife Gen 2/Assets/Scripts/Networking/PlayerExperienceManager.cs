using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerExperienceManager : MonoBehaviour
{
    [SerializeField] GameObject m_XpGameObjectInterface;
    [SerializeField] Image m_XpCircleBar;
    [SerializeField] Button m_ContinueButton;
    [SerializeField] PhotonView m_MyView;

    // Interface Texts
    [Header("Text Related Objects")]
    [SerializeField] Text m_OverallPlayerLevel;
    [SerializeField] Text m_MissionSuccessfulStatus;

    [SerializeField] Text m_ExtractionStatus;
    [SerializeField] Text m_SurvivedStatus;
    [SerializeField] Text m_BonusEarnedStatus;

    PlayerController m_MyController;
    PlayerCamera m_MyPlayersCamera;

    int m_PlayersLevel = 1;

    int m_ExperiencePointsEarnedFromMatch = 0;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();

        if (!m_MyView) return;

        m_MyController = GetComponent<PlayerController>();
        m_MyPlayersCamera = GetComponent<PlayerCamera>();

        m_ContinueButton.onClick.AddListener(delegate { m_MyPlayersCamera.MouseLockState(true); });
        m_ContinueButton.onClick.AddListener(delegate { m_MyController.SetMovement(true); });

        if (PlayerPrefs.GetInt("display_xp_screen_on_next_load") == 1)
        {

            m_XpCircleBar.fillAmount = PlayerPrefs.GetFloat("xp_fill_amount");
            m_PlayersLevel = PlayerPrefs.GetInt("xp_overall_level");

            CalculatePlayersExperience();

            m_OverallPlayerLevel.text = m_PlayersLevel.ToString();

            m_MyPlayersCamera.MouseLockState(false);
            m_MyController.SetMovement(false);

            SaveAllPlayerExperienceData();

            SetXpSetActive(true);
            PlayerPrefs.SetInt("display_xp_screen_on_next_load", 0); // Set back to 0 so it doesnt load the xp bar when loading into the map
        }
    }

    void CalculatePlayersExperience()
    {
        if (PlayerPrefs.GetInt("xp_mission_failed") == 1) // If equal to 1 then the previous match failed & all players had died.
        {
            m_MissionSuccessfulStatus.text = "Extraction Failed: All Players Died.";
            m_ExtractionStatus.text = "Failed Ghost Extraction: " + " No Xp Earned.";

            m_SurvivedStatus.text = "Did Not Survive: " + "No Xp Earned.";
            m_BonusEarnedStatus.text = "Bonus From The Boss: " + "No Bonus Earned.";

            PlayerPrefs.SetInt("xp_mission_failed", 0);
            return; // No XP is earned when mission failed.
        }

        m_XpCircleBar.fillAmount += 0.10f; // XP for successful extract (At least 1 player has to complete the extract)
        m_ExtractionStatus.text = "Successful Ghost Extraction: " + "10 Xp";

        if (PlayerPrefs.GetInt("xp_player_survived") == 1) // Extracted Alive & Earns a bonus
        {
            m_XpCircleBar.fillAmount += 0.12f; // XP for Surviving.
            m_XpCircleBar.fillAmount += 0.5f; // XP for Bonus
            m_SurvivedStatus.text = "Survived Extraction: " + "12 Xp";
            m_BonusEarnedStatus.text = "Bonus From The Boss: " + "5 Xp";
        }
        else
        {
            m_SurvivedStatus.text = "Did Not Survive: " + "No Xp Earned.";
            m_BonusEarnedStatus.text = "Bonus From The Boss: " + "No Bonus Earned.";
        }


        m_MissionSuccessfulStatus.text = "Extraction Successful: Ghost Was Captured";

        if (m_XpCircleBar.fillAmount >= 1)
        {
            m_PlayersLevel++;
            m_XpCircleBar.fillAmount = 0;
        }
    }

    public void SaveAllPlayerExperienceData()
    {
        PlayerPrefs.SetFloat("xp_fill_amount", m_XpCircleBar.fillAmount);
        PlayerPrefs.SetInt("xp_overall_level", m_PlayersLevel);
    }

    public void SetXpSetActive(bool _state)
    {
        m_XpGameObjectInterface.SetActive(_state);
    }


    public void DisplayXpScreenOnNextLoadUp()
    {
        m_MyView.RPC("RPC_DisplayExperienceOnLoadUp", RpcTarget.All);
    }

    public void MissionFailed()
    {
        m_MyView.RPC("RPC_MissionUnsuccessful", RpcTarget.All);
    }

    public void MissionCompleted()
    {
        m_MyView.RPC("RPC_MissionWasCompleted", RpcTarget.All);
    }

    // Network Remote Call Pros
    [PunRPC]
    public void RPC_DisplayExperienceOnLoadUp()
    {
        PlayerPrefs.SetInt("display_xp_screen_on_next_load", 1); // When Player get back into the lobby it should display the xp Screen.
    }

    [PunRPC]
    public void RPC_MissionUnsuccessful() // All Players in the match died so no xp is earned
    {
        PlayerPrefs.SetInt("xp_mission_failed", 1); // When Player get back into the lobby it should display the xp Screen.
    }

    [PunRPC]
    public void RPC_MissionWasCompleted() // All Players in the match died so no xp is earned
    {
        PlayerPrefs.SetInt("xp_mission_success", 1); // When Player get back into the lobby it should display the xp Screen.
    }

    [PunRPC]
    public void RPC_PlayerSurvived() // Did the player survive the extaction
    {
        PlayerPrefs.SetInt("xp_player_survived", 1); // if equal to 1 then player had survived the extraction.
    }

}
