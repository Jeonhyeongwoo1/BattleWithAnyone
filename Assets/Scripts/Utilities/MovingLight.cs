using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingLight : MonoBehaviour
{
    public bool use;
    public bool isLooping;

    [SerializeField] Vector3 m_StartPos;
    [SerializeField] Vector3 m_EndPos;

    // Start is called before the first frame update
    void Start()
    {
        if (use)
        {
            StartCoroutine(Moving());
        }
    }

    IEnumerator Moving()
    {
        float elapsed = 0;
        bool isStartPos = true;
        float duration = 3;
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        while (isLooping)
        {
            Vector3 end = isStartPos ? m_EndPos : m_StartPos;
            Vector3 start = transform.localPosition;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            isStartPos = !isStartPos;
            elapsed = 0;
            yield return waitForSeconds;
        }

    }

}
