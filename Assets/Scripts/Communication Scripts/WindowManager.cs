using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField]
    private string windowTitle = "Bullet Physics ExampleBrowser using OpenGL3+ [btgl] Release build";

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOP = IntPtr.Zero;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;

    public void EmbedWindow()
    {
        IntPtr unityWindowHandle = FindWindow(null, "Unity Window Title"); // Replace with your Unity window title

        if (unityWindowHandle == IntPtr.Zero)
        {
            Debug.Log("Unity window not found.");
            return;
        }

        IntPtr pyBulletWindowHandle = IntPtr.Zero;
        int waitTime = 0;
        int maxWaitTime = 10000; // 10 seconds
        int checkInterval = 500; // Check every 500 milliseconds

        while (pyBulletWindowHandle == IntPtr.Zero && waitTime < maxWaitTime)
        {
            pyBulletWindowHandle = FindWindow(null, windowTitle);
            if (pyBulletWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(checkInterval);
                waitTime += checkInterval;
            }
        }

        if (pyBulletWindowHandle != IntPtr.Zero)
        {
            SetParent(pyBulletWindowHandle, unityWindowHandle);
            SetWindowPos(pyBulletWindowHandle, HWND_TOP, 0, 0, 800, 600, SWP_NOMOVE | SWP_NOSIZE); // Adjust the size and position
            Debug.Log("Window embedded successfully.");
        }
        else
        {
            Debug.Log("Window not found within the maximum wait time.");
        }
    }
}
