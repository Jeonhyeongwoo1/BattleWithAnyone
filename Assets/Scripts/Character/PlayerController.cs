using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	public CharacterController character;

	[SerializeField] Animator m_Animator;
	[SerializeField] float m_JumpForce;
	[SerializeField] float m_MoveSpeed;
	[SerializeField] float m_MaxDistance = 0.5f;
	[SerializeField] float m_AnimationSmoothTime = 0.1f;
    [SerializeField] float m_AnimationPlayTransition = 0.15f;

	float m_Gravitiy = 9.8f;
	PlayerActionsScript m_PlayerActions;
	Vector3 dirY = Vector3.zero;
	Vector2 m_CurrentAnimationBlendVector;
	Vector2 m_AnimationVelocity;

	bool m_IsAttacking = false;
	bool m_IsRolling = false;

    int jumpAnimation; //Jump 애니메이션 이름이 포함된다
    Vector2 m_Direction;

	void Jump(InputAction.CallbackContext obj)
	{
		if (IsGrounded())
		{
			m_Animator.CrossFade(jumpAnimation, m_AnimationPlayTransition);
			dirY = Vector3.up * m_JumpForce;
			Vector2 v = m_PlayerActions.Player.Move.ReadValue<Vector2>();
            m_Direction = v;
        }
	}

	void Attack(InputAction.CallbackContext obj)
	{
		m_IsAttacking = !m_IsAttacking;

		if (m_IsAttacking)
		{
			StartCoroutine(Attacking());
		}

	}

	void Reload(InputAction.CallbackContext obj)
	{
		Debug.Log("Reload");
		m_Animator.SetTrigger("Reload");
	}

	void Rolling(InputAction.CallbackContext obj)
	{
		if (m_IsRolling) { return; }
		m_IsRolling = true;


	}

	IEnumerator Attacking()
	{
		m_Animator.SetBool("Attack", true);
		while (m_IsAttacking)
		{
			yield return null;
		}

		m_Animator.SetBool("Attack", false);
	}

	bool IsGrounded()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, m_MaxDistance))
			return true;
		else
			return false;
	}

	private void Update()
	{
		Vector2 v = m_PlayerActions.Player.Move.ReadValue<Vector2>();
		m_CurrentAnimationBlendVector = v != Vector2.zero ? Vector2.SmoothDamp(m_CurrentAnimationBlendVector, v, ref m_AnimationVelocity, m_AnimationSmoothTime) : Vector2.zero;

		bool isGround = IsGrounded();

		if (!isGround || !character.isGrounded)
		{
			dirY += Vector3.down * m_Gravitiy * Time.deltaTime;
            v = m_Direction;
        }

		m_Animator.SetFloat("Horizontal", isGround ? m_CurrentAnimationBlendVector.x : 0);
		m_Animator.SetFloat("Vertical", isGround ? m_CurrentAnimationBlendVector.y : 0);
		character.Move((v.x * m_MoveSpeed * transform.right + v.y * transform.forward * m_MoveSpeed + dirY) * Time.deltaTime);

	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void LateUpdate()
	{
	}

	void Awake()
	{
		m_PlayerActions = new PlayerActionsScript();
	}

	void OnEnable()
	{
		m_PlayerActions.Player.Jump.started += Jump;
		m_PlayerActions.Player.Attack.started += Attack;
		m_PlayerActions.Player.Reload.started += Reload;
		m_PlayerActions.Player.Rolling.started += Rolling;
		m_PlayerActions.Player.Enable();

        jumpAnimation = Animator.StringToHash("Jump");
	}

	void OnDisable()
	{
		m_PlayerActions.Player.Jump.started -= Jump;
		m_PlayerActions.Player.Attack.started -= Attack;
		m_PlayerActions.Player.Reload.started -= Reload;
		m_PlayerActions.Player.Rolling.started -= Rolling;
		m_PlayerActions.Player.Disable();
	}

}
