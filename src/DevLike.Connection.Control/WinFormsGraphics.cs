
using Eto.Drawing;

namespace DevLike.Connection.Control;

public static class Float4PlatformExtensions
{
    public static Rectangle ToRect(this Float4 f)
    {
        return new Rectangle((int)f.X, (int)f.Y, (int)f.W, (int)f.H);
    }
}

public static class Float2PlatformExtensions
{
    public static Point ToPoint(this Float2 f)
    {
        return new Point((int)f.X, (int)f.Y);
    }

    public static Float2 ToFloat2(this Point p)
    {
        return new Float2 { X = p.X, Y = p.Y };
    }
}

public static class TintExtensions
{
    public static Color ToColor(this Tint tint)
    {
        switch (tint)
        {
            case Tint.Background: return Color.FromArgb(255, 35, 35, 35);
            case Tint.DarkGrey: return Color.FromArgb(255, 20, 20, 20);
            case Tint.LightGrey: return Color.FromArgb(255, 50, 50, 50);
            case Tint.White: return Colors.White;
            case Tint.Blue: return Colors.Blue;
            case Tint.Green: return Colors.Green;
            case Tint.Red: return Colors.Red;
            case Tint.Pink: return Colors.Pink;
            case Tint.Purple: return Colors.Purple;
            case Tint.Orange: return Colors.Orange;
            case Tint.Yellow: return Colors.Yellow;
            case Tint.Cyan: default: return Colors.Cyan;
        }
    }
}

public class WinformsGraphics : IGraphics
{
    readonly Font font;

    public WinformsGraphics()
    {
        font = SystemFonts.Label();
        // font = new Font("codepro.ttf", 12, FontStyle.None, FontDecoration.None);
    }

    public Graphics Graphics { get; set; } = null!;

    public void DrawArrow(Tint tint, List<Float2> points, bool bothEnds = false, int lineWidth = 1, int headWidth = 5, int headHeight = 5, int headDistance = 0, bool dashed = false)
    {
        using (Pen p = new Pen(tint.ToColor(), lineWidth))
        using (GraphicsPath head = new GraphicsPath())
        {
            if (dashed)
            {
                p.DashStyle = DashStyles.Dash;
            }

            head.AddLine(headWidth, -headHeight - headDistance, 0, 0 - headDistance);
            head.AddLine(0, 0 - headDistance, -headWidth, -headHeight - headDistance);
            head.AddLine(-headWidth, -headHeight - headDistance, headWidth, -headHeight - headDistance);

            if (points.First() == points.Last())
            {
                var first = points.First();
                var control = points.Distinct().Sum();

                var v = new Float2 { X = (control.X + first.X) / 2, Y = (control.Y + first.Y) / 2 };
                var r = MathF.Max(1.0f, Float2.Distance(control, first) / 2);
                var ul = new Float2 { X = v.X - r, Y = v.Y - r };
                var rect = new Float4 { X = ul.X, Y = ul.Y, W = 2 * r, H = 2 * r };
                Graphics.DrawArc(p, rect.ToRect(), 0, 360);
            }
            else
            {
                var path = new GraphicsPath();
                path.AddCurve(points.Select(xy => new PointF(xy.X, xy.Y)));
                Graphics.DrawPath(p, path);
            }
        }
    }

    public void DrawCircle(Tint tint, Float2 center, float radius, int lineWidth = 1)
    {
        using (var pen = new Pen(tint.ToColor(), lineWidth))
        {
            Graphics.DrawEllipse(pen, center.X - radius, center.Y - radius,
              radius + radius, radius + radius);
        }
    }

    public void DrawCurve(Tint tint, List<Float2> points, int lineWidth = 1)
    {
        using (var pen = new Pen(tint.ToColor(), lineWidth))
        {
            var path = new GraphicsPath();
            path.AddCurve(points.Select(xy => new PointF(xy.X, xy.Y)));
            Graphics.DrawPath(pen, path);
        }
    }

    public void DrawArc(Tint tint, Float4 rect, int initial = 0, int arc = 360, int lineWidth = 1)
    {
        using (var pen = new Pen(tint.ToColor(), lineWidth))
        {
            rect = rect.Normalized();
            Graphics.DrawArc(pen, rect.X, rect.Y, rect.W, rect.H, initial, arc);
        }
    }

    public void DrawRectangle(Tint tint, Float4 rect, int lineWidth = 1)
    {
        using (var pen = new Pen(tint.ToColor(), lineWidth))
        {
            rect = rect.Normalized();
            Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.W, rect.H);
        }
    }

    public void DrawText(Tint tint, Float2 origin, string text)
    {
        using (var brush = new SolidBrush(tint.ToColor()))
        {
            Graphics.DrawText(font, brush, origin.X, origin.Y, text);
        }
    }

    public void FillCircle(Tint tint, Float2 center, float radius, float alpha = 1.0f)
    {
        var color = tint.ToColor();
        color = Color.FromArgb((int)(alpha * 255.0f), color.Rb, color.Gb, color.Bb);
        using (var brush = new SolidBrush(color))
        {
            Graphics.FillEllipse(brush, center.X - radius, center.Y - radius,
              radius + radius, radius + radius);
        }
    }

    public void FillRectangle(Tint tint, Float4 rect, float alpha = 1.0f)
    {
        var color = tint.ToColor();
        color = Color.FromArgb((int)(alpha * 255.0f), color.Rb, color.Gb, color.Bb);

        using (var brush = new SolidBrush(color))
        {
            Graphics.FillRectangle(brush, rect.X, rect.Y, rect.W, rect.H);
        }
    }

    public void DrawLine(Tint tint, Float4 line, int lineWidth = 1)
    {
        using (var pen = new Pen(tint.ToColor(), lineWidth))
        {
            Graphics.DrawLine(pen,
                new Point((int)line.X, (int)line.Y),
                new Point((int)line.W, (int)line.H));
        }
    }

    public Float2 GetStringWidth(string text)
    {
        var measure = Graphics.MeasureString(font, text);
        return new Float2 { X = measure.Width, Y = measure.Height };
    }
}
