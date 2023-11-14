using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class NetworkTypeRegister : MonoBehaviour
{
    [SerializeField] NetworkObject[] m_NetworkObjects;
    void Start()
    {
       // PhotonPeer.RegisterType(typeof(NetworkObject), (byte)'M', NetworkObject.Serilize, NetworkObject.Deserilize);
    }

}
