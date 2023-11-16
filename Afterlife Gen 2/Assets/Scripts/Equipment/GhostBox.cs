using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostBox : MonoBehaviour
{
    [SerializeField] Text m_2nd;
    [SerializeField] Text m_3rd;

    NetworkObject m_MyNetworkData;

    private void Start()
    {
        m_MyNetworkData = GetComponent<NetworkObject>();
    }
    void Update()
    {
        if (m_MyNetworkData.GetPowerState())
        {
            int _convertedInt2nd = Random.Range(0, 9);
            int _convertedInt3rd = Random.Range(0, 9);

            m_2nd.text = _convertedInt2nd.ToString();
            m_3rd.text = _convertedInt3rd.ToString();
        }
    }
}
