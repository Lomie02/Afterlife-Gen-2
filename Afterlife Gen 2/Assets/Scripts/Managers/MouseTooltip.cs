using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class MouseTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] UnityEvent m_OnHoverEntry;
    [SerializeField] UnityEvent m_OnHoverExit;
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        m_OnHoverEntry.Invoke();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        m_OnHoverExit.Invoke();
    }
}