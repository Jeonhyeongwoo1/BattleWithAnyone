using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class MessageCommon : MonoBehaviour
{

	private static Dictionary<string, string> messages;
	private const string path = "Jsons/Message-common-kr";

	public static string Get(string key)
	{
		if (!messages.ContainsKey(key)) { return null; }

		return messages[key];
	}

	void Awake()
	{
		var jsonData = Resources.Load<TextAsset>(path);

		if (jsonData == null)
		{
			Debug.LogError("Message Json Data를 찾을 수 없습니다.");
			return;
		}

		messages = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData.text);
	}
}
