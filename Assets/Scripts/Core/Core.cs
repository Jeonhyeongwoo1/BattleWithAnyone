using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Core : Singleton<Core>
{
    static public ScenarioDirector scenario => Core.I?.m_ScenarioDirector;
    static public PlugDirector plugs => Core.I?.m_PlugDirector;
    static public ModelDirector models => Core.I?.m_ModelDirector;
    static public NetworkManager networkManager => Core.I?.m_NetworkManager;
    static public XSettings settings => Core.I?.m_XSettings;
    static public GamePlayManager gameManager => Core.I?.m_GamePlayManager;
    static public ObjectPoolManager poolManager => Core.I?.m_ObjectPoolManager;
    static public XState state => Core.I?.m_XState;
    static public XEvent xEvent => Core.I?.m_XEvent;
    static public AudioManager audioManager => Core.I?.m_AudioManager;
    static public LanguageLocalization language => Core.I?.m_Language;

    [SerializeField] ModelDirector m_ModelDirector = null;
    [SerializeField] ScenarioDirector m_ScenarioDirector = null;
    [SerializeField] PlugDirector m_PlugDirector = null;
    [SerializeField] NetworkManager m_NetworkManager = null;
    [SerializeField] XSettings m_XSettings = null;
    [SerializeField] GamePlayManager m_GamePlayManager = null;
    [SerializeField] ObjectPoolManager m_ObjectPoolManager = null;
    [SerializeField] XState m_XState = null;
    [SerializeField] XEvent m_XEvent = null;
    [SerializeField] AudioManager m_AudioManager = null;
    [SerializeField] LanguageLocalization m_Language = null;

    static UnityEvent m_EnsureDone = new UnityEvent();

    static public void Ensure(UnityAction done = null)
    {
        if (Core.I)
        {
            done?.Invoke();
            return;
        }

        if (done != null) { m_EnsureDone.AddListener(done); }
        // Core Loaded가 항상되어있다.
        /*
                Scene coreScene = SceneManager.GetSceneByName(nameof(Core));
                if (coreScene == null || !coreScene.IsValid())
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    SceneManager.LoadScene(nameof(Core), LoadSceneMode.Additive);
                }
        */
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
        EnsureCore<NetworkManager>(ref m_NetworkManager);
        EnsureCore<ModelDirector>(ref m_ModelDirector);
        EnsureCore<XSettings>(ref m_XSettings);
        EnsureCore<ObjectPoolManager>(ref m_ObjectPoolManager);
        EnsureCore<XState>(ref m_XState);
        EnsureCore<XEvent>(ref m_XEvent);
        EnsureCore<AudioManager>(ref m_AudioManager);
        EnsureCore<LanguageLocalization>(ref m_Language);

        Debug.Log("Core Initialized.");
    }

    T CreateCore<T>() where T : MonoBehaviour
    {
        var v = new GameObject(typeof(T).Name);
        v.transform.SetParent(this.transform);
        return v.AddComponent<T>();
    }
}