namespace connection
{
    public static class Rendering
    {
        public static Float2 Mouse;

        public static void DrawGrid(IGraphics graphics, int width, int height)
        {
            graphics.FillRectangle(Tint.LightGrey, new Float4 { X = -1, Y = -1, W = width + 1, H = height + 1 });

            var cellSize = 32;
            for (int i = 0; i < Math.Max(width, height) / cellSize + 1; i++)
            {
                graphics.DrawLine(Tint.DarkGrey, new Float4 { X = i * cellSize, Y = 0, W = i * cellSize, H = height + 1 });
                graphics.DrawLine(Tint.DarkGrey, new Float4 { X = 0, Y = i * cellSize, W = width + 1, H = i * cellSize });
            }

            for (int i = 0; i < Math.Max(width, height) / cellSize + 1; i++)
            {
                graphics.DrawLine(Tint.Background, new Float4 { X = 16 + i * cellSize, Y = 0, W = 16 + i * cellSize, H = height + 1 });
                graphics.DrawLine(Tint.Background, new Float4 { X = 0, Y = 16 + i * cellSize, W = width + 1, H = 16 + i * cellSize });
            }
        }
    }
}
