using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using EditorUI;

namespace CrossEditor
{
    internal class DisplayPanel : System.Windows.Forms.Panel
    {
        private Thread _Thread;
        private bool _bThreadIsAlive = false;
        private int _ThreadID = 0;
        private IntPtr _CurrentHandle;

        public DisplayPanel() : base()
        {
            this.HandleCreated += HandleCreatedEvent;
        }

        protected override void WndProc(ref Message m)
        {
            if (_bThreadIsAlive)
            {
                ImportFunc.PostThreadMessage((uint)_ThreadID, (uint)m.Msg, (uint)m.WParam, (uint)m.LParam);
            }
            base.WndProc(ref m);
        }

        private void HandleCreatedEvent(object sender, EventArgs e)
        {
            _CurrentHandle = this.Handle;
            _Thread = new Thread(ThreadFunc);
            _Thread.Start();
        }

        private int Get_X_LParam(IntPtr LParam)
        {
            return LParam.ToInt32() & 0xffff;
        }

        private int Get_Y_LParam(IntPtr LParam)
        {
            return (LParam.ToInt32() >> 16) & 0xffff;
        }

        private void ThreadFunc()
        {
            if (ImportFunc.Init(_CurrentHandle))
            {
                Console.WriteLine("Init Success");
            }
            Console.WriteLine("start thread");
            _ThreadID = ImportFunc.GetCurrentThreadId();
            _bThreadIsAlive = true;
            Message message = new Message();
            ImportFunc.ResetTimer();
            // WM_NCDESTROY = 0x0082
            while (message.Msg != 0x0082)
            {
                if (ImportFunc.PeekMessage(ref message, IntPtr.Zero, 0, 0, 0x0001))
                {
                    switch (message.Msg)
                    {
                        case 0x0201:    // Left Button Down
                        case 0x0207:    // Middle Button Down
                        case 0x0204:    // Right Button Down
                            ImportFunc.WrappedOnMouseDown(message.WParam, Get_X_LParam(message.LParam), Get_Y_LParam(message.LParam));
                            break;
                        case 0x0202:    // Left Button Up
                        case 0x0208:    // Middle Button Up
                        case 0x0205:    // Right Button Up
                            ImportFunc.WrappedOnMouseUp(message.WParam, Get_X_LParam(message.LParam), Get_Y_LParam(message.LParam));
                            break;
                        case 0x0200:    // Mouse Move
                            ImportFunc.WrappedOnMouseMove(message.WParam, Get_X_LParam(message.LParam), Get_Y_LParam(message.LParam));
                            break;
                    }
                }
                else
                {
                    ImportFunc.Run();
                    // do nothing
                }
            }
        }
    }
}