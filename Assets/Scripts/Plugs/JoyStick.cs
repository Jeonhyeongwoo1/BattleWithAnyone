using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	public RectTransform playPosition;
	RectTransform m_RectTransform;

	[SerializeField] float m_Range;

	public void OnBeginDrag(PointerEventData eventData)
	{
		Vector3 position = GetPosition(eventData.position);
		playPosition.anchoredPosition = position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 position = GetPosition(eventData.position);
		playPosition.anchoredPosition = position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		playPosition.anchoredPosition = Vector2.zero;
	}

	Vector2 GetPosition(Vector3 inputPosition)
	{
		Vector2 input = inputPosition - transform.position;
		return input.magnitude < m_Range ? input : input.normalized * m_Range;
	}

	// Start is called before the first frame update
	void Start()
	{
		m_RectTransform = GetComponent<RectTransform>();
	}

	void OnValueChanged(string key, object o)
	{
		switch (key)
		{
			case nameof(Core.state.playerPosNormalized):
				break;
		}

	}

	void OnEnable()
	{
		Core.state.Listen(nameof(Core.state.playerPosNormalized), OnValueChanged);
	}

	void OnDisable()
	{
		Core.state.Stop(nameof(Core.state.playerPosNormalized), OnValueChanged);
	}

}
