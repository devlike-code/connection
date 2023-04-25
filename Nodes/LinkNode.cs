
namespace connection.Nodes
{
    public class LinkNode : Node
    {
        public LinkNode(Node source, Node target, bool bothWays)
            : base((source.Origin + target.Origin) * 0.5f, true, true)
        {
            Depth = 1;
            Source = source;
            Target = target;
            Dual = bothWays;

            AddTag("Label", bothWays ? "<->" : "->");
        }

        public override void Draw(IGraphics graphics)
        {
            graphics.DrawArrow(Tint.White,
                new List<Float2> {
                    Source.Origin,
                    Origin,
                    Target.Origin },
                Dual, headDistance: 2);

            graphics.DrawCircle(Tint.White, Origin, 3);

            if (IsUnder(graphics, Rendering.Mouse))
            {
                GraphInternals.Hovered.Add(this);
                graphics.FillCircle(Tint.White, Origin, 4);
            }
            else
            {
                GraphInternals.Hovered.Remove(this);
            }

            if (GraphInternals.Selected.Contains(this))
            {
                graphics.DrawCircle(Tint.Yellow, Origin, 6);
            }
        }

        public override Float4 GetBounds(IGraphics graphics)
        {
            LastBounds = new Float4 { X = Origin.X - 10, Y = Origin.Y - 10, W = 20, H = 20 };
            return LastBounds;
        }
    }
}
