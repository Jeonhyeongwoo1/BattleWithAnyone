using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour
{
    [SerializeField] Vector2 m_DefaultLeftStickPos = new Vector2(180, 143);
    [SerializeField] RectTransform m_JoyStick;

    PlayerActionScripts m_PlayerActions;
    float m_ScreenHalf;

    void OnMovePosition(Vector2 value)
    {
        if (value.x > m_ScreenHalf) { return; }
        transform.position = value;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            transform.position = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_JoyStick.anchoredPosition = m_DefaultLeftStickPos;
        }
#endif
    }

    void ReturnOriginJoystickPosition(string key, object o)
    {
        switch (key)
        {
            case "Move.Stop":
                m_JoyStick.anchoredPosition = m_DefaultLeftStickPos;
                break;
        }
    }

    void OnEnable()
    {
        m_PlayerActions?.Player.Enable();
        Core.xEvent?.Watch("Move.Stop", ReturnOriginJoystickPosition);
    }

    void OnDisable()
    {
        m_PlayerActions?.Player.Disable();
        Core.xEvent?.Stop("Move.Stop", ReturnOriginJoystickPosition);
    }


    private void Awake()
    {
        m_ScreenHalf = Screen.width * 0.5f;
        m_PlayerActions = new PlayerActionScripts();
        m_PlayerActions.Player.Touch.performed += ctx => OnMovePosition(ctx.ReadValue<Vector2>());
    }

}
