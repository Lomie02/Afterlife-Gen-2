using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] bool m_ExitOnEscape = false;
    [SerializeField] UnityEvent m_OnConfigure;
    void Start()
    {
        m_OnConfigure.Invoke();
    }

    public void ChangeScene(string _name)
    {
        SceneManager.LoadScene(_name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && m_ExitOnEscape)
        {
            Application.Quit();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        ChangeScene("Main_Menu");
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChangeNetworkScene(string _name)
    {
        PhotonNetwork.LoadLevel(_name);
    }
}
