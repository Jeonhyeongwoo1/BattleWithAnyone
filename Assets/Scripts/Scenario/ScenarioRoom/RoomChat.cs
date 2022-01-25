using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
using UnityEngine.UI;

public class RoomChat : MonoBehaviour, IChatClientListener
{
	
	[SerializeField] Button m_Clear;
	[SerializeField] Button m_BlockChat;
	[SerializeField] InputField m_InputFieldChat;
	[SerializeField] Button m_Send;
	[SerializeField] Text m_ChannelChat;
	[SerializeField] ScrollRect m_OutputScroll;
	[SerializeField] Vector2 m_DefaultChannelChatSize = new Vector2(568, 27);
	ChatClient m_ChatClient;
	string m_ChannelName;

	public bool IsConnect() => m_ChatClient.CanChat;
	public void ChatDisConnect() => m_ChatClient.Disconnect();

	public void Log(string message)
	{
		Debug.Log(message);
	}

	public void OnSendMessage(string message)
	{
		if (string.IsNullOrEmpty(message)) { return; }

		m_ChatClient.PublishMessage(m_ChannelName, message);
		m_InputFieldChat.text = null;
	}

	public void OnClearChat()
	{
		m_ChannelChat.text = null;
		m_ChannelChat.GetComponent<RectTransform>().sizeDelta = m_DefaultChannelChatSize;
		ChatChannel chatChannel = GetChatChannel(m_ChannelName);
		chatChannel?.ClearMessages();
	}

	public void OnBlockChat()
	{
		ChatChannel chatChannel = GetChatChannel(m_ChannelName);
		if (chatChannel != null)
		{
			chatChannel.BlockChat = !chatChannel.BlockChat;
		}

		m_BlockChat.transform.GetChild(1).gameObject.SetActive(!chatChannel.BlockChat);
	}

	public void Connect(string userName, string roomTitle)
	{
		m_ChatClient = new ChatClient(this);
		m_ChatClient.UseBackgroundWorkerForSending = true;
		m_ChatClient.AuthValues = new Photon.Chat.AuthenticationValues(userName);
		m_ChatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, m_ChatClient.AuthValues);

		m_ChannelName = roomTitle;
	}

	//Chat Listener
	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message) { Log("Level : " + level + "Message : " + message); }

	public void OnDisconnected()
	{
		Log("Phton Chat Disconnected");
		m_InputFieldChat.enabled = false;
	}

	public void OnConnected()
	{
		Log("Photon Chat OnConnectd");

		string[] channels = new string[] { m_ChannelName };

		m_ChatClient.Subscribe(channels, 0);
		m_ChatClient.SetOnlineStatus(ChatUserStatus.Online);
	}

	ChatChannel GetChatChannel(string channelName)
	{
		ChatChannel chatChannel = null;
		bool found = m_ChatClient.TryGetChannel(channelName, out chatChannel);
		return found ? chatChannel : null;
	}

	public void OnChatStateChange(ChatState state)
	{
		Log("Chat State Changed :" + state);

		

	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
		Log("ChannelName : " + channelName);

		if (channelName != m_ChannelName || messages.Length == 0 || senders.Length == 0) { return; }

		ChatChannel chatChannel = GetChatChannel(m_ChannelName);
		if (chatChannel == null)
		{

			return;
		}

		m_ChannelChat.text = chatChannel.ToStringMessages().TrimEnd();
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		Log("Photon Chat OnSubScribed");

		foreach (string channel in channels)
		{
			m_ChatClient.PublishMessage(channel, "Chat Entered " + m_ChatClient.AuthValues.UserId);
		}

		m_ChannelName = channels[0];
		m_ChannelChat.text = null;
		m_InputFieldChat.enabled = true;

		ChatChannel chatChannel = GetChatChannel(channels[0]);
		chatChannel.PlayerName = m_ChatClient.AuthValues.UserId;
	}

	public void OnUnsubscribed(string[] channels)
	{
		m_ChannelChat.text = "Chat Disconnected";
		m_InputFieldChat.enabled = false;
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.Log(user + message);
	}

	public void OnPrivateMessage(string sender, object message, string channelName) { }
	public void OnUserSubscribed(string channel, string user) { Debug.Log("User: " + user); }
	public void OnUserUnsubscribed(string channel, string user) { Debug.Log("User: " + user); }

	// Start is called before the first frame update
	void Start()
	{
		m_InputFieldChat.enabled = false;
		m_Send.onClick.AddListener(() => OnSendMessage(m_InputFieldChat.text));
		m_Clear.onClick.AddListener(OnClearChat);
		m_BlockChat.onClick.AddListener(OnBlockChat);
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		if (m_ChatClient != null)
		{
			m_ChatClient.Service();
		}

	}

}
