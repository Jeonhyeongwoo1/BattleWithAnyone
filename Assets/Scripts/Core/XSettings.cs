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

	public static bool isCharacterTest = true;
	public string url => profile == Profile.dev ? m_DevUrl : m_LocalUrl;
	public Profile profile;

    public const string messageCommonPath = "Jsons/Message-common-kr";

    /*
	*   -------------------------------------------------------
	*	PhotonNetwork Resource Path
	*   -------------------------------------------------------
	*/
    public const string chracterPath = "Prefabs/";
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
