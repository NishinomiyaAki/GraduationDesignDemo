using EditorUI;
using System;

namespace Editor
{
    public class CubicBazier
    {
        private Vector2f Point1;
        private Vector2f Point2;
        private Vector2f Point3;
        private Vector2f Point4;
        private float AX;
        private float BX;
        private float CX;
        private float AY;
        private float BY;
        private float CY;

        public CubicBazier()
        {
            AX = 0.0f;
            BX = 0.0f;
            CX = 0.0f;
            AY = 0.0f;
            BY = 0.0f;
            CY = 0.0f;
        }

        public void Initialize(Vector2f Point1, Vector2f Point2, Vector2f Point3, Vector2f Point4)
        {
            this.Point1 = Point1;
            this.Point2 = Point2;
            this.Point3 = Point3;
            this.Point4 = Point4;

            CX = 3.0f * (Point2.X - Point1.X);
            BX = 3.0f * (Point3.X - Point2.X) - CX;
            AX = Point4.X - Point1.X - CX - BX;

            CY = 3.0f * (Point2.Y - Point1.Y);
            BY = 3.0f * (Point3.Y - Point2.Y) - CY;
            AY = Point4.Y - Point1.Y - CY - BY;
        }

        public void GetBoundingBox(out float X1, out float Y1, out float X2, out float Y2)
        {
            X1 = 100000.0f;
            Y1 = 100000.0f;
            X2 = -100000.0f;
            Y2 = -100000.0f;

            X1 = Math.Min(X1, Point1.X);
            X1 = Math.Min(X1, Point2.X);
            X1 = Math.Min(X1, Point3.X);
            X1 = Math.Min(X1, Point4.X);

            Y1 = Math.Min(Y1, Point1.Y);
            Y1 = Math.Min(Y1, Point2.Y);
            Y1 = Math.Min(Y1, Point3.Y);
            Y1 = Math.Min(Y1, Point4.Y);

            X2 = Math.Max(X2, Point1.X);
            X2 = Math.Max(X2, Point2.X);
            X2 = Math.Max(X2, Point3.X);
            X2 = Math.Max(X2, Point4.X);

            Y2 = Math.Max(Y2, Point1.Y);
            Y2 = Math.Max(Y2, Point2.Y);
            Y2 = Math.Max(Y2, Point3.Y);
            Y2 = Math.Max(Y2, Point4.Y);

            Y1 -= 4;
            Y2 += 4;
        }

        private Vector2f CalculateBezier(float T)
        {
            Vector2f Result = new Vector2f();

            float TSquared, TCubed;
            TSquared = T * T;
            TCubed = TSquared * T;
            Result.X = (AX * TCubed) + (BX * TSquared) + (CX * T) + this.Point1.X;
            Result.Y = (AY * TCubed) + (BY * TSquared) + (CY * T) + this.Point1.Y;

            return Result;
        }

        public void Draw(Color Color, int Segment)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();
            int i;
            for (i = 0; i < Segment; i++)
            {
                float T1 = i / (float)Segment;
                float T2 = (i + 1) / (float)Segment;
                Vector2f Point1 = CalculateBezier(T1);
                Vector2f Point2 = CalculateBezier(T2);
                float X1 = Point1.X;
                float Y1 = Point1.Y;
                float X2 = Point2.X;
                float Y2 = Point2.Y;
                GraphicsHelper.DrawLine(Color, X1, Y1, X2, Y2);
            }
            if (true)
            {
                float X1 = Point4.X - 8;
                float Y1 = Point4.Y - 4;
                float X2 = Point4.X;
                float Y2 = Point4.Y;
                GraphicsHelper.DrawLine(Color, X1, Y1, X2, Y2);
            }
            if (true)
            {
                float X1 = Point4.X - 8;
                float Y1 = Point4.Y + 4;
                float X2 = Point4.X;
                float Y2 = Point4.Y;
                GraphicsHelper.DrawLine(Color, X1, Y1, X2, Y2);
            }
        }

        public bool HitTest(Vector2f Point, int Segment, float Radius)
        {
            float RadiusSq = Radius * Radius;
            int i;
            for (i = 0; i <= Segment; i++)
            {
                float T1 = i / (float)Segment;
                Vector2f Point1 = CalculateBezier(T1);
                float DistanceSq = (Point1.X - Point.X) * (Point1.X - Point.X) + (Point1.Y - Point.Y) * (Point1.Y - Point.Y);
                if (DistanceSq <= RadiusSq)
                {
                    return true;
                }
            }
            Vector2f[] Points = new Vector2f[8];
            Points[0] = new Vector2f(Point4.X - 8, Point4.Y - 4);
            Points[1] = new Vector2f(Point4.X - 8, Point4.Y + 4);
            Points[2] = new Vector2f(Point4.X - 6, Point4.Y - 3);
            Points[3] = new Vector2f(Point4.X - 6, Point4.Y + 3);
            Points[4] = new Vector2f(Point4.X - 4, Point4.Y - 2);
            Points[5] = new Vector2f(Point4.X - 4, Point4.Y + 2);
            Points[6] = new Vector2f(Point4.X - 2, Point4.Y - 1);
            Points[7] = new Vector2f(Point4.X - 2, Point4.Y + 1);
            for (i = 0; i < 8; i++)
            {
                Vector2f Point1 = Points[i];
                float DistanceSq = (Point1.X - Point.X) * (Point1.X - Point.X) + (Point1.Y - Point.Y) * (Point1.Y - Point.Y);
                if (DistanceSq <= RadiusSq)
                {
                    return true;
                }
            }
            return false;
        }
    }
}