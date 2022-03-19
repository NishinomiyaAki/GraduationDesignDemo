using EditorUI;
using System;

namespace CrossEditor
{
    class GraphicsHelper
    {
        static GraphicsHelper Instance = new GraphicsHelper();

        public const int DefaultFontSize = 12;

        int BaseX;
        int BaseY;
        Graphics2D Graphics2D;
        public Font DefaultFont;
        GraphCamera2D _Camera;

        int GridID = 0;

        public static GraphicsHelper GetInstance()
        {
            return Instance;
        }

        public int QueryGridID()
        {
            return GridID++;
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
        public void SetCamera(GraphCamera2D Camera)
        {
            _Camera = Camera;
        }

        public GraphCamera2D GetCamera() => _Camera;

        void ToScreenXY(float X, float Y, ref int outX, ref int outY)
        {
            if (_Camera != null)
            {
                _Camera.WorldToScreen((int)X, (int)Y, ref outX, ref outY);
            }
            else
            {
                outX = (int)X + BaseX;
                outY = (int)Y + BaseY;
            }
        }

        float GetZoomedWidth(float width)
        {
            if (_Camera != null)
            {
                float ret = _Camera.GetZoomRatio() * width;
                return ret < 1.0f ? 1.0f : ret;
            }
            else
            {
                return width;
            }
        }

        public void DrawLine(Color Color, float X1, float Y1, float X2, float Y2, float LineWidth = 3.0f)
        {
            if (X1 != X2 || Y1 != Y2)
            {
                int ScreenX1 = (int)X1;
                int ScreenY1 = (int)Y1;
                int ScreenX2 = (int)X2;
                int ScreenY2 = (int)Y2;
                ToScreenXY(X1, Y1, ref ScreenX1, ref ScreenY1);
                ToScreenXY(X2, Y2, ref ScreenX2, ref ScreenY2);
                Float4 colorf = new Float4();
                colorf.x = Color.R;
                colorf.y = Color.G;
                colorf.z = Color.B;
                colorf.w = Color.A;
                EditorUICanvas.GetInstance().GetUICanvasInterface().DrawLineF(ScreenX1, ScreenY1, ScreenX2, ScreenY2, GetZoomedWidth(LineWidth), 1.0f, colorf);
            }
        }

        public void DrawArrow(Color Color, float X1, float Y1, float X2, float Y2)
        {
            if (X1 != X2 || Y1 != Y2)
            {
                Float4 colorf = new Float4();
                colorf.x = Color.R;
                colorf.y = Color.G;
                colorf.z = Color.B;
                colorf.w = Color.A;
                EditorUICanvas.GetInstance().GetUICanvasInterface().DrawLineF(BaseX + X1, BaseY + Y1, BaseX + X2, BaseY + Y2, GetZoomedWidth(3.0f), 1.0f, colorf);
                // TODO Ugly Code :(
                Graphics2D.FillRectangle(Color, (int)(BaseX + X2 - 4), (int)(BaseY + Y2 - 4), 8, 8);
            }
        }

        public void DrawRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            int ScreenX1 = (int)X;
            int ScreenY1 = (int)Y;
            int ScreenX2 = (int)X + Width;
            int ScreenY2 = (int)Y + Height;
            ToScreenXY(X, Y, ref ScreenX1, ref ScreenY1);
            ToScreenXY(X + Width, Y + Height, ref ScreenX2, ref ScreenY2);
            Graphics2D.DrawRectangle(Color, ScreenX1, ScreenY1, ScreenX2 - ScreenX1, ScreenY2 - ScreenY1);
        }

        public void DrawRectangleLines(Color Color, float X, float Y, float Width, float Height, float LineWidth = 3.0f)
        {
            DrawLine(Color, X - LineWidth / 2.0f, Y, X + Width + LineWidth / 2.0f, Y, LineWidth);
            DrawLine(Color, X, Y, X, Y + Height, LineWidth);
            DrawLine(Color, X - LineWidth / 2.0f, Y + Height, X + Width + LineWidth / 2.0f, Y + Height, LineWidth);
            DrawLine(Color, X + Width, Y, X + Width, Y + Height, LineWidth);
        }

        public void FillRectangle(Color Color, int X, int Y, int Width, int Height)
        {
            int ScreenX1 = (int)X;
            int ScreenY1 = (int)Y;
            int ScreenX2 = (int)X + Width;
            int ScreenY2 = (int)Y + Height;
            ToScreenXY(X, Y, ref ScreenX1, ref ScreenY1);
            ToScreenXY(X + Width, Y + Height, ref ScreenX2, ref ScreenY2);
            Graphics2D.FillRectangle(Color, ScreenX1, ScreenY1, ScreenX2 - ScreenX1, ScreenY2 - ScreenY1);
        }

        public void DrawString(Font Font, string String, Color Color, int X, int Y, int Width, int Height, TextAlign TextAlign)
        {
            if (Font == null)
            {
                Font = DefaultFont;
                if (_Camera != null)
                {
                    int FontSize = (int)(DefaultFontSize * _Camera.GetZoomRatio());
                    if (FontSize < 1)
                    {
                        // Too small
                        return;
                    }
                    Font = UIManager.GetInstance().GetDefaultFont(FontSize);
                }
            }

            int ScreenX1 = (int)X;
            int ScreenY1 = (int)Y;
            int ScreenX2 = (int)X + Width;
            int ScreenY2 = (int)Y + Height;
            ToScreenXY(X, Y, ref ScreenX1, ref ScreenY1);
            ToScreenXY(X + Width, Y + Height, ref ScreenX2, ref ScreenY2);
            Font.DrawString(String, ref Color, ScreenX1, ScreenY1, ScreenX2 - ScreenX1, ScreenY2 - ScreenY1, TextAlign);
            
        }
        public void DrawCircleF(Color Color, float X, float Y, float Radius, int SegmentCount)
        {
            int ScreenX1 = (int)X;
            int ScreenY1 = (int)Y;
            int ScreenX2 = (int)(X + Radius);
            int ScreenY2 = (int)Y;
            ToScreenXY(X, Y, ref ScreenX1, ref ScreenY1);
            ToScreenXY(X + Radius, Y, ref ScreenX2, ref ScreenY2);
            Float4 colorf = new Float4();
            colorf.x = Color.R;
            colorf.y = Color.G;
            colorf.z = Color.B;
            colorf.w = Color.A;
            EditorUICanvas.GetInstance().GetUICanvasInterface().DrawCircleF(ScreenX1, ScreenY1, ScreenX2 - ScreenX1, SegmentCount, GetZoomedWidth(1.0f), colorf);
        }
        public void DrawTexture(Texture Texture, int X, int Y, int Width, int Height)
        {
            int ScreenX1 = (int)X;
            int ScreenY1 = (int)Y;
            int ScreenX2 = (int)X + Width;
            int ScreenY2 = (int)Y + Height;
            ToScreenXY(X, Y, ref ScreenX1, ref ScreenY1);
            ToScreenXY(X + Width, Y + Height, ref ScreenX2, ref ScreenY2);
            //Graphics2D.DrawTexture(Texture, Color.FromRGB(255, 255, 255), BaseX + X, BaseY + Y, Width, Height);
            Graphics2D.DrawTexture(Texture, Color.FromRGB(255, 255, 255), ScreenX1, ScreenY1, ScreenX2 - ScreenX1, ScreenY2 - ScreenY1);
        }

        public void DrawGrid(float cell_size, int tile_number, float scroll_offsetx, float scroll_offsety, float scroll_sizex, float scroll_sizey,Vector4f ThickColor,Vector4f ThinColor, int gridID)
        {
            if (_Camera != null)
            {
                cell_size = cell_size * _Camera.GetZoomRatio();
                int ScreenX1 = 0;
                int ScreenY1 = 0;
                int ScreenX2 = 0;
                int ScreenY2 = 0;
                ToScreenXY(0, 0, ref ScreenX1, ref ScreenY1);
                ToScreenXY(_Camera.WorldHeight * _Camera.AspectRatio, _Camera.WorldHeight, ref ScreenX2, ref ScreenY2);
                scroll_sizex = ScreenX2 - ScreenX1 + cell_size * 2 * 1000;
                scroll_sizey = ScreenY2 - ScreenY1 + cell_size * 2 * 1000;
                scroll_offsetx = _Camera.ScreenX + ((ScreenX1 - _Camera.ScreenX) % (1000 * cell_size) - 1000 * cell_size);
                scroll_offsety = _Camera.ScreenY + ((ScreenY1 - _Camera.ScreenY) % (1000 * cell_size) - 1000 * cell_size);
            }
            Float4 ThickColorf = new Float4();
            ThickColorf.x = ThickColor.X;
            ThickColorf.y = ThickColor.Y;
            ThickColorf.z = ThickColor.Z;
            ThickColorf.w = ThickColor.W;
            Float4 ThinColorf = new Float4();
            ThinColorf.x = ThinColor.X;
            ThinColorf.y = ThinColor.Y;
            ThinColorf.z = ThinColor.Z;
            ThinColorf.w = ThinColor.W;
            EditorUICanvas.GetInstance().GetUICanvasInterface().DrawGrid(cell_size, tile_number, scroll_offsetx, scroll_offsety, scroll_sizex, scroll_sizey,ThickColorf,ThinColorf, gridID);
        }
    }
}
