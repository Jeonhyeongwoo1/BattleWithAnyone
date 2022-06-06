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
    
    public int bulletCount { get => m_BulletCount; set { m_BulletCount = value; Set(nameof(bulletCount), value); } }
    public int health { get => m_Health; set { m_Health = value; Set(nameof(health), value); } }

    public float playerRotSensitivity { get => m_PlayerRotSensitivity; set { m_PlayerRotSensitivity = value; Set(nameof(playerRotSensitivity), value); } }

    //Not Observered
    public int totalDamangeReceived { get => m_TotalDamangeReceived; set { m_TotalDamangeReceived = value; } }
    public int totalTakeDamange { get => m_TotalTakeDamange; set { m_TotalTakeDamange = value; } }

}