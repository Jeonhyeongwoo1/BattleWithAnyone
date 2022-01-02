using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupBlink : MonoBehaviour
{
    [SerializeField] CanvasGroup m_CanvasGroup;
    [SerializeField] bool m_AutoStart = true;

    [SerializeField, Range(0, 1)] float m_BlinkDuration = 1f;
    [SerializeField, Range(0, 3)] float m_WaitTime = 1f;
    [SerializeField, Range(0, 1)] float m_Max = 1f;
    [SerializeField, Range(0, 1)] float m_Min = 0.3f;
    [SerializeField] AnimationCurve m_Curve;

    public void StopBlink()
    {
        StopAllCoroutines();
        m_CanvasGroup.alpha = 1;
    }

    public void StartBlink()
    {
        StartCoroutine(Blinking());
    }

    IEnumerator Blinking()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_WaitTime);
            yield return CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, m_Max, m_Min, m_BlinkDuration, null, m_Curve);
            yield return new WaitForSeconds(m_WaitTime);
            yield return CoUtilize.Lerp((v) => m_CanvasGroup.alpha = v, m_Min, m_Max, m_BlinkDuration, null, m_Curve);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();

        if (m_AutoStart)
        {
            StartBlink();
        }
    }

}
