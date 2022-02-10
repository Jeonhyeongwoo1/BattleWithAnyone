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

	public class User
	{
		string id;
		string password;

		public User(string id, string password)
		{
			this.id = id;
			this.password = password;
		}

		public (string id, string password) Get() => (id, password);
	}

	public User user
	{
		get
		{
			User user = Profile.local == profile ? new User("master2", "1234") : new User(null, null);
			return user;
		}
	}

	public string url => profile == Profile.dev ? m_DevUrl : m_LocalUrl;
	public Profile profile;

	readonly string m_DevUrl = "http://battlewithanyoneview.cafe24app.com";
	readonly string m_LocalUrl = "localhost:8001";

}
