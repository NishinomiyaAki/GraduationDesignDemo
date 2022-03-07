using EditorUI;

namespace Editor
{
    internal class Font
    {
        private System.Drawing.Font _DrawingFont;

        public Font(string FontFamily, int FontSize)
        {
            _DrawingFont = new System.Drawing.Font(FontFamily, FontSize);
        }

        public int MeasureString_Fast(string String)
        {
            return System.Windows.Forms.TextRenderer.MeasureText(String, _DrawingFont).Width;
        }

        public int GetCharHeight()
        {
            return System.Windows.Forms.TextRenderer.MeasureText("abcdefghijklmnopqrstuvwxyz", _DrawingFont).Height;
        }

        public System.Drawing.Font GetFont()
        {
            return _DrawingFont;
        }

        public void DrawString(string String, ref Color Color, int X, int Y, int Width, int Height, TextAlign TextAlign)
        {
            Graphics2D Graphics2D = Graphics2D.GetInstance();
            System.Drawing.Graphics Graphics = Graphics2D.GetGraphics();
            System.Drawing.Rectangle Rect = new System.Drawing.Rectangle(X, Y, Width, Height);
            System.Windows.Forms.TextRenderer.DrawText(Graphics, String, _DrawingFont, Rect, Graphics2D.ConvertColorToDrawingColor(Color), TextAlign.GetFormatFlags());
        }
    }
}