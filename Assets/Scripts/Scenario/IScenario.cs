using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IScenario
{
    string scenarioName { get; }
    void OnScenarioPrepare(UnityAction done);
    void OnScenarioStandbyCamera(UnityAction done);
    void OnScenarioStart(UnityAction done);
    void OnScenarioStop(UnityAction done);
}

public class ScenarioTemplate : MonoBehaviour, IScenario
{
    public string scenarioName => typeof(ScenarioTemplate).Name;

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
        done?.Invoke();
    }

    public void OnScenarioStop(UnityAction done)
    {
        done?.Invoke();
    }

}