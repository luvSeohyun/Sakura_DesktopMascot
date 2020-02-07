using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UserData
{
    [Serializable]
    public class RolePos
    {
        public int index;
        public bool enable;
        public Vector3 rootPos;
        public Vector3 rolePos;
        public Quaternion roleRot;
    }
    public string dataVer;
    public bool isTopMost = false;
    public bool isRunOnStartup = false;
    public long updateTime;
    public RolePos[] roles;
}

public class DataModel
{
    static DataModel _instance = new DataModel();
    public static DataModel Instance
    {
        get { return _instance; }
    }
    DataModel() { }

    UserData _data;
    public UserData Data
    {
        get
        {
            if (_data == null)
            {
                Init();
            }
            return _data;
        }
        set
        {
            _data = value;
        }
    }

    void Init()
    {
        if (PlayerPrefs.HasKey(Config.UserDataPath))
        {
            var strData = PlayerPrefs.GetString(Config.UserDataPath);
            _data = JsonUtility.FromJson<UserData>(strData);
        }
        else
        {
            // 首次运行
            _data = new UserData();
        }
        // 更新
        if (_data.dataVer != Config.UserDataVer)
        {
            PlayerPrefs.DeleteAll();
            _data.dataVer = Config.UserDataVer;
            _data.updateTime = DateTime.Now.ToFileTime();
        }
        if (_data.roles == null || _data.roles.Length < (int)Roles.Count || _data.roles[0].rolePos == null)
        {
            _data.roles = new UserData.RolePos[(int)Roles.Count];
            for (int i = 0; i < _data.roles.Length; i++)
            {
                _data.roles[i] = new UserData.RolePos();
                _data.roles[i].index = i;
                _data.roles[i].enable = true;
                _data.roles[i].rolePos = Vector3.zero;
                _data.roles[i].rootPos = Vector3.zero;
                _data.roles[i].roleRot = Quaternion.identity;
            }
        }
        SaveData();
    }

    public void SaveData()
    {
        var strData = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(Config.UserDataPath, strData);
    }

    public void ReloadData()
    {
        if (PlayerPrefs.HasKey(Config.UserDataPath))
        {
            var strData = PlayerPrefs.GetString(Config.UserDataPath);
            Data = JsonUtility.FromJson<UserData>(strData);
        }
    }

    public void UpdateTransformData(GameObject[] roles)
    {
        Debug.Assert(roles.Length > 0 && roles.Length == Data.roles.Length, "TransparentWindow.roleObjs与DataModel.Data长度不匹配");
        for (int i = 0; i < roles.Length; i++)
        {
            if (roles[i] != null)
            {
                Data.roles[i].rootPos = roles[i].transform.position;
                var role = roles[i].transform.GetComponentInChildren<RoleCtrlBase>().transform;
                Data.roles[i].rolePos = role.position;
                Data.roles[i].roleRot = role.rotation;
            }
        }
    }
}

