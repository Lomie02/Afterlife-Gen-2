using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using Photon.Pun;
public class GhostBox : MonoBehaviour
{
    [Header("General")]
    PhotonView m_MyView;
    [SerializeField] float m_DetectionRange = 10f;

    [Header("Responses")]
    KeywordRecognizer recognizer;
    [Space]

    [SerializeField] Text m_2nd;
    [SerializeField] Text m_3rd;

    NetworkObject m_MyNetworkData;
    [SerializeField] CursedObject m_CursedObject;

    [SerializeField] bool m_CanGiveResponses = false;
    NetworkItemSpawner m_Spawner;

    AudioSource m_AudioSource;
    [SerializeField] AudioClip[] m_AudioResponses;

    private void Start()
    {
        m_MyNetworkData = GetComponent<NetworkObject>();
        m_Spawner = FindFirstObjectByType<NetworkItemSpawner>();

        LookForCursedOject();

        m_MyView = GetComponent<PhotonView>();
        m_AudioSource = GetComponent<AudioSource>();
        recognizer = m_Spawner.GetReconizer();
        recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;

        if (m_CursedObject)
        {
            LookForCursedOject();
            AssignGhost();
        }

        StartCoroutine(UpdateGhostBox());
    }

    void LookForCursedOject()
    {
        CursedObject[] TempListCursedObjects = GameObject.FindObjectsByType<CursedObject>(FindObjectsSortMode.None);

        for (int i = 0; i < TempListCursedObjects.Length; i++)
        {
            if (TempListCursedObjects[i].IsCursedObject())
            {
                m_CursedObject = TempListCursedObjects[i];
                break;
            }
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
        if (!m_MyNetworkData.GetPowerState())
            return;

        float DistanceFromGhost;

            DistanceFromGhost = Vector3.Distance(transform.position, m_CursedObject.gameObject.transform.position);

        if (DistanceFromGhost < m_DetectionRange && m_CanGiveResponses)
        {
            m_MyView.RPC("RPC_GiveResponse", RpcTarget.All, Random.Range(0, m_AudioResponses.Length));
            Debug.Log(args.text);
        }
    }
    private IEnumerator UpdateGhostBox()
    {
        while (true)
        {

            if (m_MyNetworkData.GetPowerState())
            {
                int _convertedInt2nd = Random.Range(0, 9);
                int _convertedInt3rd = Random.Range(0, 9);

                m_2nd.text = _convertedInt2nd.ToString();
                m_3rd.text = _convertedInt3rd.ToString();

                if (!m_CursedObject)
                {
                    m_CursedObject = FindObjectOfType<CursedObject>();
                    AssignGhost();
                }
            }

            yield return new WaitForSeconds(5f);
        }
    }

    [PunRPC]
    public void RPC_GiveResponse(int _response)
    {
        if (m_AudioSource.isPlaying)
            return;

        m_AudioSource.clip = m_AudioResponses[_response];
        m_AudioSource.Play();
    }

    void AssignGhost()
    {
        if (m_CursedObject.GetGhostProfile().m_Evidence1 == EvidenceTypes.SpiritBox || m_CursedObject.GetGhostProfile().m_Evidence2 == EvidenceTypes.SpiritBox || m_CursedObject.GetGhostProfile().m_Evidence3 == EvidenceTypes.SpiritBox)
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
