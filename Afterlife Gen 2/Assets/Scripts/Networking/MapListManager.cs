using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;

[System.Serializable]
public struct MapProfileMod
{
    public string m_MapName;
    public int m_ArrayPosition;
    public string m_MapSceneName;

    public Sprite m_MapIcon;
}

public enum MapGameModeDifficulty
{
    Map_Easy = 0,
    Map_Medium,
    Map_Hard,
    Map_RedMoon,
}

public class MapListManager : MonoBehaviourPunCallbacks
{
    [SerializeField] MapProfileMod[] m_MapsList;

    MapGameModeDifficulty m_MapDifficulty;

    [SerializeField] GameObject m_MapButtonParent;

    [Space]

    [SerializeField] Image m_MapIcon;
    [SerializeField] Text m_MapName;
    [SerializeField] Text m_PlayersInGame;
    [SerializeField] Text m_Difficulty;

    // Map Buttons
    [SerializeField] Dropdown m_MapDropDownDiff;

    int m_ActiveMap = 0;
    PhotonView m_MyView;
    void Start()
    {
        m_MyView = GetComponent<PhotonView>();
        if (!PhotonNetwork.IsMasterClient) return;

        // Get All Buttons
        if (!m_MapIcon)
            m_MapIcon = GameObject.Find("Map_Icon").GetComponent<Image>();

        if (!m_MapName)
            m_MapName = GameObject.Find("Map_Name").GetComponent<Text>();

        if (!m_PlayersInGame)
            m_PlayersInGame = GameObject.Find("Map_Players").GetComponent<Text>();

        if (!m_Difficulty)
            m_Difficulty = GameObject.Find("Map_Difficulty").GetComponent<Text>();

        // if (!m_MapDropDownDiff)
        //     m_MapDropDownDiff = GameObject.Find("Map_DropdownDiff").GetComponent<Dropdown>();

        m_MapButtonParent = GameObject.Find("Map_Buttons");

        Button[] ButtonChildren = m_MapButtonParent.GetComponentsInChildren<Button>();

        for (int i = 0; i < ButtonChildren.Length; i++)
        {
            ButtonChildren[i].image.sprite = m_MapsList[i].m_MapIcon;
            ButtonChildren[i].GetComponentInChildren<Text>().text = m_MapsList[i].m_MapName;

            if (i == 0)
            {
                ButtonChildren[i].onClick.AddListener(() => ChangeMap_Manor());
            }
            else if (i == 1)
            {
                ButtonChildren[i].onClick.AddListener(() => ChangeMap_Carnival());
            }
            Debug.Log("Maps Assigned: " + i.ToString());
        }

        m_PlayersInGame.text = "Players: " + PhotonNetwork.PlayerList.Length.ToString();

        if (m_MapDropDownDiff)
            m_MapDropDownDiff.onValueChanged.AddListener(UpdateDifficulty);

        PrepareMapChange(0);

        m_MapButtonParent.transform.parent.gameObject.SetActive(false);
    }

    public void PrepareMapChange(int _index)
    {
        m_MyView.RPC("ChangeMap", RpcTarget.AllBufferedViaServer, _index);
    }

    public void ChangeMap_Manor()
    {
        m_MyView.RPC("ChangeMap", RpcTarget.AllBufferedViaServer, 0);

    }

    public void ChangeMap_Carnival()
    {

        m_MyView.RPC("ChangeMap", RpcTarget.AllBufferedViaServer, 1);
    }

    [PunRPC]
    void ChangeMap(int _index)
    {
        if (_index >= m_MapsList.Length) { Debug.Log(" Map Index: " + _index.ToString() + " Does not exist!"); return; }

        m_ActiveMap = _index;

        m_MapName.text = "Map: " + m_MapsList[_index].m_MapName;
        m_MapIcon.sprite = m_MapsList[_index].m_MapIcon;

        Debug.Log("Updated Map: " + m_MapsList[m_ActiveMap].m_MapName);
    }

    public MapProfileMod GetMapProfileData()
    {
        return m_MapsList[m_ActiveMap];
    }

    public void UpdateDiffuclty(int _index)
    {
        m_MyView.RPC("UpdateDifficulty", RpcTarget.AllBufferedViaServer, _index);
    }

    [PunRPC]
    void UpdateDifficulty(int _index)
    {
        m_MapDifficulty = (MapGameModeDifficulty)_index;

        switch (m_MapDifficulty)
        {
            case MapGameModeDifficulty.Map_Easy:
                m_Difficulty.text = "Difficulty: Easy.";
                break;

            case MapGameModeDifficulty.Map_Medium:
                m_Difficulty.text = "Difficulty: Medium.";
                break;

            case MapGameModeDifficulty.Map_Hard:
                m_Difficulty.text = "Difficulty: Hard.";
                break;

            case MapGameModeDifficulty.Map_RedMoon:
                m_Difficulty.text = "Difficulty: Red Moon.";
                break;
        }
    }


}
