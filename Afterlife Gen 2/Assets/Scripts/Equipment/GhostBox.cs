using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class GhostBox : MonoBehaviour
{
    [Header("General")]
    [SerializeField] float m_DetectionRange = 10f;

    [Header("Responses")]
    KeywordRecognizer recognizer;
    [Space]

    [SerializeField] Text m_2nd;
    [SerializeField] Text m_3rd;

    NetworkObject m_MyNetworkData;
    [SerializeField] GhostAI m_Ghost;

    [SerializeField] bool m_CanGiveResponses = false;
    NetworkItemSpawner m_Spawner;
    private void Start()
    {
        m_MyNetworkData = GetComponent<NetworkObject>();
        m_Spawner = FindObjectOfType<NetworkItemSpawner>();

        recognizer = m_Spawner.GetReconizer();
        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;

        if (m_Ghost)
        {
            m_Ghost = FindObjectOfType<GhostAI>();
            AssignGhost();
        }

    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.Stop();
            recognizer.Dispose();
        }
    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {

        float DistanceFromGhost;

        if (transform.parent)
            DistanceFromGhost = Vector3.Distance(transform.localPosition, m_Ghost.gameObject.transform.position);
        else
            DistanceFromGhost = Vector3.Distance(transform.position, m_Ghost.gameObject.transform.position);

        if (DistanceFromGhost <= m_DetectionRange && m_CanGiveResponses)
        {
            Debug.Log(args.text);
        }
    }
    void Update()
    {
        if (m_MyNetworkData.GetPowerState())
        {
            int _convertedInt2nd = Random.Range(0, 9);
            int _convertedInt3rd = Random.Range(0, 9);

            m_2nd.text = _convertedInt2nd.ToString();
            m_3rd.text = _convertedInt3rd.ToString();

            if (!m_Ghost)
            {
                m_Ghost = FindObjectOfType<GhostAI>();
                AssignGhost();
            }
        }
    }

    void AssignGhost()
    {
        if (m_Ghost.GetGhostProfile().m_Evidence1 == EvidenceTypes.SpiritBox || m_Ghost.GetGhostProfile().m_Evidence2 == EvidenceTypes.SpiritBox || m_Ghost.GetGhostProfile().m_Evidence3 == EvidenceTypes.SpiritBox)
        {
            m_CanGiveResponses = true;
        }
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }

}
