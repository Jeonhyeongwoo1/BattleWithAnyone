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

	public string url => profile == Profile.dev ? devUrl : localUrl;
	public Profile profile;

	readonly string devUrl = "http://battlewithanyoneview.cafe24app.com";
	readonly string localUrl = "localhost:8001";

}
