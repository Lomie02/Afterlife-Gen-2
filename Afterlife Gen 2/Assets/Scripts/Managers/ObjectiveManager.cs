using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct ObjectiveMod
{
    public string ObjectiveName;
    public string ObjectiveText;

    [Space]
    public string m_Tag;
    [Space]

    public bool m_IsCompleted;
    public bool m_IsActiveObjective;
}

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField]
    ObjectiveMod[] m_Objectives;
    int m_CurrentObjective = 0;

    GameObject m_ObjectiveInterface;
    Text m_ObjectiveTask;
    Text m_ObjectiveTitle;
    PhotonView m_MyPhotonView;

    Animator m_ObjectiveAnimator;
    private void Start()
    {
        m_MyPhotonView = GetComponent<PhotonView>();
    }

    public void SubmitObjectiveInterface(GameObject _obj, Text _ObjectiveTitle, Text _objectiveTaskText)
    {
        m_ObjectiveInterface = _obj;
        m_ObjectiveTask = _objectiveTaskText;
        m_ObjectiveTitle = _ObjectiveTitle;

        m_ObjectiveAnimator = m_ObjectiveInterface.GetComponent<Animator>();
        SetUpObjective();
    }

    void SetUpObjective()
    {
        m_ObjectiveTitle.text = "Objective: " + m_Objectives[m_CurrentObjective].ObjectiveName;
        m_ObjectiveTask.text = m_Objectives[m_CurrentObjective].ObjectiveText;
        m_Objectives[m_CurrentObjective].m_IsActiveObjective = true;
    }

    public int GetCurrentObjectivePosition()
    {
        return m_CurrentObjective;
    }

    public ObjectiveMod GetCurrentObjective()
    {
        return m_Objectives[m_CurrentObjective];
    }

    public void ObjectiveCompleted(int _index)
    {
        if (!m_Objectives[_index].m_IsCompleted)
        {
            m_MyPhotonView.RPC("RPC_UpdateObjectiveData", RpcTarget.All);
        }
    }

    public void ObjectiveCompleted(string _index)
    {
        for (int i = 0; i < m_Objectives.Length; i++)
        {
            if (!m_Objectives[i].m_IsCompleted && m_Objectives[i].m_Tag == _index)
            {
                m_MyPhotonView.RPC("RPC_UpdateObjectiveData", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void RPC_UpdateObjectiveData()
    {
        m_Objectives[m_CurrentObjective].m_IsCompleted = true;
        m_Objectives[m_CurrentObjective].m_IsActiveObjective = false;
        m_CurrentObjective++;

        m_ObjectiveTitle.text = "Objective: " + m_Objectives[m_CurrentObjective].ObjectiveName;
        m_ObjectiveTask.text = m_Objectives[m_CurrentObjective].ObjectiveText;
        m_Objectives[m_CurrentObjective].m_IsActiveObjective = true;

        m_ObjectiveAnimator.SetTrigger("Replay");

        if (m_CurrentObjective >= m_Objectives.Length) // Objectives Completed;
        {
            //TODO: Make something happen
        }
    }
}
