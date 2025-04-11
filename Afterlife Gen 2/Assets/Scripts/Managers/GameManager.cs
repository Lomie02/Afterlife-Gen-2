using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] bool m_ExitOnEscape = false;
    [SerializeField] UnityEvent m_OnConfigure;

    float m_Fps;

    void Start()
    {
        m_OnConfigure.Invoke();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //StartCoroutine(DisplayFps());
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

    IEnumerator DisplayFps()
    {
        while (true)
        {
            m_Fps = 1.0f / Time.deltaTime;
            Debug.Log("FPS: " + Mathf.Ceil(m_Fps));
            yield return new WaitForSeconds(0.5f);
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
