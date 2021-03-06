using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T s_Instance = null;
    static bool s_IsQuitting = false;

    public static T I
    {
        get
        {
            if (s_IsQuitting) { return null; }

            if (!s_Instance)
            {
                s_Instance = (T)FindObjectOfType(typeof(T));
            }

            return s_Instance;
        }
    }

    public Singleton()
    {
        if (s_Instance)
        {
            Debug.LogWarning("SingletonBehaviour<" + typeof(T).Name + "> instance already exist! ");
        }
    }

    private void OnApplicationQuit()
    {
        s_IsQuitting = true;
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }
}