using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterControl : MonoBehaviour
{
    public Animator animator;

    [SerializeField] Transform m_FirstPersonCam;
    [SerializeField] CharacterController controller;

    [SerializeField, Range(0, 1)] float m_TouchSensiblityX = 0.25f;
    [SerializeField, Range(0, 1)] float m_TouchSensiblityY = 0.25f;
    [SerializeField, Range(0, 10)] float m_CharacterSpeed = 0;
    [SerializeField] float m_AxisYMax = 320;
    [SerializeField] float m_AxisYMin = 40;

    int rightFingerId = -1;
    int leftFingerId = -1;
    
    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    float screenHalf = Screen.width / 2;
                    if (rightFingerId == -1 && screenHalf < touch.position.x)
                    {
                        rightFingerId = touch.fingerId;
                    }
                    else if (leftFingerId == -1 && screenHalf > touch.position.x)
                    {
                        leftFingerId = touch.fingerId;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == rightFingerId)
                    {
                        rightFingerId = -1;
                    }
                    else if (touch.fingerId == leftFingerId)
                    {
                        leftFingerId = -1;
                    }
                    break;
                case TouchPhase.Moved:
                    if (touch.fingerId == rightFingerId)
                    {
                        Rotation(touch);
                    }

                    if (touch.fingerId == leftFingerId)
                    {
                        Move(touch);
                    }
                    break;
            }
        }
    }

    void Move(Touch touch)
    {
        if (touch.position.x > Screen.width / 2) { return; }
        Vector2 value = touch.deltaPosition.normalized * m_CharacterSpeed * Time.deltaTime;
        controller.Move(transform.right * value.x + transform.forward * value.y);
    }

    void Rotation(Touch touch)
    {
        if (touch.position.x < Screen.width / 2) { return; }
        float deltaX = touch.deltaPosition.x * 180 / Screen.width;
        float deltaY = -touch.deltaPosition.y * 90 / Screen.height;
        float x = m_FirstPersonCam.eulerAngles.y + deltaX * m_TouchSensiblityX;
        float y = m_FirstPersonCam.eulerAngles.x + deltaY * m_TouchSensiblityY;
        float clampedY = y;
        if (y >= m_AxisYMin && y <= m_AxisYMax && deltaY < 0) { clampedY = m_AxisYMax; }
        else if (y >= m_AxisYMin && y <= m_AxisYMax && deltaY > 0) { clampedY = m_AxisYMin; }

        transform.Rotate(Vector3.up, touch.deltaPosition.x * m_TouchSensiblityX);
        m_FirstPersonCam.rotation = Quaternion.Euler(clampedY, transform.localEulerAngles.y, 0);
    }

}
