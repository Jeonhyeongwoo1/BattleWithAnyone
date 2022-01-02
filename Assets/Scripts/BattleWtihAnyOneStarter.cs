using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleWtihAnyOneStarter : MonoBehaviour
{
    private void Awake()
    {
        ScenarioDirector.I.OnLoadScenario(nameof(ScenarioLoading));
    }

}
