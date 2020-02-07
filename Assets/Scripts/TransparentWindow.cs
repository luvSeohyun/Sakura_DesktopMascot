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

    [SerializeField]
    Material _material;

    [Header("Textures (Unsupported compression!)")]
    [SerializeField]
    Texture2D _enableTexture;
    [SerializeField]
    Texture2D _systemTrayTexture;
    Image _enableImage;
    Icon _systemTrayIcon;
    int _xOffset = 0;
    int _yOffset = 0;

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
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    //uFlags = 1：忽略大小；2：忽略位置；4：忽略Z顺序
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint WS_EX_TOOLWINDOW = 0x00000080;//隐藏图标
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

    void Start()
    {

        if (Application.isEditor) return;
        Application.targetFrameRate = 80;

        // 写入icon并读取
        LoadIconFile(Application.persistentDataPath);

        // 设置窗口大小、位置
        SetWindowPosRect();

        // Set properties of the window
        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591%28v=vs.85%29.aspx
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT); // 实现鼠标穿透

        MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
        // Extend the window into the client area
        //See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx 
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, 1 | 2);
        if (!DataModel.Instance.Data.isTopMost)
            SetWindowPos(windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, 1 | 2);


        AddSystemTray();

        AutoUpdate();
    }

    bool _isInRoleRect = false;
    Vector2Int _lastMousePos = Vector2Int.zero;

    public void SetMousePenetrate(bool isPenetrate)
    {
        var s = GetWindowLong(windowHandle, GWL_EXSTYLE);
        if (isPenetrate)
        {
            SetWindowLong(windowHandle, GWL_EXSTYLE, (uint)(s | WS_EX_TRANSPARENT));
        }
        else
        {
            SetWindowLong(windowHandle, GWL_EXSTYLE, (uint)(s & ~WS_EX_TRANSPARENT));
        }
    }

private void LateUpdate() {
        CursorPenetrate();
    }
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

    void SetWindowPosRect()
    {
        // 获取多屏幕的总宽度和最高高度，以及任务栏offset
        int width = 0, height = 0;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            width += screen.Bounds.Width;
            if (screen.Bounds.Height > height)
            {
                height = screen.Bounds.Height;
            }
            if (screen.Primary)
            {
                _xOffset = screen.WorkingArea.X;
                _yOffset = screen.WorkingArea.Y;
            }
        }

        if (width == 0 || height == 0)
            Debug.LogError("获取分辨率失败");

        SetWindowPos(windowHandle, HWND_TOPMOST, _xOffset, _yOffset, width, height, 4);
    }

    float GetMouseMove(Vector2 last, Vector2 current)
    {
        return Mathf.Abs(current.x - last.x) + Mathf.Abs(current.y - last.y);
    }

    #region 托盘

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
        _icon.AddDoubleClickEvent(ToggleTopMost);
        _icon.AddSingleClickEvent(ShowRole);

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
            _icon.ShowNotification(3, "嘤嘤嘤", "嘤嘤怪回到了初始位置");
        }
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
    #endregion

    // 配合TAA，用Gamma减少TAA的Ghosting
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        UnityEngine.Graphics.Blit(src, dest, _material);
    }
}