using System;
using UnityEngine;
using Cinemachine;

[Serializable]
public class CharacterOption
{
    [Tooltip("체력")]
    public int health;
	[Tooltip("이동속도")]
    public float movingSpeed;
	[Tooltip("구르기 쿨타임")]
    public float rollingCoolTime;
	[Tooltip("점프 강도")]
    public float jumpForce;
	[Tooltip("점프 쿨타임")]
    public float jumpCoolTime;
	[Tooltip("공격 속도")]
    public float attackSpeed;
	[Tooltip("장전 시간")]
    public float reloadTime;
	[Tooltip("총알 갯수")]
    public int bulletCount;
	[Tooltip("다음 총알 발사 대기시간")]
    public float shootingCoolTime;
    [Tooltip("Roll 이동 파워")]
    public float rollingForce;
    public JumpOption jumpOption;
}

[Serializable]
public class JumpOption
{
    [Tooltip("애니메이션 전환")]
    public float animationTransition = 0.15f;
    [Tooltip("Ground LayerMask")]
    public LayerMask groundLayerMask;
}

[Serializable]
public class CameraOption
{
    [Tooltip("카메라")]
    public CinemachineVirtualCamera camera;
    [Tooltip("상하좌우 회전 속도")]
    public float rotationSpeed;
	[Tooltip("올려다 보기 제한")]
    public float lookUpDegree;
	[Tooltip("내려 보기 제한")]
    public float lookDownDegree;
    [Tooltip("Default 회전 값")]
    public Vector3 rotation;

}

[Serializable]
public class AnimationParameters
{
    public readonly string horizontal = "Horizontal";
    public readonly string vertical = "Vertical";
    public readonly string attack = "Attack";
    public readonly string reload = "Reload";
    public readonly string victory = "Victory";
    public readonly string die = "Die";
    public readonly string jump = "Jump";
    public readonly string isGrounded = "IsGrounded";
    public readonly string reloadingSpeed = "ReloadingSpeed";
    public readonly string idle = "Idle";
    public RollingOption rolling;

	[Serializable]
	public class RollingOption
	{
		public readonly string forward = "RollingFWD";
		public readonly string left = "RollingLeft";
		public readonly string right = "RollingRight";
		public readonly string back = "RollingBWD";
	}
}