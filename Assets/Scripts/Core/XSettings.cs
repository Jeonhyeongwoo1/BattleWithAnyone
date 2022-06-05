using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XSettings : MonoBehaviour
{
	public enum Profile
	{
		local,
		dev
	}

	public static bool isCharacterTest = false;
	public string url => profile == Profile.dev ? m_DevUrl : m_LocalUrl;
	public Profile profile;

	public static bool scenarioDirectorLog = true;
	public static bool gamePlayManagerLog = true;
	public static bool modelDirectorLog = true;
	public static bool plugDirectorLog = true;
	public static bool networkManagerLog = true;
	public static bool roomChatLog = true;

    public const string messageCommonPath = "Jsons/Message-common-kr";

    /*
	*   -------------------------------------------------------
	*	PhotonNetwork Resource Path
	*   -------------------------------------------------------
	*/
    public const string chracterPath = "Prefabs/Characters/";
	public const string bulletPath = "Prefabs/Bullet/";
	public const string bulletImpactPath = "Prefabs/BulletImpact/";
	public const string itemPath = "Prefabs/InteractableItem/";
    /*
	*   -------------------------------------------------------
	*/

    /*
	*   -------------------------------------------------------
	*	Network Url Path
	*   -------------------------------------------------------
	*/
    private readonly string m_DevUrl = "http://battlewithanyoneview.cafe24app.com";
    private readonly string m_LocalUrl = "localhost:8001";
    /*
 	*   -------------------------------------------------------
 	*/

}
