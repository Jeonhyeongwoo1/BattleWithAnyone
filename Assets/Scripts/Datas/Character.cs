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
    public float bulletCount;
	[Tooltip("다음 총알 발사 대기시간")]
    public float shootingDelay; 
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
    public string horizontal = "Horizontal";
    public string vertical = "Vertical";
    public string attack = "Attack";
    public string reload = "Reload";
    public string victory = "Victory";
    public string die = "Die";
    public RollingOption rolling;

	[Serializable]
	public class RollingOption
	{
		public string forward = "RollingFWD";
		public string left = "RollingLeft";
		public string right = "RollingRight";
		public string back = "RollingBWD";
	}
}