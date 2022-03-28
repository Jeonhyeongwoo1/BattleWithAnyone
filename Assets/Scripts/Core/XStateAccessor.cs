using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class XState
{

    public string masterName { get => m_MasterName; set { m_MasterName = value; Set(nameof(masterName), value); } }
    public string playerName { get => m_PlayerName; set { m_PlayerName = value; Set(nameof(playerName), value); } }

    public MapPreferences mapPreferences { get => m_MapPreferences; set { m_MapPreferences = value; Set(nameof(MapPreferences), value); } }

    public Transform masterCharacter { get => m_MasterCharacter; set { m_MasterCharacter = value; } }
    public Transform playerCharacter { get => m_PlayerCharecter; set { m_PlayerCharecter = value; } }

}