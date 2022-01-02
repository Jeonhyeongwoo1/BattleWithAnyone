using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ScenarioLoading : MonoBehaviour, IScenario
{
    public string scenarioName => typeof(ScenarioLoading).Name;

    public TextAnimator textAnimator;
    public Slider loadingbar;
    public TextMeshProUGUI loadingbarTxt;

    [SerializeField, Range(0, 4)] float m_MinLoadingTime = 3f;
    [SerializeField] AnimationCurve m_Curve;

    public void OnScenarioPrepare(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStandbyCamera(UnityAction done)
    {
        done?.Invoke();
    }

    public void OnScenarioStart(UnityAction done)
    {
        StartLoading();
        done?.Invoke();
    }

    void OnLoadScenarioHome()
    {
        ScenarioDirector.I.OnLoadScenario(nameof(ScenarioHome));
    }

    public void OnScenarioStop(UnityAction done)
    {
        done?.Invoke();
    }

    void StartLoading()
    {
        textAnimator.StartAnimation();
        StartCoroutine(Loading(OnLoadScenarioHome));
    }

    IEnumerator Loading(UnityAction done)
    {
        float elapsed = 0;

        while (elapsed < m_MinLoadingTime)
        {
            elapsed += Time.deltaTime;
            loadingbar.value = Mathf.Lerp(0, 100, m_Curve.Evaluate(elapsed / m_MinLoadingTime));
            loadingbarTxt.text = "Loading..." + loadingbar.value.ToString("0") + "%";
            yield return null;
        }

        done?.Invoke();
    }

    private void Awake()
    {
        ScenarioDirector.I.OnLoadedScenario(this);
    }
}
