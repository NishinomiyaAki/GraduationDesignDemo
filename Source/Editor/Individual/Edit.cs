using EditorUI;
using System;

namespace Editor
{
    internal class Edit : Control
    {
        private int _FontSize;
        private EditMode _EditMode;
        private bool _bReadOnly;
        private int _Width;
        private int _Height;
        private int _PaddingX = 0;
        public const int _SpanX = 5;
        public const int _SpanY = 3;
        private string _Text;

        public new string Text => _Text;

        public Edit()
        {
        }

        public void Initialize(EditMode EditMode)
        {
            _EditMode = EditMode;
        }

        public void SetFontSize(int FontSize)
        {
            _FontSize = FontSize;
        }

        public static int GetDefalutFontHeight()
        {
            return _SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + _SpanY;
        }

        public void LoadSource(string SourcePath)
        {
            // TODO: load source
        }

        public void SetReadOnly(bool bReadOnly)
        {
            _bReadOnly = bReadOnly;
        }

        public void SetSize(int Width, int Height)
        {
            _Width = Width;
            _Height = Height;
        }

        public void SetText(string Text)
        {
            _Text = Text;
        }

        public void SetPaddingX(int X)
        {
            _PaddingX = X;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                BackColor = Parent.BackColor;
            }
            base.OnParentChanged(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            Graphics.Clear(BackColor);

            System.Drawing.Brush Brush = Graphics2D.GetInstance().CreateSolidBrushFromColor(Color.EDITOR_UI_GENERAL_TEXT_COLOR);
            Graphics.DrawString(_Text, GraphicsHelper.GetInstance().DefaultFont.GetFont(), Brush, _SpanX + _PaddingX, _SpanY);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();
            base.OnPaint(e);
        }
    }
}