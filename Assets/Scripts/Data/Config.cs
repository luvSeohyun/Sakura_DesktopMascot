using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Roles
{
    Sakura,
    Herugaa,
    Count
}
public enum TouchType
{
    Normal,
    Head,
    Special
}

public class Config : MonoBehaviour
{
    public GameObject[] roles;
    public static readonly string UserDataPath = "Sakura_DesktopMascot_UserData";
    public static readonly string UserDataVer = "2.6";// 当大改数据结构时手动更新版本以重新生成数据

}
