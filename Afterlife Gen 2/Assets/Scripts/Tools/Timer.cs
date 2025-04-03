using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] UnityEvent m_OnTimerComplete;
    [Space]
    [SerializeField] float m_TimerDuration = 5f;

    float m_TimerValue = 0;
    bool m_TimerIsEnabled = false;

    /// <summary>
    /// Set Timer data
    /// </summary>
    public void Start()
    {
        m_TimerValue = m_TimerDuration;
    }

    /// <summary>
    ///  Updates Timers
    /// </summary>
    public void Update()
    {
        if (!m_TimerIsEnabled) return;

        m_TimerValue -= Time.deltaTime;

        if (m_TimerValue <= 0)
        {
            m_TimerValue = m_TimerDuration;
            m_TimerIsEnabled = false;
            m_OnTimerComplete.Invoke();
        }
    }

    /// <summary>
    /// Start the timer
    /// </summary>
    public void StartTimer()
    {
        m_TimerIsEnabled = true;
    }
}
