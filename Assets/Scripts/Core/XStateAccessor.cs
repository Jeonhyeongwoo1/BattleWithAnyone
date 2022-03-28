using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class XState
{

    public MapPreferences mapPreferences { get => m_MapPreferences; set { m_MapPreferences = value; Set(nameof(MapPreferences), value); } }

    public Transform masterCharacter { get => m_MasterCharacter; set { m_MasterCharacter = value; } }
    public Transform playerCharacter { get => m_PlayerCharecter; set { m_PlayerCharecter = value; } }

}