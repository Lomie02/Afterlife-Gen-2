using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public struct MapProfileMod
{
    public string m_MapName;
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

    int m_CurrentButtonToSet = 0;
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

        foreach (Transform buttons in m_MapButtonParent.transform)
        {
            buttons.GetComponent<Button>().image.sprite = m_MapsList[m_CurrentButtonToSet].m_MapIcon;
            buttons.GetComponent<Button>().GetComponentInChildren<Text>().text = m_MapsList[m_CurrentButtonToSet].m_MapName;
            buttons.GetComponent<Button>().onClick.AddListener(delegate { m_MyView.RPC("ChangeMap", RpcTarget.AllBufferedViaServer, m_CurrentButtonToSet); });
            m_CurrentButtonToSet++;
        }

        m_PlayersInGame.text = "Players: " + PhotonNetwork.PlayerList.Length.ToString();

        if (m_MapDropDownDiff)
            m_MapDropDownDiff.onValueChanged.AddListener(UpdateDifficulty);

        m_CurrentButtonToSet = 0;
        m_MapButtonParent.transform.parent.gameObject.SetActive(false);

        m_MyView.RPC("ChangeMap", RpcTarget.AllBufferedViaServer, 0);
    }


    [PunRPC]
    void ChangeMap(int _index)
    {
        m_ActiveMap = _index;

        m_MapName.text = "Map: " + m_MapsList[_index].m_MapName;
        m_MapIcon.sprite = m_MapsList[_index].m_MapIcon;
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
