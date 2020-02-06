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
    private void Start()
    {
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            Debug.Log(screen.DeviceName);
            Debug.Log(screen.WorkingArea);
            Debug.Log(screen.Bounds);
            Debug.Log(screen.BitsPerPixel);
            Debug.Log(screen.Primary);
        }
    }
}
