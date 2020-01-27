using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;

public class Test : MonoBehaviour
{
    class TestJson
    {
        public float testA;
        public long testB;
    }

    // public GameObject[] go;
    // public float test;

    void Start()
    {
        // TestMyJson();
        // EXETest();


    }

    private void Update()
    {
    }


    // public Texture2D texture;
    // public NotifyIcon trayIcon;
    // public System.Windows.Forms.ContextMenu trayMenu;

    // private ContextMenuStrip contextMenuStrip;


    // private void OnEnable()
    // {
    //     AddSystemTray();
    // }

    // private void OnDisable()
    // {
    //     _icon.Dispose();
    // }

    // SystemTray _icon;
    // // 创建托盘图标、添加选项
    // void AddSystemTray()
    // {
    //     _icon = new SystemTray();
    //     // _icon.AddItem("切换置顶显示", () => { Debug.Log("切换置顶显示"); });
    //     // var icon = _icon.trayMenu.Items.Add("icon", SystemTray.Texture2DToImage(texture), null);
    //     // Debug.Log(texture.EncodeToPNG());
    //     // Debug.Log(new MemoryStream(texture.EncodeToPNG()));
    //     Debug.Log(UnityEngine.Application.persistentDataPath);
    //     File.WriteAllBytes(UnityEngine.Application.dataPath + "/Checkmark.png", texture.EncodeToPNG());
    //     Debug.Log(Image.FromFile(UnityEngine.Application.dataPath + "/Checkmark.png"));
    //     _icon.AddSeparator();
    //     _icon.AddItem("查看文档", () => { Debug.Log("查看文档"); });
    //     _icon.AddSeparator();
    //     _icon.AddItem("退出", () => { _icon.ShowNotification(3, "yyy", "exit"); });
    //     _icon.AddDoubleClickEvent(() => { Debug.Log("click"); });
    // }


    void IconEvent(object sender, EventArgs e)
    {
        Debug.Log(66666666666666);
    }
    void IconEvent2(object sender, EventArgs e)
    {
        // Debug.Log(((MenuItem)sender).Text + "    " + e);
        Debug.Log(777);
    }

    void EXETest()
    {
        System.Diagnostics.Process exep = new System.Diagnostics.Process();
        exep.StartInfo.FileName = "calc.exe";
        exep.StartInfo.Arguments = "";
        exep.Start();
    }


    void TestMyJson()
    {
        DateTime t1 = DateTime.Now;
        var str = JsonUtility.ToJson(t1);
        Debug.Log(t1.ToFileTime());
        Debug.Log(DateTime.FromFileTime(t1.ToFileTime()));
        Debug.Log(JsonUtility.ToJson(new TestJson() { testA = 6.66f, testB = 9999999 }));
        var obj = JsonUtility.FromJson<TestJson>("{\"testA\":6.659999847412109}");
        Debug.Log(obj.testA);
        Debug.Log(obj.testB);
    }
}
