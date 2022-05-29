using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchEffect : MonoBehaviour
{
    [SerializeField] Gradient m_Gradient;
    [SerializeField] Image m_Image;
    [SerializeField] float m_ColorChangeMultiplier;
    [SerializeField] float m_Duration;
    [SerializeField] AnimationCurve m_Curve;
    [SerializeField] Vector3 m_ScaleUp = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] float m_Range = 3;

    IEnumerator Showing()
    {
        Vector3 direction = new Vector3(Random.Range(-m_Range, m_Range), Random.Range(-m_Range, m_Range));
        float elapsed = 0;
        while (elapsed < m_Duration)
        {
            elapsed += Time.deltaTime;
            m_Image.color = m_Gradient.Evaluate((elapsed) / m_Duration);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, m_ScaleUp, m_Curve.Evaluate(elapsed / m_Duration));
            transform.position = Vector3.LerpUnclamped(transform.position, transform.position + direction, m_Curve.Evaluate(elapsed / m_Duration));
            yield return null;
        }

        Core.poolManager.Despawn(nameof(TouchEffect), gameObject);
    }

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        StartCoroutine(Showing());
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }

}
