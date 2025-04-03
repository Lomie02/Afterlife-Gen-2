using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
public class UniversalEvent : MonoBehaviour
{
    [Header("Probability")]
    [SerializeField] PhotonView m_MyView;
    [SerializeField] UnityEvent m_OnTriggered;

    [Header("Probability")]

    [SerializeField] int m_ProbabilityRate = 4;

    [SerializeField] int m_MinProb = 4;
    [SerializeField] int m_MaxProb = 20;

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

    public int GetMaxProb()
    {
        return m_MaxProb;
    }

    public int GetMinProb()
    {
        return m_MinProb;
    }

    public int GetProbalityRate()
    {
        return m_ProbabilityRate;
    }

    public void IncreaseProb()
    {
        m_ProbabilityRate--;
        m_ProbabilityRate = Mathf.Clamp(m_ProbabilityRate, m_MinProb, m_MaxProb);
    }

    public void DecreaseProb()
    {
        m_ProbabilityRate++;
        m_ProbabilityRate = Mathf.Clamp(m_ProbabilityRate, m_MinProb, m_MaxProb);
    }
}
