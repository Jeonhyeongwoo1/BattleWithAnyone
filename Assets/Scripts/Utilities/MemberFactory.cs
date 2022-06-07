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
    public LocalMember(string id, string pwd, string email, string name)
    {
        member.mbr_id = id;
        member.mbr_pwd = pwd;
        member.mbr_email = email;
        member.mbr_nm = name;
    }

    public override Member Get() => member;
}

public class MemberFactory
{
    public static Member Get()
    {
        LocalMember member = null;
        switch (Core.settings.profile)
        {
            case XSettings.Profile.local:
                member = new LocalMember("testMaster", "1234", "guddn1234k@naver.com", "testPlayer");
                return member.Get();
            case XSettings.Profile.dev:
                member = new LocalMember("testMaster", "1234", "guddn1234k@naver.com", "testPlayer");
                return member.Get();
            default:
                return null;
        }
    }

}