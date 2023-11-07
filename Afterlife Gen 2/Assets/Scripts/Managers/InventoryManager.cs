using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class InventoryManager : MonoBehaviour
{
    [SerializeField] GameObject m_ObjectsParent;
    int m_ItemSlots = 3;
    [SerializeField] NetworkObject[] m_Items;
    PhotonView m_MyView;
    int m_CurrentSlotSelected = 0;
    int m_PreviousSelected;
    CapsuleCollider m_HitBoxPlayer;
    void Start()
    {
        m_HitBoxPlayer = gameObject.GetComponent<CapsuleCollider>();
        m_MyView = GetComponent<PhotonView>();
        m_Items = new NetworkObject[m_ItemSlots];
    }

    public void CycleInvetory()
    {
        if (!m_MyView.IsMine)
            return;
        m_PreviousSelected = m_CurrentSlotSelected;
        m_CurrentSlotSelected++;

        if (m_CurrentSlotSelected > m_Items.Length - 1)
        {
            m_CurrentSlotSelected = 0;
        }

        UpdateCurrentItemDisplay();
    }

    public bool IsCurrentSlotTaken()
    {
        if (!m_MyView.IsMine)
            return false;

        if (m_Items[m_CurrentSlotSelected] == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void AssignItem(NetworkObject _object)
    {
        if (!m_MyView.IsMine)
            return;
        Physics.IgnoreCollision(m_HitBoxPlayer, _object.GetComponent<Collider>());

        _object.gameObject.transform.parent = m_ObjectsParent.transform;
        _object.transform.position = m_ObjectsParent.transform.position;
        _object.transform.forward = transform.forward;
        _object.SetBodysState(false);

        m_Items[m_CurrentSlotSelected] = _object;
    }

    public void CycleCurrentItemsPower()
    {
        if (!m_MyView.IsMine)
            return;

        m_Items[m_CurrentSlotSelected].CyclePowerStage();
    }

    public void DropItem()
    {
        if (!m_MyView.IsMine)
            return;

        Physics.IgnoreCollision(m_HitBoxPlayer, m_Items[m_CurrentSlotSelected].GetComponent<Collider>(), false);
        m_Items[m_CurrentSlotSelected].SetBodysState(true);
        m_Items[m_CurrentSlotSelected].gameObject.transform.parent = null;

        m_Items[m_CurrentSlotSelected] = null;
    }

    void UpdateCurrentItemDisplay()
    {
        if (m_Items[m_PreviousSelected] != null)
        {
            m_Items[m_PreviousSelected].SetObjectsState(false);
        }
        if (m_Items[m_CurrentSlotSelected] != null)
        {
            m_Items[m_CurrentSlotSelected].SetObjectsState(true);
        }
    }
}
