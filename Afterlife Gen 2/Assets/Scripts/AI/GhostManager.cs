using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum EvidenceTypes
{
    SpiritBox = 0,
    Emf,
    Writing,
    LaserProjector,
    BloodyHandprints,
    GhostOrb,
    AudioSensor,
    FreezingTemps,
    FloatingObjects,
    RemPod,
    Bloodtrail,
}

[System.Serializable]
public struct GhostProfile
{
    public string m_GhostName;
    public EvidenceTypes m_Evidence1;
    public EvidenceTypes m_Evidence2;
    public EvidenceTypes m_Evidence3;
    public EvidenceTypes m_Evidence4;
}
public class GhostManager : MonoBehaviour
{
    [SerializeField] GhostProfile[] m_GhostProfiles;

    [SerializeField] GameObject m_Ghost;
    [SerializeField] Transform[] m_GhostSpawnLocations;

    [SerializeField] GhostAI m_GhostAi;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int randomSpawn = Random.Range(0, m_GhostSpawnLocations.Length - 1);
            PhotonNetwork.InstantiateRoomObject(m_Ghost.name, m_GhostSpawnLocations[randomSpawn].position, m_GhostSpawnLocations[randomSpawn].rotation);

            m_GhostAi = FindFirstObjectByType<GhostAI>();
            m_GhostAi.SetGhostProfile(Random.Range(0, m_GhostProfiles.Length - 1));
        }
    }

    public GhostProfile GrabGhostProfile(int _index)
    {
        return m_GhostProfiles[_index];
    }


}
