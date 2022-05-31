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

	public static bool scenarioDirectorLog = false;
	public static bool gamePlayManagerLog = false;
	public static bool modelDirectorLog = false;
	public static bool plugDirectorLog = false;
	public static bool networkManagerLog = false;
	public static bool roomChatLog = false;

    public const string messageCommonPath = "Jsons/Message-common-kr";

    /*
	*   -------------------------------------------------------
	*	PhotonNetwork Resource Path
	*   -------------------------------------------------------
	*/
    public const string chracterPath = "Prefabs/Character/";
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
