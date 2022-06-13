using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Member
{
    public string mbr_seq;
	public string mbr_id;
	public string mbr_pwd;
	public string mbr_email;
	public string mbr_nm;
	public string mbr_token;
}

[Serializable]
public class AppleLoginAuth
{
    public string appleUser;
    public string authCode;
    public string idToken;
    public string email;
    public string nickName;
}
