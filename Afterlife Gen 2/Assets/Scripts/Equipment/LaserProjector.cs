using UnityEngine;

public class LaserProjector : MonoBehaviour
{
    [SerializeField] Light m_ProjectorLight;
    [SerializeField] Transform m_RotatingAxis;
    float m_RotationAmount;
    [SerializeField] float m_RotationSpeed;
    bool m_IsEnabled = false;

    GhostAI m_Ghost;
    ObjectiveManager m_ObjectiveManager;

    private void Start()
    {
        m_ProjectorLight.gameObject.SetActive(false);
        m_RotationAmount = m_RotatingAxis.transform.localRotation.y;
    }
    void Update()
    {
        if (!m_IsEnabled) return;

        m_RotationAmount += Time.deltaTime * m_RotationSpeed;
        m_RotatingAxis.localRotation = Quaternion.Euler(0, 0, m_RotationAmount);

    }

    public void TurnOn()
    {
        m_IsEnabled = true;
        m_ProjectorLight.gameObject.SetActive(true);
    }

    public void TurnOff()
    {
        m_IsEnabled = false;
        m_ProjectorLight.gameObject.SetActive(false);
    }
}
