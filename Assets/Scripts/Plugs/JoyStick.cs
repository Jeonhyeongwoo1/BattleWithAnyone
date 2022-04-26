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
    private Vector2 m_PointerDownPos;
    private Vector3 m_StartPos;

    public void OnBeginDrag(PointerEventData eventData)
	{
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);

        //	Vector3 position = GetPosition(eventData.position);
        //	playPosition.anchoredPosition = position;
    }

	public void OnDrag(PointerEventData eventData)
	{
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
        var delta = position - m_PointerDownPos;

        delta = Vector2.ClampMagnitude(delta, m_Range);
        ((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;

        var newPos = new Vector2(delta.x / m_Range, delta.y / m_Range);
		playPosition.anchoredPosition = newPos;

        //	Vector3 position = GetPosition(eventData.position);
        //	playPosition.anchoredPosition = position;
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

}
