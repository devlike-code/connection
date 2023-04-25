using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace connection.Nodes
{
    public class DotNode : Node
    {
        public DotNode(Float2 xy) : base(xy, true, true)
        {
            AddTag("Label", $"Node {Id}");
        }

        public override void Draw(IGraphics graphics)
        {
            graphics.FillCircle(Tint.White, Origin, 8);
            graphics.FillCircle(this.Color, Origin, 7);
            graphics.FillCircle(Tint.White, Origin, 6);
            graphics.FillCircle(this.Color, Origin, 3);

            if (IsUnder(graphics, Rendering.Mouse))
            {
                GraphInternals.Hovered.Add(this);
                graphics.DrawCircle(Tint.White, Origin, 10);
            }
            else
            {
                GraphInternals.Hovered.Remove(this);
            }

            if (GraphInternals.Selected.Contains(this))
            {
                graphics.DrawCircle(Tint.Yellow, Origin, 12);
            }
        }

        public override Float4 GetBounds(IGraphics graphics)
        {
            LastBounds = new Float4 { X = Origin.X - 10, Y = Origin.Y - 10, W = 20, H = 20 };
            return LastBounds;
        }
    }
}
