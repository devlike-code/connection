using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace connection
{
    public enum Tint
    {
        Background,
        DarkGrey,
        LightGrey,
        White,
        Blue,
        Green,
        Red,
        Pink,
        Purple,
        Orange,
        Yellow,
        Cyan,
    }

    public struct Float2
    {
        public static Float2 Zero { get => new Float2 { X = 0.0f, Y = 0.0f }; }

        public float X { get; set; }
        public float Y { get; set; }

        public static Float2 operator +(Float2 a, Float2 b)
        {
            return new Float2 { X = a.X + b.X, Y = a.Y + b.Y };
        }

        public static Float2 operator -(Float2 a, Float2 b)
        {
            return new Float2 { X = a.X - b.X, Y = a.Y - b.Y };
        }

        public static Float2 operator *(Float2 a, Float2 b)
        {
            return new Float2 { X = a.X * b.X, Y = a.Y * b.Y };
        }

        public static Float2 operator +(Float2 a, float b)
        {
            return new Float2 { X = a.X + b, Y = a.Y + b };
        }

        public static Float2 operator *(Float2 a, float b)
        {
            return new Float2 { X = a.X * b, Y = a.Y * b };
        }

        public static bool operator ==(Float2 a, Float2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Float2 a, Float2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static float Distance(Float2 a, Float2 b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dxx = dx * dx;
            var dyy = dy * dy;

            return (float)Math.Sqrt(dxx + dyy);            
        }

        public override bool Equals(object obj)
        {
            if (obj is Float2 f2)
            {
                return this == f2;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
    }

    public static class Float2Extensions
    {
        public static Float2 Sum(this IEnumerable<Float2> self)
        {
            if (self.Count() == 0) 
                return new Float2 { X = 0, Y = 0 };

            var point = new Float2 { X = 0.0f, Y = 0.0f };
            foreach (var s in self)
            {
                point.X += s.X;
                point.Y += s.Y;
            }

            return point * (1.0f / (float)self.Count());
        }
    }

    public struct Float4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }

        public static Float4 operator +(Float4 a, Float2 b)
        {
            return new Float4 { X = a.X + b.X, Y = a.Y + b.Y, W = a.W, H = a.H };
        }

        public static Float4 operator +(Float4 a, float b)
        {
            return new Float4 { X = a.X - b, Y = a.Y - b, W = a.W + 2 * b, H = a.H + 2 * b };
        }

        public static Float4 operator -(Float4 a, Float2 b)
        {
            return new Float4 { X = a.X - b.X, Y = a.Y - b.Y, W = a.W, H = a.H };
        }

        public Float2 Center { get => new Float2 { X = X + W / 2, Y = Y + W / 2 }; }

        public static Float4 Empty { get => new Float4 { X = 0, Y = 0, W = 0, H = 0 }; }

        public bool Contains(Float2 xy) => xy.X >= X && xy.X <= X + W && xy.Y >= Y && xy.Y <= Y + H;

        public bool IntersectsWith(Float4 rect)
        {
            if (W == 0 || H == 0 || rect.W == 0 || rect.H == 0) return false;
            if (X > rect.X + rect.W || rect.X > X + W) return false;
            if (Y > rect.Y + rect.H || rect.Y > Y + H) return false;
            return true;
        }
    }

    public interface IGraphics
    {
        public Float2 GetStringWidth(string text);

        public void DrawCircle(Tint tint, Float2 center, float radius, int lineWidth = 1);

        public void FillCircle(Tint tint, Float2 center, float radius, float alpha = 1.0f);

        public void DrawCurve(Tint tint, List<Float2> points, int lineWidth = 1);

        public void DrawArrow(Tint tint, List<Float2> points, bool bothEnds, int lineWidth = 1, int headWidth = 2, int headHeight = 5, int headDistance = 0);

        public void DrawText(Tint tint, Float2 origin, string text);

        public void DrawRectangle(Tint tint, Float4 rect, int lineWidth = 1);

        public void FillRectangle(Tint tint, Float4 rect, float alpha = 1.0f);

        public void DrawLine(Tint tint, Float4 line, int lineWidth = 1);
    }
}
