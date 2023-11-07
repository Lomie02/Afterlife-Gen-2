using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DataManager : MonoBehaviour
{
    public void SetPlayersSavedSpecialist(string _name)
    {
        PlayerPrefs.SetString("specialist_fav_selection", _name);
    }

    public string GetPlayersSavedSpecialist()
    {
        return PlayerPrefs.GetString("specialist_fav_selection");
    }
}
