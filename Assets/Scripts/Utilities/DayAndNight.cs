using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DayAndNight : MonoBehaviour
{
    //오직 빛에 관해서만 컨트롤한다.
    [SerializeField] Light m_Sun;
    [SerializeField] float m_LightIntensity;
    [SerializeField] Vector3 m_NightAxisX = new Vector3(-90, 30, 0);
    [SerializeField] Vector3 m_DayAxisX = new Vector3(30, 30, 0);
    [SerializeField] AnimationCurve m_BlendCurve;

    public void SetDayOrNight(bool isDay)
    {
        m_Sun.transform.rotation = Quaternion.Euler(isDay ? m_DayAxisX : m_NightAxisX);
        m_Sun.intensity = isDay ? 1 : m_LightIntensity;
    }

    public void DayNightCycleAsync(float duration, float ratio, bool useRepeat)
    {
        StartCoroutine(Blending(duration, ratio, useRepeat));
    }

    IEnumerator Blending(float duration, float ratio, bool useRepeat)
    {
        float elapsed = 0;
        
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

    }


}
