using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[System.Serializable]
struct ItemPoolObject
{
    public string m_ItemsName;
    [Space]
    public GameObject m_ItemObject;
    public int m_MaxAmount;
    public int m_AmountSpawned;
}

enum ItemSpawnMethod
{
    AllSpawns = 0,
    RandomSpawns,
    RandomMax,
    MaxSpawn,
}
public class NetworkItemSpawner : MonoBehaviour
{
    [Header("Spawn Mode")]
    [SerializeField] ItemSpawnMethod m_SpawnMode;

    [Header("Max Mode Settings")]
    [SerializeField, Range(2,5)] int m_RandomOdds = 2;
    [SerializeField] int m_MaxItemsAllowed = 100;
    int m_ItemsSpawned;

    [Header("Item Pool")]
    [SerializeField] ItemPoolObject[] m_NetworkPool;

    [HideInInspector]
    [SerializeField] Transform[] m_SpawnLocations;

    int m_PreviousItemSpawned = 0;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        GameObject[] m_ObjectSpawnList;
        m_ObjectSpawnList = GameObject.FindGameObjectsWithTag("ItemSpawn");
        m_SpawnLocations = new Transform[m_ObjectSpawnList.Length];
        for (int i = 0; i < m_ObjectSpawnList.Length; i++)
        {
            m_SpawnLocations[i] = m_ObjectSpawnList[i].transform;
        }

        AssignPoolObjects();
    }
    void AssignPoolObjects()
    {
        for (int i = 0; i < m_SpawnLocations.Length; i++)
        {
            switch (m_SpawnMode)
            {
                case ItemSpawnMethod.AllSpawns:
                    int RandomItems_01 = Random.Range(0, m_NetworkPool.Length);

                    if (m_NetworkPool[RandomItems_01].m_AmountSpawned < m_NetworkPool[RandomItems_01].m_MaxAmount)
                    {
                        PhotonNetwork.InstantiateRoomObject(m_NetworkPool[RandomItems_01].m_ItemObject.name, m_SpawnLocations[i].position, Quaternion.identity);
                        m_NetworkPool[RandomItems_01].m_AmountSpawned++;
                    }
                    break;

                case ItemSpawnMethod.RandomSpawns:

                    int RandomItems_02 = Random.Range(0, m_NetworkPool.Length);
                    int CanSpawnItems = Random.Range(0, m_RandomOdds);

                    if (CanSpawnItems == 1)
                    {
                        if (m_NetworkPool[RandomItems_02].m_AmountSpawned < m_NetworkPool[RandomItems_02].m_MaxAmount)
                        {
                            PhotonNetwork.InstantiateRoomObject(m_NetworkPool[RandomItems_02].m_ItemObject.name, m_SpawnLocations[i].position, Quaternion.identity);
                            m_NetworkPool[RandomItems_02].m_AmountSpawned++;
                        }
                    }
                    break;

                case ItemSpawnMethod.RandomMax:

                    int RandomItems_03 = Random.Range(0, m_NetworkPool.Length);
                    int CanSpawnItems_1 = Random.Range(0, 2);

                    if (CanSpawnItems_1 == 1 && m_ItemsSpawned < m_MaxItemsAllowed)
                    {
                        if (m_NetworkPool[RandomItems_03].m_AmountSpawned < m_NetworkPool[RandomItems_03].m_MaxAmount)
                        {
                            PhotonNetwork.InstantiateRoomObject(m_NetworkPool[RandomItems_03].m_ItemObject.name, m_SpawnLocations[i].position, Quaternion.identity);
                            m_NetworkPool[RandomItems_03].m_AmountSpawned++;
                            m_ItemsSpawned++;
                        }
                    }
                    break;

                case ItemSpawnMethod.MaxSpawn:

                    int RandomItems_04 = Random.Range(0, m_NetworkPool.Length);

                    if (m_ItemsSpawned < m_MaxItemsAllowed)
                    {
                        if (m_NetworkPool[RandomItems_04].m_AmountSpawned < m_NetworkPool[RandomItems_04].m_MaxAmount)
                        {
                            PhotonNetwork.InstantiateRoomObject(m_NetworkPool[RandomItems_04].m_ItemObject.name, m_SpawnLocations[i].position, Quaternion.identity);
                            m_NetworkPool[RandomItems_04].m_AmountSpawned++;
                            m_ItemsSpawned++;
                        }
                    }
                    break;
            }

        }
    }
}
