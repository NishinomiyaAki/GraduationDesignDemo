using EditorUI;
using System;

namespace Editor
{
    internal class MenuItem : Control
    {
        private string _Text;
        private Menu _Menu;
        private const int _SpanX = 20;
        private const int _SpanY = 3;
        private Color _BackColor;

        public delegate void OnMouseEvent(MenuItem Sender);

        public event OnMouseEvent ClickedEvent;

        public void SetText(string Text)
        {
            _Text = Text;
        }

        public void SetMenu(Menu Menu)
        {
            _Menu = Menu;
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (ClickedEvent != null)
            {
                ContextMenu.GetInstance().HideMenu();
                ClickedEvent(this);
            }
            
            if (_Menu == null)
            {
                base.OnMouseClick(e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _BackColor = Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU;
            Refresh();
            if (_Menu != null)
            {
                ContextMenu.GetInstance().ShowSubMenu(_Menu, Width, Location.Y);
            }
            else
            {
                ContextMenu.GetInstance().HideSubMenu(this);
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _BackColor = Color.EDITOR_UI_GENERAL_BACK_COLOR;
            Refresh();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            Graphics.Clear(this.BackColor);

            System.Drawing.SolidBrush Brush = Graphics2D.GetInstance().CreateSolidBrushFromColor(_BackColor);
            Graphics.FillRectangle(Brush, 0, 0, this.Width, this.Height);

            Brush = Graphics2D.GetInstance().CreateSolidBrushFromColor(Color.EDITOR_UI_GENERAL_TEXT_COLOR);
            Graphics.DrawString(_Text, GraphicsHelper.GetInstance().DefaultFont.GetFont(), Brush, _SpanX, _SpanY);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();
            base.OnPaint(e);
        }

        public void DoLayout()
        {
            Width = _SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(_Text) + _SpanX;
            Height = _SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + _SpanY;
            _BackColor = Color.EDITOR_UI_GENERAL_BACK_COLOR;
            if (_Menu != null)
            {
                _Menu.DoLayout();
            }
        }
    }
}