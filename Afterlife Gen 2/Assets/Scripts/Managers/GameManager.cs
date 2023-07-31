using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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


}
