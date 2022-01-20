using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SearchRoom : MonoBehaviour
{
	public RoomMenu roomMenu;

	[SerializeField] InputField m_InputedRoomName;
	[SerializeField] Button m_Search;
	[SerializeField] Button m_Close;
	[SerializeField] RectTransform m_Popup;
	[SerializeField, Range(0, 1)] float m_OpenCloseDuration;
	[SerializeField] AnimationCurve m_Curve;

	Vector3 m_OriginPos = new Vector3(0, -519f, 0);

	public void Open(UnityAction done = null)
	{
		gameObject.SetActive(true);
		StartCoroutine(CoUtilize.VLerp((v) => m_Popup.anchoredPosition = v, m_OriginPos, Vector3.zero, m_OpenCloseDuration, done, m_Curve));
	}

	public void Close(UnityAction done = null)
	{
		StartCoroutine(CoUtilize.VLerp((v) => m_Popup.anchoredPosition = v, m_Popup.anchoredPosition, m_OriginPos, m_OpenCloseDuration, () => Closed(done), m_Curve));
	}

	void Closed(UnityAction done)
	{
		m_InputedRoomName.text = null;
		done?.Invoke();
		gameObject.SetActive(false);
	}

	void OnSearchRoom()
	{
		if (string.IsNullOrEmpty(m_InputedRoomName.text))
		{
			Debug.Log("Input Player name");
			return;
		}

		Close();
		roomMenu.SearchRoom(m_InputedRoomName.text);
	}

	// Start is called before the first frame update
	void Start()
	{
		m_Close.onClick.AddListener(()=> Close());
		m_Search.onClick.AddListener(OnSearchRoom);
	}
}
