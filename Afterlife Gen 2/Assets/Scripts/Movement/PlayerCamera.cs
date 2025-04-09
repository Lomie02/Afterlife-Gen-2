using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Camera m_PlayersCamera;
    [SerializeField] float m_Sens = 200;

    [SerializeField] Transform m_Body;

    NetworkWorldCameraHud m_LobbyHud;

    PhotonView m_MyView;
    float m_Xview;
    float m_Yview;

    bool m_CanLookAround = true;
    bool m_TrapStance;
    SettingsPreferenceManager m_SettingsPreferenceManager;

    // Leg IK
    public Transform TorsoController;
    public Transform CharacterRoot;

    public float maxTorsoRotation = 45f;
    public float fullBodyTurnSpeed = 5f;

    public void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        if (!m_MyView.IsMine)
        {
            gameObject.GetComponent<Camera>().enabled = false;
            gameObject.GetComponent<AudioSource>().enabled = false;

            enabled = false;
        }

        if (m_MyView)
        {
            if (PlayerPrefs.HasKey("player_settings_mouse"))
                m_Sens = PlayerPrefs.GetFloat("player_settings_mouse");

            m_SettingsPreferenceManager = GetComponentInChildren<SettingsPreferenceManager>();

            UpdateRelSettings();

            m_SettingsPreferenceManager.m_OnSettingsApplied.AddListener(UpdateRelSettings);

            NetworkWorldCameraHud[] m_Canvases = FindObjectsByType<NetworkWorldCameraHud>(FindObjectsSortMode.None);

            for (int i = 0; i < m_Canvases.Length; i++)
            {
                if (m_Canvases[i].tag == "LobbyHud")
                {
                    m_LobbyHud = m_Canvases[i];
                    m_LobbyHud.AssignCamera(m_PlayersCamera);
                    break;
                }
            }
            MouseLockState(true);
            StartCoroutine(UpdateCamera());
        }


    }

    public void SetTrapStance(bool _state, Vector3 _positionTarget)
    {
        m_TrapStance = _state;

        if (m_TrapStance)
        {
            Cursor.visible = true;
            m_CanLookAround = false;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            m_CanLookAround = true;
        }

        transform.forward = _positionTarget;
    }

    void UpdateRelSettings()
    {
        m_Sens = m_SettingsPreferenceManager.FetchMouseSens();
        m_PlayersCamera.fieldOfView = m_SettingsPreferenceManager.FetchFieldofView();
    }

    IEnumerator UpdateCamera()
    {
        while (true)
        {
            if (m_CanLookAround)
            {
                float xPos = Input.GetAxis("Mouse X") * m_Sens * Time.deltaTime;
                float yPos = Input.GetAxis("Mouse Y") * m_Sens * Time.deltaTime;

                m_Xview += xPos;
                m_Yview -= yPos;

                m_Body.Rotate(Vector3.up, xPos);

                m_Yview = Mathf.Clamp(m_Yview, -30, 62);

                m_PlayersCamera.transform.localEulerAngles = new Vector3(m_Yview, 0, 0);

            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void MouseLockState(bool _state)
    {
        if (!m_MyView.IsMine || m_TrapStance)
            return;

        if (_state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            m_CanLookAround = true;
        }
        else
        {
            Cursor.visible = true;
            m_CanLookAround = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetCameraState(bool _state)
    {
        m_PlayersCamera.enabled = _state;
    }
}
