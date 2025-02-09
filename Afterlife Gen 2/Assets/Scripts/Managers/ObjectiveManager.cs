using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

[System.Serializable]
struct ObjectiveMod
{
    public string ObjectiveName;
    public string ObjectiveText;
    public bool m_IsCompleted;
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

    private void Start()
    {
        gameObject.AddComponent<PhotonView>();

    }

    public void SubmitObjectiveInterface(GameObject _obj, Text _ObjectiveTitle, Text _objectiveTaskText)
    {
        m_ObjectiveInterface = _obj;
        m_ObjectiveTask = _objectiveTaskText;
        m_ObjectiveTitle = _ObjectiveTitle;

        SetUpObjective();
    }

    void SetUpObjective()
    {
        m_ObjectiveTitle.text = "Objective: " + m_Objectives[m_CurrentObjective].ObjectiveName;
        m_ObjectiveTask.text = m_Objectives[m_CurrentObjective].ObjectiveText;
    }


    public void ObjectiveCompleted(int _index)
    {
        if (!m_Objectives[_index].m_IsCompleted)
        {
            m_MyPhotonView.RPC("RPC_UpdateObjectiveData", RpcTarget.All);
        }
    }

    public void RPC_UpdateObjectiveData()
    {
        m_Objectives[m_CurrentObjective].m_IsCompleted = true;
        m_CurrentObjective++;
        m_ObjectiveTitle.text = "Objective: " + m_Objectives[m_CurrentObjective].ObjectiveName;
        m_ObjectiveTask.text = m_Objectives[m_CurrentObjective].ObjectiveText;

        if (m_CurrentObjective >= m_Objectives.Length) // Objectives Completed;
        {
            //TODO: Make something happen
        }
    }
}
