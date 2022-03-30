using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class XState
{

	public MapPreferences mapPreferences { get => m_MapPreferences; set { m_MapPreferences = value; Set(nameof(MapPreferences), value); } }

	public Transform masterCharacter { get => m_MasterCharacter; set { m_MasterCharacter = value; } }
	public Transform playerCharacter { get => m_PlayerCharecter; set { m_PlayerCharecter = value; } }

	public int masterWinCount { get => m_MasterWinCount; set { m_MasterWinCount = value; Set(nameof(masterWinCount), value); } }
	public int playerWinCount { get => m_PlayerWinCount; set { m_PlayerWinCount = value; Set(nameof(playerWinCount), value); } }
	public Vector2 playerPosNormalized { get=> m_PlayerPosNormalized; set { m_PlayerPosNormalized = value; Set(nameof(playerPosNormalized), value);}}

}