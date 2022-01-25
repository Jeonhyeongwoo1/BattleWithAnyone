using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField, Range(0, 1)] float m_ReturningDuration = 0.3f;
    [SerializeField] AnimationCurve m_Curve;
    
    Transform m_Character;
    Vector3 m_OrgRot = new Vector3(0, 180, 0);

    public void SetCharacter(Transform character) => m_Character = character;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!m_Character) { return; }
        StopAllCoroutines();
        m_Character.eulerAngles = m_OrgRot;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_Character) { return; }

        Vector3 delta = eventData.delta;
        m_Character.Rotate(new Vector3(0, -delta.x, 0));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!m_Character) { return; }

        StartCoroutine(ReturningOriginRot());
    }

    IEnumerator ReturningOriginRot()
    {
        float elapsed = 0;
        Vector3 rot = m_Character.eulerAngles;
        Vector3 angle = Vector3.zero;

        while (elapsed < m_ReturningDuration)
        {
            elapsed += Time.deltaTime;
            angle = Vector3.Lerp(rot, m_OrgRot, m_Curve.Evaluate(elapsed / m_ReturningDuration));
            m_Character.eulerAngles = angle;
            yield return null;
        }

        m_Character.eulerAngles = m_OrgRot;

    }
}
