using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoForm : MonoBehaviour
{
    [SerializeField] Text m_CharacterName;
    [SerializeField] Text m_GunType;
    [SerializeField] Text m_Speed;
    [SerializeField] Text m_Power;
    [SerializeField] Text m_BulletCount;

    public void SetInfo(string name, BulletAttribute.BulletType gunType, float speed, int power, int bulletCount)
    {
        m_CharacterName.text = name;
        m_GunType.text = string.Format(MessageCommon.Get("room.guntype"), GetType(gunType));
        m_Speed.text = string.Format(MessageCommon.Get("room.speed"), speed);
        m_Power.text = string.Format(MessageCommon.Get("room.power"), power);
        m_BulletCount.text = string.Format(MessageCommon.Get("room.bulletcount"), bulletCount);
    }

    string GetType(BulletAttribute.BulletType gunType)
    {
        switch (gunType)
        {
            case BulletAttribute.BulletType.Shotgun:
                return MessageCommon.Get("gun.shotgun");
            case BulletAttribute.BulletType.Cannon:
            case BulletAttribute.BulletType.Junkrat:
                return MessageCommon.Get("gun.boom");
            case BulletAttribute.BulletType.Pistol:
                return MessageCommon.Get("gun.pistol");
            case BulletAttribute.BulletType.Fireball:
                return MessageCommon.Get("gun.magic");
            default:
                return "";
        }
    }

}
