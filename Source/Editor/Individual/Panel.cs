namespace CrossEditor
{
    public class Panel : Control
    {
        public delegate void OnPaintEvent(Control Sender);

        public delegate void OnMouseEvent(Control Sender, int MouseX, int MouseY, ref bool bContinue);

        public event OnPaintEvent PaintEvent;

        public event OnMouseEvent LeftMouseDownEvent;

        public event OnMouseEvent LeftMouseUpEvent;

        public event OnMouseEvent RightMouseDownEvent;

        public event OnMouseEvent RightMouseUpEvent;

        public event OnMouseEvent MouseMoveEvent;

        private System.Timers.Timer Timer;

        public void Initialize()
        {
            Timer = new System.Timers.Timer(1000 / 60);
            Timer.Elapsed += Tick;
            Timer.AutoReset = true;
            Timer.SynchronizingObject = MainUI.GetInstance().MainWindow;
        }

        private void Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invalidate();
        }

        public void SetAutoRefresh(bool bAutoRefresh)
        {
            if (bAutoRefresh)
            {
                Timer.Start();
            }
            else
            {
                Timer.Stop();
            }
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseMoveEvent != null)
            {
                bool bContinue = false;
                MouseMoveEvent(this, e.X, e.Y, ref bContinue);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && LeftMouseDownEvent != null)
            {
                bool bContinue = false;
                LeftMouseDownEvent(this, e.X, e.Y, ref bContinue);
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right && RightMouseDownEvent != null)
            {
                bool bContinue = false;
                RightMouseDownEvent(this, e.X, e.Y, ref bContinue);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && LeftMouseUpEvent != null)
            {
                bool bContinue = false;
                LeftMouseUpEvent(this, e.X, e.Y, ref bContinue);
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right && RightMouseUpEvent != null)
            {
                bool bContinue = false;
                RightMouseUpEvent(this, e.X, e.Y, ref bContinue);
            }
            base.OnMouseUp(e);
        }

        public void SetImage(object Image)
        {
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (Width != 0 && Height != 0)
            {
                if (PaintEvent != null)
                {
                    Graphics2D.GetInstance().Graphics = e.Graphics;
                    PaintEvent(this);
                }
            }
            base.OnPaint(e);
        }
    }
}