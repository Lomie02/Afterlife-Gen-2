using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
public class UniversalEvent : MonoBehaviour
{
    [SerializeField] PhotonView m_MyView;
    [SerializeField] UnityEvent m_OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (m_MyView.IsMine && other.tag == "Player")
        {
            m_MyView.RPC("RPC_OnGhostEventTriggered", RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_OnGhostEventTriggered()
    {
        m_OnTriggered.Invoke();
        gameObject.SetActive(false);
    }

    public void RestoreEvent()
    {
        m_MyView.RPC("RPC_RestoreEvent", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_RestoreEvent()
    {
        gameObject.SetActive(true);
    }

    [PunRPC]
    public void RPC_ClearEvent()
    {
        gameObject.SetActive(false);
    }

    public void ClearEvent()
    {
        m_MyView.RPC("RPC_ClearEvent", RpcTarget.All);
    }
}
