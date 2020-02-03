using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
// using UnityEngine.UI;

public class TransparentWindow : MonoBehaviour
{
    static TransparentWindow _instance;
    public static TransparentWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TransparentWindow>();
            }
            return _instance;
        }
    }

    // [SerializeField]
    // private Material m_Material;

    [SerializeField]
    int _xOffset = 83;
    [SerializeField]
    int _yOffset = 0;
    [Header("Textures (Unsupported compression!)")]
    [SerializeField]
    Texture2D _enableTexture;
    [SerializeField]
    Texture2D _systemTrayTexture;
    Image _enableImage;
    Icon _systemTrayIcon;

    #region 导入API

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left; //最左坐标
        public int Top; //最上坐标
        public int Right; //最右坐标
        public int Bottom; //最下坐标
    }
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern long GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;

    const uint WS_EX_TOPMOST = 0x00000008;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_EX_TOOLWINDOW = 0x00000080;//隐藏图标

    const int SWP_FRAMECHANGED = 0x0020;
    const int SWP_SHOWWINDOW = 0x0040;
    const int LWA_ALPHA = 2;


    private IntPtr HWND_TOPMOST = new IntPtr(-1);
    private IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    #endregion

    public IntPtr windowHandle
    {
        get
        {
            if (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = FindWindow(null, Application.productName);
            }
            return _windowHandle;
        }
    }

    IntPtr _windowHandle = IntPtr.Zero;
    Vector2Int _offset = Vector2Int.zero;

    void Start()
    {
        // Camera.main.depthTextureMode = DepthTextureMode.Depth;
        // Camera.main.clearFlags = CameraClearFlags.Color;

        if (Application.isEditor) return;

        MARGINS margins = new MARGINS() { cxLeftWidth = -1 };

        LoadIconFile(Application.persistentDataPath);

        //1：忽略大小；2：忽略位置；4：忽略Z顺序
        SetWindowPos(windowHandle, HWND_TOPMOST, _xOffset, _yOffset,
        System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width + 83,
        System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height, 4);

        // Set properties of the window
        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591%28v=vs.85%29.aspx
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT); // 实现鼠标穿透

        // Extend the window into the client area
        //See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx 
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        SetWindowPos(windowHandle, DataModel.Instance.Data.isTopMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, 1 | 2);

        AddSystemTray();

        AutoUpdate();
    }

    private void LateUpdate()
    {

        if (Application.isEditor) return;

        CursorPenetrate();
    }

    bool _isInRoleRect = false;
    Vector2Int _lastMousePos = Vector2Int.zero;

    void CursorPenetrate()
    {
        // 鼠标有位移时打射线，碰到角色则不穿透，否则窗口穿透
        var pos = GetMousePosW2U();
        if (GetMouseMove(_lastMousePos, pos) < 1) return;
        var posV3 = new Vector3(pos.x, pos.y, 0);
        RaycastHit hitInfo;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(posV3), out hitInfo, 100f, LayerMask.GetMask("WindowRect")))
        {
            // 鼠标进入角色范围
            if (!_isInRoleRect)
            {
                var s = GetWindowLong(windowHandle, GWL_EXSTYLE);
                SetWindowLong(windowHandle, GWL_EXSTYLE, (uint)(s & ~WS_EX_TRANSPARENT));
                _isInRoleRect = true;
            }
        }
        else
        {
            // 鼠标移出
            if (_isInRoleRect)
            {
                var s = GetWindowLong(windowHandle, GWL_EXSTYLE);
                SetWindowLong(windowHandle, GWL_EXSTYLE, (uint)(s | WS_EX_TRANSPARENT));
                _isInRoleRect = false;
            }
        }
        _lastMousePos = pos;
    }

    // 获取从Windows桌面空间转换到Unity屏幕空间的鼠标位置
    public Vector2Int GetMousePosW2U()
    {
        RECT rect = new RECT();
        GetWindowRect(windowHandle, ref rect);
        Vector2Int leftBottom = new Vector2Int(rect.Left, rect.Bottom);
        var mousePos = new Vector2Int(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        var screenHeight = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
        leftBottom.y = screenHeight - leftBottom.y;
        mousePos.y = screenHeight - mousePos.y;

        return mousePos - leftBottom;
    }

    float GetMouseMove(Vector2 last, Vector2 current)
    {
        return Mathf.Abs(current.x - last.x) + Mathf.Abs(current.y - last.y);
    }

    SystemTray _icon;
    System.Windows.Forms.ToolStripItem _topmost, _runOnStart;

    // 创建托盘图标、添加选项
    void AddSystemTray()
    {
        _icon = new SystemTray(_systemTrayIcon);
        _topmost = _icon.AddItem("置顶显示", ToggleTopMost);
        _runOnStart = _icon.AddItem("开机自启", ToggleRunOnStartup);
        _icon.AddItem("重置位置", ResetPos);
        _icon.AddSeparator();
        _icon.AddItem("查看文档", OpenDoc);
        _icon.AddItem("检查更新", CheckUpdate);
        _icon.AddSeparator();
        _icon.AddItem("退出", Exit);
        _icon.AddDoubleClickEvent(ShowRole);

        _topmost.Image = DataModel.Instance.Data.isTopMost ? _enableImage : null;
        _runOnStart.Image = DataModel.Instance.Data.isRunOnStartup ? _enableImage : null;
    }

    //! 不支持压缩
    void LoadIconFile(string basePath)
    {
        string enableImagePath = basePath + "/Checkmark.png";
        string iconPath = basePath + "/Icon.png";

        File.WriteAllBytes(enableImagePath, _enableTexture.EncodeToPNG());
        _enableImage = Image.FromFile(enableImagePath);

        File.WriteAllBytes(iconPath, _systemTrayTexture.EncodeToPNG());
        _systemTrayIcon = Icon.FromHandle((new Bitmap(iconPath)).GetHicon());
    }

    void ToggleTopMost()
    {
        bool isTop = !DataModel.Instance.Data.isTopMost;
        DataModel.Instance.Data.isTopMost = isTop;
        DataModel.Instance.SaveData();
        DataModel.Instance.ReloadData();
        SetWindowPos(windowHandle, isTop ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, 1 | 2);
        _topmost.Image = isTop ? _enableImage : null;
    }

    void ToggleRunOnStartup()
    {
        bool isRun = !DataModel.Instance.Data.isRunOnStartup;
        DataModel.Instance.Data.isRunOnStartup = isRun;
        DataModel.Instance.SaveData();
        DataModel.Instance.ReloadData();
        _runOnStart.Image = isRun ? _enableImage : null;
        if (isRun)
        {
            Rainity.AddToStartup();
        }
        else
        {
            Rainity.RemoveFromStartup();
        }

    }

    void Exit()
    {
        _icon.Dispose();
        Application.Quit();
    }

    void ResetPos()
    {
        var cc = FindObjectOfType<CameraCtrl>();
        if (cc)
        {
            cc.ResetPos();
        }
        _icon.ShowNotification(3, "嘤嘤嘤", "嘤嘤怪回到了初始位置");
    }

    void OpenDoc()
    {
        Application.OpenURL("https://github.com/Jason-Ma-233/Sakura_DesktopMascot");
    }

    void AutoUpdate()
    {
        // 写入版本文件以供py读取
        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\" + "Ver.data", Application.version + "\n"
                                                                                   + Application.productName + ".exe");
        // 比较日期，大于一周则调用检查更新
        var lateUpdateDate = DateTime.FromFileTime(DataModel.Instance.Data.updateTime);
        var now = DateTime.Now;
        TimeSpan ts = now - lateUpdateDate;
        if (ts.Days >= 7)
        {
            CheckUpdate();
            DataModel.Instance.Data.updateTime = DateTime.Now.ToFileTime();
            DataModel.Instance.SaveData();
        }
    }

    void CheckUpdate()
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Update.exe";
        p.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory;
        p.Start();
    }

    void ShowRole()
    {
        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, 1 | 2);
        SetWindowPos(windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, 1 | 2);
    }

    // void OnRenderImage(RenderTexture from, RenderTexture to)
    // {
    //     UnityEngine.Graphics.Blit(from, to, m_Material);
    // }
}