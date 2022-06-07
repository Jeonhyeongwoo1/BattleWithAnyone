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
        m_GunType.text = string.Format(Core.language.GetNotifyMessage("room.guntype"), GetType(gunType));
        m_Speed.text = string.Format(Core.language.GetNotifyMessage("room.speed"), speed);
        m_Power.text = string.Format(Core.language.GetNotifyMessage("room.power"), power);
        m_BulletCount.text = string.Format(Core.language.GetNotifyMessage("room.bulletcount"), bulletCount);
    }

    string GetType(BulletAttribute.BulletType gunType)
    {
        switch (gunType)
        {
            case BulletAttribute.BulletType.Shotgun:
                return Core.language.GetNotifyMessage("gun.shotgun");
            case BulletAttribute.BulletType.Cannon:
            case BulletAttribute.BulletType.Junkrat:
                return Core.language.GetNotifyMessage("gun.boom");
            case BulletAttribute.BulletType.Pistol:
                return Core.language.GetNotifyMessage("gun.pistol");
            case BulletAttribute.BulletType.Fireball:
                return Core.language.GetNotifyMessage("gun.magic");
            default:
                return "";
        }
    }

}
