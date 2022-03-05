using EditorUI;

namespace CrossEditor
{
    internal class GraphicsHelper
    {
        private static GraphicsHelper Instance = new GraphicsHelper();

        public const int DefaultFontSize = 12;

        private int BaseX;
        private int BaseY;
        private Graphics2D Graphics2D;
        public Font DefaultFont;

        public static GraphicsHelper GetInstance()
        {
            return Instance;
        }

        public GraphicsHelper()
        {
            Graphics2D = Graphics2D.GetInstance();
            DefaultFont = UIManager.GetInstance().GetDefaultFont(DefaultFontSize);
        }

        public void SetBaseXAndBaseY(int BaseX, int BaseY)
        {
            this.BaseX = BaseX;
            this.BaseY = BaseY;
        }

        public void DrawLine(Color Color, float X1, float Y1, float X2, float Y2)
        {
            if (X1 != X2 || Y1 != Y2)
            {
                AnimatorUICanvas.GetInstance().DrawLineF(new Vector2f(BaseX + X1, BaseY + Y1), new Vector2f(BaseX + X2, BaseY + Y2), 3.0f, 1.0f, Color);
            }
        }

        public void DrawRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            Graphics2D.DrawRectangle(Color, BaseX + X, BaseY + Y, Width, Height);
        }

        public void FillRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            Graphics2D.FillRectangle(Color, BaseX + X, BaseY + Y, Width, Height);
        }

        public void DrawString(Font Font, string String, Color Color, int X, int Y, int Width, int Height, TextAlign TextAlign)
        {
            if (Font == null)
            {
                Font = DefaultFont;
            }
            Font.DrawString(String, ref Color, BaseX + X, BaseY + Y, Width, Height, TextAlign);
        }
    }
}