using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkWorldCameraHud : MonoBehaviour
{
    Canvas m_Canvas;

    private void Start()
    {
        m_Canvas = GetComponent<Canvas>();

    }

    public void AssignCamera(Camera _cam)
    {
        m_Canvas.worldCamera = _cam;
    }

}
