using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
}
