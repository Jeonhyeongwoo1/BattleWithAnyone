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

	public string url => profile == Profile.dev ? m_DevUrl : m_LocalUrl;
	public Profile profile;

    /*
	*   ---------------------------- ---------------------------
	*	PhotonNetwork Resource Path
	*   ---------------------------- ---------------------------
	*/
    public static readonly string chracterPath = "Prefabs/";
    /*
	*   ---------------------------- ---------------------------
	*/

    private	readonly string m_DevUrl = "http://battlewithanyoneview.cafe24app.com";
	private	readonly string m_LocalUrl = "localhost:8001";


}
