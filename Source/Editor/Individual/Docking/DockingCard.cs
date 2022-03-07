using EditorUI;

namespace Editor
{
    public class DockingCard : Control
    {
        private DockingBlock _DockingBlock = DockingBlock.GetInstance();
        private string _Text;
        private Panel _Panel;
        private bool _bActive;
        private const int _SpanX = 3;
        private const int _SpanY = 3;

        public bool Active
        {
            get
            {
                return _bActive;
            }
            set
            {
                _bActive = value;
            }
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && !_bActive)
            {
                DockingBlock.GetInstance().ActivateCard(this);
                DockingManager.GetInstance().ChangePanel(_Panel);
                InspectorManager.GetInstance().ClearInspector();
            }

            base.OnMouseClick(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            Graphics.Clear(this.BackColor);

            Graphics2D Graphics2D = Graphics2D.GetInstance();

            System.Drawing.Brush Brush = Graphics2D.CreateSolidBrushFromColor(Color.EDITOR_UI_CONTROL_BACK_COLOR);
            if (_bActive)
            {
                Brush = Graphics2D.CreateSolidBrushFromColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
            }
            Graphics.FillRectangle(Brush, 0, 0, this.Width, this.Height);

            Brush = Graphics2D.CreateSolidBrushFromColor(Color.EDITOR_UI_GENERAL_TEXT_COLOR);
            Graphics.DrawString(_Text, GraphicsHelper.GetInstance().DefaultFont.GetFont(), Brush, _SpanX, _SpanY);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();

            base.OnPaint(e);
        }

        public void Initialize(string Text, Panel Panel)
        {
            _Panel = Panel;
            _Text = Text;
            _bActive = false;
            Width = _SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(Text) + _SpanX;
            Height = _SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + _SpanY;
        }

        public static int GetDefalutHeight()
        {
            return (_SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + _SpanY);
        }

        public DockingBlock GetDockingBlock()
        {
            return _DockingBlock;
        }

        public bool GetActive()
        {
            return _bActive;
        }

        public void SetText(string Text)
        {
            _Text = Text;
            _DockingBlock.DoLayout();
            _DockingBlock.Refresh();
        }

        public void DoLayout(ref int PaddingX)
        {
            Width = _SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(_Text) + _SpanX;
            Location = new System.Drawing.Point(PaddingX, 0);
            PaddingX += Width;
        }
    }
}