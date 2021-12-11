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
        playPosition.anchoredPosition = GetPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        playPosition.anchoredPosition = GetPosition(eventData.position);
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

    // Update is called once per frame
    void Update()
    {

    }
}
