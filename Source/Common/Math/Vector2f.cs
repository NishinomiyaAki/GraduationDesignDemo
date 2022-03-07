using EditorUI;
using System;

namespace Editor
{
    public struct Vector2f
    {
        public float X;
        public float Y;

        public Vector2f(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public override bool Equals(object Object)
        {
            if (Object is Vector2f)
            {
                return this == (Vector2f)Object;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(Vector2f v1, Vector2f v2)
        {
            return Math.Abs(v1.X - v2.X) < MathHelper.Epsilon &&
                   Math.Abs(v1.Y - v2.Y) < MathHelper.Epsilon;
        }

        public static bool operator !=(Vector2f v1, Vector2f v2)
        {
            return Math.Abs(v1.X - v2.X) >= MathHelper.Epsilon ||
                   Math.Abs(v1.Y - v2.Y) >= MathHelper.Epsilon;
        }

        public static Vector2f operator +(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2f operator -(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2f operator *(Vector2f v1, float f)
        {
            return new Vector2f(v1.X * f, v1.Y * f);
        }

        public static Vector2f Lerp(ref Vector2f v1, ref Vector2f v2, float f)
        {
            float f1 = 1.0f - f;
            float X = v1.X * f1 + v2.X * f;
            float Y = v1.Y * f1 + v2.Y * f;
            return new Vector2f(X, Y);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public Vector2f Normalize()
        {
            float length = MathF.Sqrt(X * X + Y * Y);

            if (length < float.Epsilon)
                return new Vector2f(0, 0);

            return new Vector2f(X / length, Y / length);
        }
    }
}