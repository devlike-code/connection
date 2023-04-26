
using System.Drawing;

namespace connection.Nodes
{
    public class LinkNode : Node
    {
        public bool SideMoved = false;

        public LinkNode(Node source, Node target, bool bothWays)
            : base((source.Origin + target.Origin) * 0.5f, true, true)
        {
            Depth = 2;
            ExportDepth = 1;

            Source = source;
            Target = target;
            Dual = bothWays;

            source.OnNodeMove += Source_OnNodeMove;
            target.OnNodeMove += Target_OnNodeMove;

            if (source == target)
            {
                Origin.X += 30;
            }

            AddTag("Label", bothWays ? "<->" : "->");

            if (bothWays)
                AddTag("BothWays", "true");
        }

        public override string Export()
        {
            var tags = string.Join("; ", Tags.Select(tag => $"{tag.Key}: \"{tag.Value}\""));
            return $"link\t|\t{Id}\t|\t{Source.Id}\t|\t{Target.Id}\t|\t{tags}";
        }

        private void Source_OnNodeMove(Node movedNode, Float2 oldPos, Float2 newPos)
        {
            if (GraphInternals.Selected.Contains(Target)) return;

            var oldOrigin = (oldPos + Target.Origin) * 0.5f;
            var delta = Origin - oldOrigin;

            var newOrigin = (newPos + Target.Origin) * 0.5f;
            Origin = newOrigin + delta;
        }

        private void Target_OnNodeMove(Node movedNode, Float2 oldPos, Float2 newPos)
        {
            if (GraphInternals.Selected.Contains(Source)) return;

            var oldOrigin = (Source.Origin + oldPos) * 0.5f;
            var delta = Origin - oldOrigin;

            var newOrigin = (Source.Origin + newPos) * 0.5f;
            Origin = newOrigin + delta;
        }

        public override void Draw(IGraphics graphics)
        {
            if (Source.Origin == Target.Origin)
            {
                var v = (Origin + Source.Origin) * 0.5f;
                var r = MathF.Max(1.0f, Float2.Distance(Origin, v) / 2);
                var ul = new Float2 { X = v.X - r, Y = v.Y - r };

                graphics.DrawArc(Tint.White, 
                    new Float4 { 
                        X = ul.X - r, 
                        Y = ul.Y - r, 
                        W = 4 * r, 
                        H = 4 * r 
                    });
            }
            else
            {
                graphics.DrawArrow(Tint.White,
                    new List<Float2> {
                    Source.Origin,
                    Origin,
                    Target.Origin },
                    Dual, headDistance: 2);
            }

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
