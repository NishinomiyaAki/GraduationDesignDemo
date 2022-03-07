using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace CrossEditor
{
    class ImportFunc
    {
        [DllImport("DisplayPanel.dll", EntryPoint = "Init")]
        public static extern bool Init(IntPtr hWnd);

        [DllImport("DisplayPanel.dll", EntryPoint = "Run")]
        public static extern void Run();

        [DllImport("DisplayPanel.dll", EntryPoint = "ResetTimer")]
        public static extern void ResetTimer();

        [DllImport("DisplayPanel.dll", EntryPoint = "WrappedOnMouseDown")]
        public static extern void WrappedOnMouseDown(IntPtr wParam, int x, int y);

        [DllImport("DisplayPanel.dll", EntryPoint = "WrappedOnMouseUp")]
        public static extern void WrappedOnMouseUp(IntPtr wParam, int x, int y);

        [DllImport("DisplayPanel.dll", EntryPoint = "WrappedOnMouseMove")]
        public static extern void WrappedOnMouseMove(IntPtr wParam, int x, int y);

        [DllImport("DisplayPanel.dll", EntryPoint = "GetBoxPosition")]
        public static extern void GetPosition(ref float x, ref float y, ref float z);

        [DllImport("DisplayPanel.dll", EntryPoint = "GetPlayerPosition")]
        public static extern void GetPlayerPosition(ref float x, ref float y, ref float z);

        [DllImport("DisplayPanel.dll", EntryPoint = "MoveBoxTo")]
        public static extern void SetPosition(float x, float y, float z);

        [DllImport("DisplayPanel.dll", EntryPoint = "GetMovementArea")]
        public static extern void GetMovementArea(ref float xMin, ref float yMin, ref float xMax, ref float yMax);

        [DllImport("user32.dll", EntryPoint = "PostThreadMessage", SetLastError = true)]
        public static extern bool PostThreadMessage(uint idThread, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        public static extern bool PeekMessage(ref Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        public static extern int GetMessage(ref Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        public static extern int TranslateMessage(ref Message lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        public static extern int DispatchMessage(ref Message lpMsg);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
        public static extern int GetCurrentThreadId();
    }
}
