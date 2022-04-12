using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ScenarioLoading : MonoBehaviour, IScenario
{
    public string scenarioName => nameof(ScenarioLoading);

    public TextAnimator textAnimator;
    public Slider loadingbar;
    public TextMeshProUGUI loadingbarTxt;

    [SerializeField, Range(0, 4)] float m_MinLoadingTime = 3f;
    [SerializeField] AnimationCurve m_Curve;

	string[] m_Plugables = { nameof(MapSettings), nameof(Popups), nameof(XTheme) };
	int m_LoadedCount = 0;

    public void OnScenarioPrepare(UnityAction done)
    {
        BlockSkybox blockSkybox = BattleWtihAnyOneStarter.GetBlockSkybox();
        blockSkybox.UseBlockSkybox(false, () =>
        {
            done?.Invoke();
            blockSkybox.gameObject.SetActive(false);
        });
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

    void OnLoadScenarioLogin()
    {
        Core.scenario.OnLoadScenario(nameof(ScenarioLogin));
    }

    public void OnScenarioStop(UnityAction done)
    {
        done?.Invoke();
    }

    void StartLoading()
    {
        textAnimator.StartAnimation();
        StartCoroutine(Loading(OnLoadScenarioLogin));
        LoadPlugables();
    }

    void LoadPlugables()
    {
        foreach (string v in m_Plugables)
        {
            StartCoroutine(LoadingPlug(v, () => m_LoadedCount++));
        }
    }

    IEnumerator LoadingPlug(string name, UnityAction done = null)
    {
        bool isDone = false;
        Core.plugs.OnLoadSceneAsync(name, () => isDone = true);
        while (!isDone) { yield return null; }
        done?.Invoke();
    }

    IEnumerator Loading(UnityAction done)
    {
        float elapsed = 0;
        float duration = 0, ensureMinTime = 0;

        while (duration < 1)
        {
            elapsed += Time.deltaTime;
            ensureMinTime = m_Curve.Evaluate(elapsed / m_MinLoadingTime);
            duration = Mathf.Min(ensureMinTime, (m_LoadedCount / m_Plugables.Length));
            loadingbar.value = Mathf.Lerp(0, 100, duration);
            loadingbarTxt.text = "Loading..." + loadingbar.value.ToString("0") + "%";
            yield return null;
        }

        done?.Invoke();
    }

    private void Start()
    {
        Core.Ensure(() => Core.scenario.OnLoadedScenario(this));
    }

    private void Awake()
    {
        Core.Ensure(() => Core.scenario.OnScenarioAwaked(this));
    }
}
