using Photon.Pun;
using UnityEngine;

public class AnimationEventWatcher : MonoBehaviour
{
    PlayerInput m_PlayerInput;

    void Start()
    {
        m_PlayerInput = GetComponentInParent<PlayerInput>();
    }

    public void EnableLighter()
    {
        m_PlayerInput.EnableLighter();
    }
}
