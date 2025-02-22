using Photon.Pun;
using UnityEngine;

public class AnimationEventWatcher : MonoBehaviour
{
    PlayerInput m_PlayerInput;
    InventoryManager m_InventoryManager;

    PlayerController m_PlayerController;
    void Start()
    {
        m_PlayerInput = GetComponentInParent<PlayerInput>();
        m_InventoryManager = GetComponentInParent<InventoryManager>();
        m_PlayerController = GetComponentInParent<PlayerController>();
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

    public void StopLighterInspect()
    {
        m_PlayerInput.SetInspectStatus(false);
    }
}
