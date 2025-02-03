using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using Photon.Pun;
public class InventoryManager : MonoBehaviour
{
    [SerializeField] GameObject m_ObjectsParent;
    [SerializeField] NetworkObject[] m_Items;
    [SerializeField] int m_ItemSlots = 3;

    PhotonView m_MyView;
    int m_CurrentSlotSelected = 0;
    int m_PreviousSelected;
    CapsuleCollider m_HitBoxPlayer;

    PlayerController m_PlayerController;
    [Header("First Person")]
    [SerializeField] NetworkObject[] m_ThirdPersonViewItems;

    [SerializeField] Animator m_PlayersAnimation;
    [SerializeField] Transform m_DropLocation;

    float m_ItemLerp;
    int m_WeightLayerForCurrentDevice;
    int m_PreviousDeviceWeight;

    int m_CurrentWeightHandle = 0;
    [SerializeField] ChainIKConstraint[] m_RightArmConstraint;

    [Header("Interface")]
    [SerializeField] Text m_ItemSlowName;
    [SerializeField] Text m_ItemCurrentlyOnSlot;
    int m_MaxSlotsCurrent = 0;

    [Header("Cultist Only")]
    [SerializeField] NetworkObject m_BookObject;

    void Start()
    {
        m_HitBoxPlayer = gameObject.GetComponent<CapsuleCollider>();
        m_MyView = GetComponent<PhotonView>();
        m_Items = new NetworkObject[m_ItemSlots];
        m_PlayerController = GetComponent<PlayerController>();

        for (int i = 0; i < m_ThirdPersonViewItems.Length; i++)
        {
            m_ThirdPersonViewItems[i].RPC_SetObjectState(false);
        }

        m_MaxSlotsCurrent = m_Items.Length;

        if(m_ItemSlowName) m_ItemSlowName.text = "";

        int _convertedSlotNumber = m_CurrentSlotSelected;
        _convertedSlotNumber++;

        m_ItemCurrentlyOnSlot.text = "Slot " + _convertedSlotNumber.ToString() + "/" + m_MaxSlotsCurrent.ToString();

        if (GetComponent<SpecialstAbility>().GetSpecialistType() == SpecialistSelected.Cultist)
        {
            AssignItem(m_BookObject);
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

        if (_object.GetItemID() == ItemID.SantiyPill)
        {
            GetComponent<PlayerController>().RestorePossesion();
            PhotonNetwork.Destroy(_object.GetComponent<PhotonView>());
            return;
        }

        m_Items[m_CurrentSlotSelected] = _object;
        m_ItemSlowName.text = m_Items[m_CurrentSlotSelected].GetItemsName();

        if (m_Items[m_CurrentSlotSelected] != null)
        {
            for (int j = 0; j < m_ThirdPersonViewItems.Length; j++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemID() == m_ThirdPersonViewItems[j].GetItemID())
                {
                    m_ThirdPersonViewItems[j].RPC_SetObjectState(true);
                    m_ThirdPersonViewItems[j].SetPowerState(m_Items[m_CurrentSlotSelected].GetPowerState());

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
            if (m_Items[i] != null && m_Items[i].GetItemID() != ItemID.Perma_Item)
            {
                m_Items[i].SetBodysState(true);
                m_Items[i].RPC_SetObjectState(true);

                m_ItemSlowName.text = "";

                m_Items[i].transform.position = transform.position;

                for (int j = 0; j < m_ThirdPersonViewItems.Length; j++)
                {
                    if (m_Items[i].GetItemID() == m_ThirdPersonViewItems[j].GetItemID())
                    {
                        m_Items[i].SetPowerState(m_ThirdPersonViewItems[j].GetPowerState());
                    }
                }

                m_Items[i] = null;
            }
        }

        for (int i = 0; i < m_ThirdPersonViewItems.Length; i++)
        {
            m_ThirdPersonViewItems[i].RPC_SetObjectState(false);
        }
    }

    public void CycleCurrentItemsPower()
    {
        if (!m_MyView.IsMine)
            return;

        if (m_Items[m_CurrentSlotSelected])
        {
            for (int i = 0; i < m_ThirdPersonViewItems.Length; i++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemID() == m_ThirdPersonViewItems[i].GetItemID())
                {
                    m_ThirdPersonViewItems[i].CyclePowerStage();
                }
            }
        }
    }

    public void DropItem()
    {
        if (!m_MyView.IsMine || m_Items[m_CurrentSlotSelected].GetItemID() == ItemID.Perma_Item)
            return;

        m_Items[m_CurrentSlotSelected].SetBodysState(true);
        m_Items[m_CurrentSlotSelected].RPC_SetObjectState(true);

        m_Items[m_CurrentSlotSelected].transform.position = m_DropLocation.position;

        m_ItemSlowName.text = "";
        for (int j = 0; j < m_ThirdPersonViewItems.Length; j++)
        {
            if (m_Items[m_CurrentSlotSelected].GetItemID() == m_ThirdPersonViewItems[j].GetItemID())
            {
                m_Items[m_CurrentSlotSelected].SetPowerState(m_ThirdPersonViewItems[j].GetPowerState());
            }
        }

        m_Items[m_CurrentSlotSelected] = null;

        for (int i = 0; i < m_ThirdPersonViewItems.Length; i++)
        {
            m_ThirdPersonViewItems[i].RPC_SetObjectState(false);
        }
    }

    void UpdateCurrentItemDisplay()
    {
        if (m_Items[m_PreviousSelected] != null)
        {
            for (int j = 0; j < m_ThirdPersonViewItems.Length; j++)
            {
                if (m_Items[m_PreviousSelected].GetItemID() == m_ThirdPersonViewItems[j].GetItemID())
                {
                    m_ThirdPersonViewItems[j].RPC_SetObjectState(false);
                }
            }

            m_ItemSlowName.text = "";
        }
        if (m_Items[m_CurrentSlotSelected] != null)
        {
            for (int j = 0; j < m_ThirdPersonViewItems.Length; j++)
            {
                if (m_Items[m_CurrentSlotSelected].GetItemID() == m_ThirdPersonViewItems[j].GetItemID())
                {
                    m_PreviousDeviceWeight = m_WeightLayerForCurrentDevice;
                    m_WeightLayerForCurrentDevice = m_ThirdPersonViewItems[j].GetLayerWeight();

                    m_ItemSlowName.text = m_Items[m_CurrentSlotSelected].GetItemsName();
                    if (m_Items[m_CurrentSlotSelected].GetItemID() == ItemID.CamCorder)
                    {
                        m_CurrentWeightHandle = 1;
                    }
                    else if (m_Items[m_CurrentSlotSelected].GetItemID() != ItemID.CamCorder && m_CurrentWeightHandle == 1)
                    {
                        m_MyView.RPC("RPC_LerpItem", RpcTarget.All, 0f);
                        m_CurrentWeightHandle = 0;
                    }



                    if (m_PreviousDeviceWeight != m_WeightLayerForCurrentDevice)
                    {
                        m_ItemLerp = 0;
                        m_MyView.RPC("RPC_LerpItem", RpcTarget.All, 0f);

                        m_PlayersAnimation.SetLayerWeight(m_PreviousDeviceWeight, 0);
                    }

                    m_ThirdPersonViewItems[j].RPC_SetObjectState(true);
                    break;
                }
            }
        }
        int _convertedSlotNumber = m_CurrentSlotSelected;
        _convertedSlotNumber++;

        if(m_ItemCurrentlyOnSlot) m_ItemCurrentlyOnSlot.text = "Slot " + _convertedSlotNumber.ToString() + "/" + m_MaxSlotsCurrent.ToString();
    }

    [PunRPC]
    public void RPC_LerpItem(float _index)
    {
        m_ItemLerp = Mathf.Lerp(m_ItemLerp, _index, 5 * Time.deltaTime);
        m_RightArmConstraint[m_CurrentWeightHandle].weight = m_ItemLerp;
    }

    public void Update()
    {
        if (m_MyView.IsMine)
        {
            UpdateItemLerp();
        }
    }

    public void UpdateItemLerp()
    {
        if (m_Items[m_CurrentSlotSelected])
        {
            if (m_PlayerController.IsSprinting())
                m_MyView.RPC("RPC_LerpItem", RpcTarget.All, 0f);
            else
                m_MyView.RPC("RPC_LerpItem", RpcTarget.All, 1f);
        }
        else
        {
            m_MyView.RPC("RPC_LerpItem", RpcTarget.All, 0f);
        }

    }


    public ItemID GetCurrentItemsId()
    {
        return m_Items[m_CurrentSlotSelected].GetItemID();
    }

    public void DestroyCurrentItem()
    {
        if (m_Items[m_CurrentSlotSelected].GetItemID() == ItemID.Perma_Item) return;

        PhotonNetwork.Destroy(m_Items[m_CurrentSlotSelected].gameObject);
        m_ItemSlowName.text = "";
        m_Items[m_CurrentSlotSelected] = null;

        for (int i = 0; i < m_ThirdPersonViewItems.Length; i++)
        {
            m_ThirdPersonViewItems[i].RPC_SetObjectState(false);
        }
    }
}
