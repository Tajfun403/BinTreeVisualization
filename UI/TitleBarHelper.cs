﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BinTreeVisualization.UI;

/// <summary>
/// Helps to setup the title bar color of a window.
/// </summary>
internal class TitleBarHelper
{
    // https://stackoverflow.com/a/62811758
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
    {
        if (IsWindows10OrGreater(17763))
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }

        return false;
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }

    /// <summary>
    /// Enable dark mode for the window's title bar
    /// </summary>
    /// <param name="window"></param>
    /// <returns>Whether the change was successful</returns>
    public static bool EnableDarkMode(Window window)
    {
        var wih = new System.Windows.Interop.WindowInteropHelper(window);
        var handle = wih.EnsureHandle();
        var ret = UseImmersiveDarkMode(handle, true);
        Debug.WriteLine($"Dark mode enabled: {ret}");
        return ret;
    }
}
