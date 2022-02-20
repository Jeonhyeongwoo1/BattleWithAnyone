using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenarioDirector : MonoBehaviour
{
    public static bool scenarioReady { get; set; }

    public bool logSetting = true;
    IScenario m_PreviousScenario = null;
    IScenario m_CurrentScenario = null;

    public void OnScenarioAwaked(IScenario scenario)
    {
        if (scenarioReady)
        {
            Debug.LogError(scenario.scenarioName + " 에디터에 시나리오가 중복 활성화 되어있어 이 시나리오를 Unload합니다.");
            SceneManager.UnloadSceneAsync(scenario.scenarioName);
            return;
        }

        scenarioReady = true;
    }

    public void OnLoadedScenario(IScenario scenario)
    {
        scenarioReady = false;
        if (m_CurrentScenario == null)
        {
            m_CurrentScenario = scenario;
            OnScenarioPrepare(null, scenario);
            return;
        }

        m_PreviousScenario = m_CurrentScenario;
        OnScenarioPrepare(m_PreviousScenario, scenario);
    }

    public IScenario GetGurScenario() => m_CurrentScenario;
    public string GetCurScenarioName() => m_CurrentScenario.scenarioName;

    void OnScenarioPrepare(IScenario from, IScenario to)
    {
        if (to != null)
        {
            Log("On Scenario Prepare");
            to.OnScenarioPrepare(() => OnScenarioStandbyCamera(from, to));
        }
    }

    void OnScenarioStandbyCamera(IScenario from, IScenario to)
    {
        if (to != null)
        {
            Log("On Scenario Standby Camera");
            to.OnScenarioStandbyCamera(() => OnScenarioStart(from, to));
        }
    }

    void OnScenarioStart(IScenario from, IScenario to)
    {
        if (to != null)
        {
            Log("On Scenario Start");
            to.OnScenarioStart(() => OnScenarioStarted(from, to));
        }
    }

    void OnScenarioStarted(IScenario from, IScenario to)
    {
        Log("On Scenario Started : " + to.scenarioName);
        m_CurrentScenario = to;

        if (from != null)
        {
            OnScenarioStop(from);
        }
    }

    void OnScenarioStop(IScenario scenario)
    {
        if (scenario != null)
        {
            Log("On Scenario Stop :" + scenario.scenarioName);
            scenario.OnScenarioStop(() => UnloadScenario(scenario));
            m_PreviousScenario = null;
        }
    }

    public void UnloadScenario(IScenario scenario, UnityAction unloaded = null)
    {
        Scene scene = SceneManager.GetSceneByName(scenario.scenarioName);
        if (scene == null)
        {
            Log("Already Unloaded Scenario");
            return;
        }

        Log("Unload : " + scenario);
        StartCoroutine(UnloadScnarioAsync(scenario.scenarioName, unloaded));
    }

    public void OnLoadScenario(string scenario, UnityAction loaded = null)
    {
        Scene scene = SceneManager.GetSceneByName(scenario);
        if (scene.name != null)
        {
            Log("Already Loaded Scenario");
            return;
        }

        Log("Load : " + scenario);
        StartCoroutine(LoadScnarioAsync(scenario, loaded));
    }

    IEnumerator LoadScnarioAsync(string scenario, UnityAction done)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scenario, LoadSceneMode.Additive);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

    IEnumerator UnloadScnarioAsync(string scenario, UnityAction done)
    {
        AsyncOperation async = SceneManager.UnloadSceneAsync(scenario);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

    void Log(string message)
    {
        if (logSetting)
        {
            Debug.Log(message);
        }
    }

}
