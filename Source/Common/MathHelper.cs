using System;

namespace EditorUI
{
    public static class MathHelper
    {
        public const float Epsilon = 0.0001f;

        public static sbyte ParseSByte(string String)
        {
            sbyte Value;
            sbyte.TryParse(String, out Value);
            return Value;
        }

        public static byte ParseByte(string String)
        {
            byte Value;
            byte.TryParse(String, out Value);
            return Value;
        }

        public static short ParseShort(string String)
        {
            short Value;
            short.TryParse(String, out Value);
            return Value;
        }

        public static ushort ParseUShort(string String)
        {
            ushort Value;
            ushort.TryParse(String, out Value);
            return Value;
        }

        public static int ParseInt(string String)
        {
            int Value;
            int.TryParse(String, out Value);
            return Value;
        }

        public static uint ParseUInt(string String)
        {
            uint Value;
            uint.TryParse(String, out Value);
            return Value;
        }

        public static long ParseLong(string String)
        {
            long Value;
            long.TryParse(String, out Value);
            return Value;
        }

        public static ulong ParseULong(string String)
        {
            ulong Value;
            ulong.TryParse(String, out Value);
            return Value;
        }

        public static float ParseFloat(string String)
        {
            float Value;
            float.TryParse(String, out Value);
            return Value;
        }

        public static double ParseDouble(string String)
        {
            double Value;
            double.TryParse(String, out Value);
            return Value;
        }

        public static decimal ParseDecimal(string String)
        {
            decimal Value;
            decimal.TryParse(String, out Value);
            return Value;
        }

        public static object ParseNumber(string String, Type NumericType)
        {
            object Value = null;
            if (NumericType == typeof(sbyte))
            {
                Value = MathHelper.ParseSByte(String);
            }
            else if (NumericType == typeof(byte))
            {
                Value = MathHelper.ParseByte(String);
            }
            else if (NumericType == typeof(short))
            {
                Value = MathHelper.ParseShort(String);
            }
            else if (NumericType == typeof(ushort))
            {
                Value = MathHelper.ParseUShort(String);
            }
            else if (NumericType == typeof(int))
            {
                Value = MathHelper.ParseInt(String);
            }
            else if (NumericType == typeof(uint))
            {
                Value = MathHelper.ParseUInt(String);
            }
            else if (NumericType == typeof(long))
            {
                Value = MathHelper.ParseLong(String);
            }
            else if (NumericType == typeof(ulong))
            {
                Value = MathHelper.ParseULong(String);
            }
            else if (NumericType == typeof(float))
            {
                Value = MathHelper.ParseFloat(String);
            }
            else if (NumericType == typeof(double))
            {
                Value = MathHelper.ParseDouble(String);
            }
            else if (NumericType == typeof(decimal))
            {
                Value = MathHelper.ParseDecimal(String);
            }
            return Value;
        }

        public static float DegreeToRadians(float degree)
        {
            return (float)((Math.PI / 180.0) * degree);
        }

        public static float RadiansToDegree(float degree)
        {
            return (float)((180.0 / Math.PI) * degree);
        }
    }
}