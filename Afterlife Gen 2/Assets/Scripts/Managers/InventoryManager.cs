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

    PlayerController m_PlayerController;
    [Header("First Person")]
    [SerializeField] NetworkObject[] m_FirstPersonObjects;
    [SerializeField] Animator m_PlayersAnimation;

    float m_ItemLerp;
    int m_WeightLayerForCurrentDevice;
    int m_PreviousDeviceWeight;
    void Start()
    {
        m_HitBoxPlayer = gameObject.GetComponent<CapsuleCollider>();
        m_MyView = GetComponent<PhotonView>();
        m_Items = new NetworkObject[m_ItemSlots];
        m_PlayerController = GetComponent<PlayerController>();
        for (int i = 0; i < m_FirstPersonObjects.Length; i++)
        {
            m_FirstPersonObjects[i].RPC_SetObjectState(false);
        }
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

    [PunRPC]
    public void RPC_ReTransform(NetworkObject _object, Transform _newParent)
    {
        Physics.IgnoreCollision(m_HitBoxPlayer, _object.GetComponent<Collider>(), true);
        _object.gameObject.transform.parent = _newParent.transform;
        _object.transform.position = _newParent.transform.position;
        _object.transform.forward = _newParent.forward;
    }

    [PunRPC]
    public void RPC_UnParent(NetworkObject _object)
    {
        _object.SetBodysState(true);
        _object.gameObject.transform.parent = null;
        Physics.IgnoreCollision(m_HitBoxPlayer, _object.GetComponent<Collider>(), false);
    }

    public void AssignItem(NetworkObject _object)
    {
        if (!m_MyView.IsMine)
            return;

        _object.GetComponent<PhotonView>().RequestOwnership();

        if (_object.GetItemID() == ItemID.Useable)
        {
            _object.UseItem();
            PhotonNetwork.Destroy(_object.GetComponent<PhotonView>());
            return;
        }

        m_Items[m_CurrentSlotSelected] = _object;

        if (m_Items[m_CurrentSlotSelected] != null)
        {
            for (int j = 0; j < m_FirstPersonObjects.Length; j++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemID() == m_FirstPersonObjects[j].GetItemID())
                {
                    m_FirstPersonObjects[j].RPC_SetObjectState(true);
                    m_FirstPersonObjects[j].SetPowerState(m_Items[m_CurrentSlotSelected].GetPowerState());
                }
            }
        }

        m_WeightLayerForCurrentDevice = m_Items[m_CurrentSlotSelected].GetLayerWeight();
        m_Items[m_CurrentSlotSelected].RPC_SetObjectState(false);
    }

    public void DropItemsOnPerson()
    {
        if (!m_MyView.IsMine)
            return;

        for (int i = 0; i < m_ItemSlots; i++)
        {
            if (m_Items[i] != null)
            {
                m_Items[i].SetBodysState(true);
                m_Items[i].RPC_SetObjectState(true);

                m_Items[i].transform.position = transform.position;

                for (int j = 0; j < m_FirstPersonObjects.Length; j++)
                {
                    if (m_Items[i].GetItemID() == m_FirstPersonObjects[j].GetItemID())
                    {
                        m_Items[i].SetPowerState(m_FirstPersonObjects[j].GetPowerState());
                    }
                }

                m_Items[i] = null;
            }
        }

        for (int i = 0; i < m_FirstPersonObjects.Length; i++)
        {
            m_FirstPersonObjects[i].RPC_SetObjectState(false);
        }
    }

    public void CycleCurrentItemsPower()
    {
        if (!m_MyView.IsMine)
            return;

        if (m_Items[m_CurrentSlotSelected])
        {
            for (int i = 0; i < m_FirstPersonObjects.Length; i++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemsID() == m_FirstPersonObjects[i].GetItemsID())
                {
                    m_FirstPersonObjects[i].CyclePowerStage();
                }
            }
        }
    }

    public void DropItem()
    {
        if (!m_MyView.IsMine)
            return;

        m_Items[m_CurrentSlotSelected].SetBodysState(true);
        m_Items[m_CurrentSlotSelected].RPC_SetObjectState(true);

        m_Items[m_CurrentSlotSelected].transform.position = transform.position;


        for (int j = 0; j < m_FirstPersonObjects.Length; j++)
        {
            if (m_Items[m_CurrentSlotSelected].GetItemID() == m_FirstPersonObjects[j].GetItemID())
            {
                m_Items[m_CurrentSlotSelected].SetPowerState(m_FirstPersonObjects[j].GetPowerState());
            }
        }

        m_Items[m_CurrentSlotSelected] = null;

        for (int i = 0; i < m_FirstPersonObjects.Length; i++)
        {
            m_FirstPersonObjects[i].RPC_SetObjectState(false);
        }
    }

    void UpdateCurrentItemDisplay()
    {
        if (m_Items[m_PreviousSelected] != null)
        {
            for (int j = 0; j < m_FirstPersonObjects.Length; j++)
            {
                if (m_Items[m_PreviousSelected].GetItemID() == m_FirstPersonObjects[j].GetItemID())
                {
                    m_FirstPersonObjects[j].RPC_SetObjectState(false);
                }
            }
        }
        if (m_Items[m_CurrentSlotSelected] != null)
        {
            for (int j = 0; j < m_FirstPersonObjects.Length; j++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemID() == m_FirstPersonObjects[j].GetItemID())
                {
                    m_PreviousDeviceWeight = m_WeightLayerForCurrentDevice;
                    m_WeightLayerForCurrentDevice = m_FirstPersonObjects[j].GetLayerWeight();

                    if (m_PreviousDeviceWeight != m_WeightLayerForCurrentDevice)
                    {
                        m_ItemLerp = 0;
                        m_PlayersAnimation.SetLayerWeight(m_PreviousDeviceWeight, 0);
                    }

                    m_FirstPersonObjects[j].RPC_SetObjectState(true);
                    break;
                }
            }
        }
    }

    void LerpItem(float _index)
    {
        m_ItemLerp = Mathf.Lerp(m_ItemLerp, _index, 5 * Time.deltaTime);
        m_PlayersAnimation.SetLayerWeight(m_WeightLayerForCurrentDevice, m_ItemLerp);
    }

    public void Update()
    {
        if (m_Items[m_CurrentSlotSelected])
        {
            if (m_PlayerController.IsTacticalSprinting())
                LerpItem(0);
            else
                LerpItem(1);
        }
        else
        {
            LerpItem(0);
        }
    }

    public ItemID GetCurrentItemsId()
    {
        return m_Items[m_CurrentSlotSelected].GetItemID();
    }

    public void DestroyCurrentItem()
    {
        PhotonNetwork.Destroy(m_Items[m_CurrentSlotSelected].gameObject);

        m_Items[m_CurrentSlotSelected] = null;

        for (int i = 0; i < m_FirstPersonObjects.Length; i++)
        {
            m_FirstPersonObjects[i].RPC_SetObjectState(false);
        }
    }
}
