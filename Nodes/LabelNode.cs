
namespace connection.Nodes
{
    public class LabelNode : Node
    {
        public LabelNode(Float2 xy, Node? source = null) : base(xy, false, false)
        {
            Source = source;
            if (source != null)
            {
                this.OnNodeTagChanged += LabelNode_OnNodeTagChanged;                
                this.AddTag("Label", source.GetTag("Label"));
            }
        }

        private void LabelNode_OnNodeTagChanged(Node movedNode, string key, string value, NodeTagChange change)
        {
            if (movedNode == this && key == "Label" && change == NodeTagChange.Edit)
            {
                if (Source != null)
                {
                    Source.AddTag("Label", value);
                }
            }
        }

        public override void Draw(IGraphics graphics)
        {
            if (IsUnder(graphics, Rendering.Mouse))
            {
                GraphInternals.Hovered.Add(this);
                graphics.FillRectangle(Tint.White, GetBounds(graphics) + 2.0f);
                graphics.DrawText(Tint.DarkGrey, Origin + Source.Origin, GetTag("Label"));
            }
            else
            {
                GraphInternals.Hovered.Remove(this);
                graphics.FillRectangle(Tint.LightGrey, GetBounds(graphics), 0.5f);
                graphics.DrawText(Tint.White, Origin + Source.Origin, GetTag("Label"));
            }

            if (GraphInternals.Selected.Contains(this))
            {
                graphics.DrawRectangle(Tint.Yellow, GetBounds(graphics));                
            }
        }

        public override Float4 GetBounds(IGraphics graphics)
        {
            var measure = graphics.GetStringWidth(this.GetTag("Label"));            
            LastBounds = new Float4
            {
                X = Source.Origin.X + Origin.X,
                Y = Source.Origin.Y + Origin.Y,
                W = Math.Max(10.0f, measure.X),
                H = Math.Max(14.0f, measure.Y),                
            };

            return LastBounds;
        }
    }
}
