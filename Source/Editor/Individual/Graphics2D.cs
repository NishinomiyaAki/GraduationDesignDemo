using EditorUI;

namespace CrossEditor
{
    internal class Graphics2D
    {
        private static Graphics2D _Instance = new Graphics2D();

        private System.Drawing.Graphics _Graphics;

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return _Graphics;
            }
            set
            {
                _Graphics = value;
            }
        }

        public static Graphics2D GetInstance()
        {
            return _Instance;
        }

        public System.Drawing.Graphics GetGraphics()
        {
            return _Graphics;
        }

        public void DrawRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            _Graphics.DrawRectangle(CreatePenFromColor(Color), X, Y, Width, Height);
        }

        public void FillRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            _Graphics.FillRectangle(CreateSolidBrushFromColor(Color), X, Y, Width, Height);
        }

        public System.Drawing.Pen CreatePenFromColor(Color Color)
        {
            return new System.Drawing.Pen(ConvertColorToDrawingColor(Color));
        }

        public System.Drawing.SolidBrush CreateSolidBrushFromColor(Color Color)
        {
            return new System.Drawing.SolidBrush(ConvertColorToDrawingColor(Color));
        }

        public System.Drawing.Color ConvertColorToDrawingColor(Color Color)
        {
            uint Dword = Color.ToDword();
            int ByteR = (int)(Dword & 0xff);
            int ByteG = (int)((Dword >> 8) & 0xff);
            int ByteB = (int)((Dword >> 16) & 0xff);
            int ByteA = (int)((Dword >> 24) & 0xff);
            return System.Drawing.Color.FromArgb(ByteA, ByteR, ByteG, ByteB);
        }
    }
}