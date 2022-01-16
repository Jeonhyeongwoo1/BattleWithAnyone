    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleWtihAnyOneStarter : MonoBehaviour
{
    private void Start()
    {

        if (ScenarioDirector.scenarioReady)
        {
            Debug.Log("Scnesario is Loaded");
            return;
        }

        Core.scenario.OnLoadScenario(nameof(ScenarioLoading));
    }

}
