using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Core : Singleton<Core>
{
    static public ScenarioDirector scenario => Core.I?.m_ScenarioDirector;
    static public PlugDirector plugs => Core.I?.m_PlugDirector;

    [SerializeField] ScenarioDirector m_ScenarioDirector = null;
    [SerializeField] PlugDirector m_PlugDirector = null;

    static UnityEvent m_EnsureDone = new UnityEvent();

    // static methods
    static public void Ensure(UnityAction done = null)
    {
        if (Core.I)
        {
            done?.Invoke();
            return;
        }

        if (done != null) { m_EnsureDone.AddListener(done); }

        Scene coreScene = SceneManager.GetSceneByName(nameof(Core));
        if (coreScene == null || !coreScene.IsValid())
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(nameof(Core), LoadSceneMode.Additive);
        }
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != nameof(Core)) { return; }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("XCores Scene loaded");
        m_EnsureDone.Invoke();
        m_EnsureDone.RemoveAllListeners();
    }

    X EnsureCore<X>(ref X behaviour) where X : MonoBehaviour
    {
        if (!behaviour) { behaviour = Core.I.CreateCore<X>(); }

        return behaviour as X;
    }

    // public methods
    private void Awake()
    {
        EnsureAllCores();
    }

    public void EnsureAllCores()
    {
        EnsureCore<ScenarioDirector>(ref m_ScenarioDirector);
        EnsureCore<PlugDirector>(ref m_PlugDirector);

        Debug.Log("Core Initialized.");
    }

    T CreateCore<T>() where T : MonoBehaviour
    {
        var v = new GameObject(typeof(T).Name);
        v.transform.SetParent(this.transform);
        return v.AddComponent<T>();
    }
}