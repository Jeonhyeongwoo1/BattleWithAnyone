using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public bool isLogined = false;
	public string userNickName;

	float m_NetworkMaxWaitTime = 10f;
	bool m_IsConnectSuccessed = false;

	public void Log(string message)
	{
		Debug.Log(message);
	}

	public void SetPlayerName(string name)
	{
		PhotonNetwork.LocalPlayer.NickName = name;
		userNickName = PhotonNetwork.LocalPlayer.NickName;
	}

	public void ReqLogin(string id, string password, UnityAction<string> success, UnityAction<string> fail)
	{
		string url = Core.settings.url + "/login";

		WWWForm form = new WWWForm();
		form.AddField("id", id);
		form.AddField("password", password);

		UnityWebRequest request = UnityWebRequest.Post(url, form);
		StartCoroutine(RequestData(request, success, fail));
	}

	IEnumerator RequestData(UnityWebRequest request, UnityAction<string> success, UnityAction<string> fail)
	{

		if (request == null)
		{
			Debug.LogError("Check Request Method!!");
			fail?.Invoke(null);
			yield break;
		}

		yield return request.SendWebRequest();

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("Failed to GetData");
			string error = "Error Code : " + request.responseCode + "/" + request.error;
			fail?.Invoke(error);
			request.Dispose();
			yield break;
		}

		if (request.isDone)
		{
			string data = request.downloadHandler.text;
			if (string.IsNullOrEmpty(request.downloadHandler.text))
			{
				string error = "There is no data";
				fail?.Invoke(error);
				request.Dispose();
				yield break;
			}

			success?.Invoke(data);
			request.Dispose();
		}
	}

	public void ConnectPhotonNetwork(UnityAction done)
	{
		StartCoroutine(ConnectingNetwork(done));
	}

	public void LeaveRoom()
	{
		if (!PhotonNetwork.IsConnected) { return; }
		PhotonNetwork.LeaveRoom();
	}

	public void JoinRoom()
	{
		if (PhotonNetwork.IsConnected)
		{
			Log("Join Room");
		}
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Log("DisConnected");
	}

	public override void OnConnected()
	{
		Log("OnConnected");
	}

	public override void OnConnectedToMaster()
	{
		m_IsConnectSuccessed = true;
		Log("OnConnectedToMaster");
		//        m_UserName.text = PhotonNetwork.LocalPlayer.NickName;
	}

	public override void OnJoinedRoom()
	{
		Log("Joined Room " + PhotonNetwork.CurrentRoom.Players);
		//        GameObject go = PhotonNetwork.Instantiate("player", Vector3.zero, Quaternion.identity);
		//        go.name = PhotonNetwork.LocalPlayer.NickName;
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Log("On JoinRoom Failed");
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Log("Failed Create Room. Return Code : " + returnCode + ", Message : " + message);
	}

	IEnumerator ConnectingNetwork(UnityAction done)
	{
		float elapsed = 0;
		bool o = PhotonNetwork.ConnectUsingSettings();

		if (!o) { Log("Failed to Network Connect. Check User Settings"); }

		while (!m_IsConnectSuccessed)
		{
			if (elapsed > m_NetworkMaxWaitTime && !o)
			{
				Log("Failed to Network Connect");
				yield break;
			}
			elapsed += Time.deltaTime;
			yield return null;
		}

		done?.Invoke();
	}

}
