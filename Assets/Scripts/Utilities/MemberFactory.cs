using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MemberBase
{
	protected Member member = new Member();

	public abstract Member Get();
}

public class LocalMember : MemberBase
{
	public LocalMember(string id, string pwd, string telNo, string email, string name)
	{
		member.mbr_id = id;
		member.mbr_pwd = pwd;
		member.mbr_tel_no = telNo;
		member.mbr_email = email;
		member.mbr_nm = name;
	}

	public override Member Get() => member;
}

public class MemberFactory
{
	public static Member Get()
	{
		switch (Core.settings.profile)
		{
			case XSettings.Profile.local:
				LocalMember member = new LocalMember("master", "1234", "010-1234-1234", "guddn1234k@naver.com", "testPlayer");
				return member.Get();
			default:
				return null;
		}
	}

}