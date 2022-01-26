using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] Image m_Loading;
    [SerializeField, Range(0, 1)] float m_Duration;
    [SerializeField] AnimationCurve m_Curve;
    UnityAction m_LoadingDone = null;

    public void StartLoading(UnityAction loadingDone = null)
    {
        if (!gameObject.activeSelf) { gameObject.SetActive(true); }

        StartCoroutine(Loading());
        m_LoadingDone = loadingDone;
    }

    public void StopLoading()
    {
        StopAllCoroutines();
        m_LoadingDone?.Invoke();
        gameObject.SetActive(false);
    }

    IEnumerator Loading()
    {
        float elapsed = 0;
        while (true)
        {
            while (elapsed < m_Duration)
            {
                elapsed += Time.deltaTime;
                m_Loading.fillAmount = Mathf.Lerp(0, 1, m_Curve.Evaluate(elapsed / m_Duration));
                yield return null;
            }
            elapsed = 0;
        }
    }
}
