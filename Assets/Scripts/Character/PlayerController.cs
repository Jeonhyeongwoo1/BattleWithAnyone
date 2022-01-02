using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController character;

    [SerializeField] float m_JumpForce = 1f;
    [SerializeField] float m_MoveSpeed = 1f;
    [SerializeField] float m_MaxDistance = 0.5f;

    float m_Gravitiy = 9.8f;
    PlayerActionsScript m_PlayerActions;

    void Awake()
    {
        m_PlayerActions = new PlayerActionsScript();
    }

    void OnEnable()
    {
        m_PlayerActions.Player.Jump.started += Jump;
        m_PlayerActions.Player.Enable();
    }

    void OnDisable()
    {
        m_PlayerActions.Player.Jump.started -= Jump;
        m_PlayerActions.Player.Disable();
    }

    private void Update()
    {
        Vector2 v = m_PlayerActions.Player.Move.ReadValue<Vector2>();

        if (!IsGrounded())
        {
            dirY += Vector3.down * m_Gravitiy * Time.deltaTime;
        }
Debug.Log(character.isGrounded);

        character.Move(v.x * m_MoveSpeed * transform.right + v.y * transform.forward * m_MoveSpeed + dirY);
    }
    
    Vector3 dirY = Vector3.zero;
    
    void Jump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            dirY = Vector3.up * m_JumpForce;
        }
    }

    bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * m_MaxDistance, Color.blue);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, m_MaxDistance))
            return true;    
        else
            return false;
    }

    private void OnDrawGizmos() {

    }

}
