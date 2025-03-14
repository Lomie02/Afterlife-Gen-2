using Photon.Pun;
using UnityEngine;

public class AnimationEventWatcher : MonoBehaviour
{
    PlayerInput m_PlayerInput;
    InventoryManager m_InventoryManager;
    PlayerController m_PlayerController;
    PlayerCamera m_PlayerCamera;

    Animator m_Animator;

    public bool m_IsThirdPersonCamera;

    void Start()
    {
        m_PlayerInput = GetComponentInParent<PlayerInput>();
        m_InventoryManager = GetComponentInParent<InventoryManager>();
        m_PlayerController = GetComponentInParent<PlayerController>();
        m_PlayerCamera = GetComponentInParent<PlayerCamera>();

        if (m_IsThirdPersonCamera)
        {
            GetComponent<Camera>().enabled = false;
            m_Animator = GetComponent<Animator>();
        }

    }

    public void EnableLighter()
    {
        m_PlayerInput.EnableLighter();
    }

    public void Hide()
    {
        transform.gameObject.SetActive(false);
    }

    public void SetRightArmState(int _state) // Set only right arm IK & equipment states.
    {
        bool converted = _state != 0;

        m_InventoryManager.SetAllItemStates(converted);
        m_InventoryManager.ToggleInverseK(converted);
    }

    public void SetLeftArmIK(int _state)
    {
        bool converted = _state != 0;
        m_PlayerInput.ToggleInverseK(converted);
    }

    public void SetLeftArmState(int _state) // Set only left arm IK & equipment bone state
    {
        bool converted = _state != 0;
        m_PlayerInput.ToggleInverseK(converted);
        m_PlayerInput.SetLighterBone(converted);
    }

    public void SetBothArmsStates(int _state) // Set both arms IKs & item bones states
    {
        bool converted = _state != 0;

        m_PlayerInput.ToggleInverseK(converted);
        m_PlayerInput.SetLighterBone(converted);

        m_PlayerInput.SetKeepLighterWeight(converted);
        
        m_InventoryManager.SetAllItemStates(converted);
        m_InventoryManager.ToggleInverseK(converted);
    }

    public void ResetBashWeight()
    {
        m_PlayerController.ResetWeightLayer(7);
    }

    public void ResetInteractWeight()
    {
        m_PlayerController.ResetWeightLayer(8);
    }

    public void StopLighterInspect()
    {
        m_PlayerInput.SetInspectStatus(false);
    }

    public void HidePlayerCamera()
    {
        m_PlayerCamera.SetCameraState(false);
        if (m_IsThirdPersonCamera)
            GetComponent<Camera>().enabled = true;
    }

    public void DoorBashCameraStart()
    {

    }

    public void DoorBashCamAnim()
    {
        m_Animator.SetTrigger("DoorBash");
    }

    public void DoorBashCameraStop()
    {

    }

    public void ShowPlayerCamera()
    {
        m_PlayerCamera.SetCameraState(true);
        if (m_IsThirdPersonCamera)
            GetComponent<Camera>().enabled = false;
    }
}
