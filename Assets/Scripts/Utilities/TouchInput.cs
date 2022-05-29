using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    public static bool use;

    [SerializeField] GameObject m_TouchEffect;

    private void OnEnable()
    {
        ObjectPool objectPool = new ObjectPool(20, 25, transform, m_TouchEffect);
        Core.poolManager?.Add(nameof(TouchEffect), objectPool);
        Core.poolManager?.Initialize(nameof(TouchEffect));
    }

    private void OnDisable()
    {
        Core.poolManager?.Remove(nameof(TouchEffect));
    }

    // Update is called once per frame
    void Update()
    {
        if (use && Input.GetMouseButtonDown(0))
        {
            GameObject go = Core.poolManager.Spawn(nameof(TouchEffect));
            go.transform.position = Input.mousePosition;
            go.SetActive(true);
        }
    }
}
