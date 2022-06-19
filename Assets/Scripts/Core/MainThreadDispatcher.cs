using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.Networking;
using System.Threading;
using System.Threading.Tasks;

public class MainThreadDispatcher : MonoBehaviour
{
    private readonly Queue<Action> m_ExecuteQueue = new Queue<Action>();
    

    public void Request(IEnumerator request)
    {
        m_ExecuteQueue.Enqueue(() => StartCoroutine(request));
    }

    public void Request(Action action)
    {
        m_ExecuteQueue.Enqueue(() => StartCoroutine(ActionWrapper(action)));
    }

    public Task RequestAsync(Action action)
    {
        var tsc = new TaskCompletionSource<bool>();

        void WrappedAction()
        {
            try
            {
                action();
                tsc.TrySetResult(true);
            }
            catch (Exception e)
            {
                tsc.TrySetException(e);
                Debug.LogError(e);
            }
        }

        Request(ActionWrapper(WrappedAction));
        return tsc.Task;
    }

    IEnumerator ActionWrapper(Action action)
    {
        action?.Invoke();
        yield return null;
    }


    // Update is called once per frame
    void Update()
    {
        lock (m_ExecuteQueue)
        {
            while (m_ExecuteQueue.Count > 0)
            {
                m_ExecuteQueue.Dequeue()?.Invoke();
            }
        }
    }
}
