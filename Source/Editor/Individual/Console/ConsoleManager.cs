using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class ConsoleManager
    {
        public static ConsoleManager Instance = new ConsoleManager();
        private Panel _Panel;
        private System.Windows.Forms.Control _Container;

        public static ConsoleManager GetInstance()
        {
            return Instance;
        }

        public void Initialize(System.Windows.Forms.Control Control)
        {
            _Container = Control;
            _Panel = new Panel();
            _Panel.Initialize();
            _Panel.SetAutoRefresh(true);
            _Panel.Location = new System.Drawing.Point(0, 0);
            _Panel.Size = _Container.Size;
            _Panel.MouseWheel += OnMouseWheel;
            _Panel.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GENERAL_BACK_COLOR);

            _Container.Controls.Add(_Panel);
        }

        private void OnMouseWheel(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            Panel Self = Sender as Panel;
            if (e.Delta < 0 && Self.Location.Y >= (Self.Parent.Height - Self.Height))
            {
                int Y = Self.Location.Y - 60;
                Y = Y >= (Self.Parent.Height - Self.Height) ? Y : (Self.Parent.Height - Self.Height);
                Self.Location = new System.Drawing.Point(Self.Location.X, Y);
                Self.Refresh();
            }
            if (e.Delta > 0 && Self.Location.Y <= 0)
            {
                int Y = Self.Location.Y + 60;
                Y = Y <= 0 ? Y : 0;
                Self.Location = new System.Drawing.Point(Self.Location.X, Y);
                Self.Refresh();
            }
        }

        public void ScrollToBottom()
        {
            if(_Panel.Height > _Container.Height)
            {
                _Panel.Location = new System.Drawing.Point(0, _Container.Height - _Panel.Height);
            }
        }

        public void AddLogItem(LogItem LogItem)
        {
            _Panel.AddChild(LogItem);
            DoLayout();
            ScrollToBottom();
        }

        public void ClearLog()
        {
            _Panel.ClearChildren();
            _Panel.Location = new System.Drawing.Point(0, 0);
            DoLayout();
        }

        public void DoLayout()
        {
            int ChildHeight = 0;
            foreach (LogItem Child in _Panel.Controls)
            {
                Child.DoLayout(ref ChildHeight);
            }
            _Panel.Width = _Container.Width;
            _Panel.Height = Math.Max(_Container.Height, ChildHeight);
        }
    }
}
