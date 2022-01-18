using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public abstract class BaseModel : MonoBehaviour
{
    public abstract void LoadModel(UnityAction done = null);
    public abstract void UnLoadModel(UnityAction done = null);
}

public class ModelDirector : MonoBehaviour
{
    //Model은 한가지만 로드가 가능하다.
    BaseModel m_LoadedModel = null;
    UnityAction<Scene, LoadSceneMode> m_SceneLoaded;

    public void Ensure(BaseModel baseModel, UnityAction done = null)
    {
        string name = nameof(baseModel);

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.isLoaded)
        {
            Debug.Log("Model :" + name + " Is Loaded");
            return;
        }

        OnLoadSceneAsync(name, done);
    }

    public void OnLoaded(BaseModel baseModel)
    {
        Debug.Log("Model : " + baseModel + "loaded");
        SceneManager.sceneLoaded -= m_SceneLoaded;
        m_LoadedModel = baseModel;
        m_LoadedModel.LoadModel();
    }

    public void Unloaded(BaseModel baseModel)
    {
        Debug.Log("Model : " + baseModel + "Unloaded");
        m_LoadedModel = null;
    }

    public void Load<T>(UnityAction done = null) where T : BaseModel
    {
        string name = typeof(T).Name;
        if (m_LoadedModel != null || m_LoadedModel?.name == name)
        {
            Debug.LogError("Already Loaded Model.. Close previous Loaded Model");
            UnloadScene(m_LoadedModel.name);
        }

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.isLoaded)
        {
            Debug.Log("Model :" + name + " Is Loaded");
            return;
        }

        OnLoadScene(name, done);
    }

    public void Unload<T>(UnityAction done = null) where T : BaseModel
    {
        string name = typeof(T).Name;

        Scene scene = SceneManager.GetSceneByName(name);
        if (!scene.isLoaded)
        {
            Debug.Log("Model :" + name + " Is Unloaded");
            return;
        }

        UnloadScene(name, done);
    }

    public bool HasModel<T>() where T : BaseModel
    {
        return typeof(T).Name == m_LoadedModel.name;
    }

    public BaseModel GetLoadedModel() => m_LoadedModel;

    void UnloadScene(string name, UnityAction done = null)
    {
        if (done != null)
        {
            SceneManager.sceneLoaded += m_SceneLoaded = (s, o) => done?.Invoke();
        }

        StartCoroutine(UnloadingSceneAsync(name, done));
    }

    void OnLoadScene(string name, UnityAction done = null)
    {
        if (done != null)
        {
            SceneManager.sceneLoaded += m_SceneLoaded = (s, o) => done?.Invoke();
        }

        SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    void OnLoadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(OnLoaidngSceneAsync(name, done));
    }

    void UnloadSceneAsync(string name, UnityAction done = null)
    {
        StartCoroutine(UnloadingSceneAsync(name, done));
    }

    IEnumerator UnloadingSceneAsync(string name, UnityAction done = null)
    {
        AsyncOperation async = SceneManager.UnloadSceneAsync(name);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

    IEnumerator OnLoaidngSceneAsync(string name, UnityAction done = null)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!async.isDone) { yield return null; }

        done?.Invoke();
    }

}
