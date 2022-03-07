using EditorUI;

namespace Editor
{
    internal class AnimatorUICanvas
    {
        private static AnimatorUICanvas Instance = new AnimatorUICanvas();

        public static AnimatorUICanvas GetInstance()
        {
            return Instance;
        }

        public void DrawLineF(Vector2f Point1, Vector2f Point2, float PenWidth, float UnknownParameter, Color Color)
        {
            Graphics2D Graphics2D = Graphics2D.GetInstance();
            System.Drawing.Graphics Graphics = Graphics2D.GetGraphics();
            System.Drawing.Pen Pen = new System.Drawing.Pen(Graphics2D.ConvertColorToDrawingColor(Color), PenWidth);
            Graphics.DrawLine(Pen, Point1.X, Point1.Y, Point2.X, Point2.Y);
        }
    }
}