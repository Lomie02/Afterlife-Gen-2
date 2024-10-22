using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Diagnostics;

public class GhostHumanViewDetector : MonoBehaviour
{
    GhostAI m_GhostAiSystem;
    public void Start()
    {
        m_GhostAiSystem = GetComponentInParent<GhostAI>();
    }

    public void OnTriggerEnter(Collider other) // Player has entered the Ais view
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.gameObject.tag == "Player" && !other.gameObject.GetComponent<PlayerController>().IsPlayerDowned())
        {
            m_GhostAiSystem.PlayerHasEnteredView(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other) // Player leaves the Ais view
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.gameObject.tag == "Player")
        {
            m_GhostAiSystem.PlayerHasExitView(other.gameObject);
        }
    }
}
