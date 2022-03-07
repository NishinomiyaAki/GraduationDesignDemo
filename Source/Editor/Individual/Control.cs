namespace Editor
{
    public class Control : System.Windows.Forms.ScrollableControl
    {
        private int _ScreenX;
        private int _ScreenY;

        public Control()
        {
            DoubleBuffered = true;
            SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw, true);
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();
            MouseClick += OnMouseClick;
        }

        private void OnMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ContextMenu.GetInstance().HideMenu();
        }

        public int GetScreenX()
        {
            _ScreenX = 0;
            System.Windows.Forms.Control Sender = this;
            while (Sender.Parent != null)
            {
                _ScreenX += Sender.Location.X;
                Sender = Sender.Parent;
            }

            return _ScreenX;
        }

        public int GetScreenY()
        {
            _ScreenY = 0;
            System.Windows.Forms.Control Sender = this;
            while (Sender.Parent != null)
            {
                _ScreenY += Sender.Location.Y;
                Sender = Sender.Parent;
            }

            return _ScreenY;
        }

        public int GetWidth()
        {
            return Width;
        }

        public int GetHeight()
        {
            return Height;
        }

        public void CaptureMouse()
        {
        }

        public void ReleaseMouse()
        {
        }

        public bool IsPointIn(int MouseX, int MouseY)
        {
            if (MouseX > _ScreenX && MouseX < (_ScreenX + Width) && MouseY > _ScreenY && MouseY < (_ScreenY + Height))
            {
                return true;
            }
            return false;
        }

        public void AddChild(Control Child)
        {
            Controls.Add(Child);
        }

        public void ClearChildren()
        {
            Controls.Clear();
        }

        public void SetPosition(int X, int Y, int Width, int Height)
        {
            _ScreenX = X;
            _ScreenY = Y;
            Location = new System.Drawing.Point(X, Y);
            Size = new System.Drawing.Size(Width, Height);
        }
    }
}