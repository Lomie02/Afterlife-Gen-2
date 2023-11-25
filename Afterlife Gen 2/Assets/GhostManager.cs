using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GhostManager : MonoBehaviour
{
    [SerializeField] GameObject m_Ghost;
    [SerializeField] Transform[] m_GhostSpawnLocations;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int randomSpawn = Random.Range(0, m_GhostSpawnLocations.Length - 1);
            PhotonNetwork.InstantiateRoomObject(m_Ghost.name, m_GhostSpawnLocations[randomSpawn].position, m_GhostSpawnLocations[randomSpawn].rotation);
        }
    }

}
