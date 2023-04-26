using connection.Nodes;

namespace connection
{
    public enum NodeTagChange
    {
        Add, Remove, Edit
    }

    public delegate void NodeMove(Node movedNode, Float2 oldPos, Float2 newPos);
    public delegate void NodeTagChanged(Node movedNode, string key, string value, NodeTagChange change);    

    public abstract class Node : IComparable<Node>
    {
        public static int Count = 1;

        public event NodeMove? OnNodeMove;
        public event NodeTagChanged? OnNodeTagChanged;

        public Node? Source = null;
        public Node? Target = null;

        public int Depth = 0;
        public int ExportDepth = 0;

        public int RecurseExportDepth()
        {
            var selfSource = Source == null || Source == this;
            var selfTarget = Target == null || Target == this;
            var s = selfSource ? 1 : Source.RecurseExportDepth() * Source.Id;
            var t = selfTarget ? 1 : Target.RecurseExportDepth() * Target.Id;
            return s * t;
        }

        public bool Dual = false;
        public Float2 Origin;
        public Dictionary<string, string> Tags = new();

        public Float2 RelativeLabelPosition;

        public Tint Color;
        public bool Alive;
        public bool CanConnect;
        public bool CanDelete;

        public int Id;

        public Node(Float2 xy, bool canConnect, bool canDelete)
        {
            Alive = true;
            
            Origin = xy;
            this.AddTag("Position", $"{Origin.X},{Origin.Y}");

            CanConnect = canConnect;
            CanDelete = canDelete;

            Id = Count++;

            Source = this;
            Target = this;
        }

        public abstract string Export();

        public void AddTag(string key, string value)
        {
            if (Tags.ContainsKey(key))
            {
                Tags.Remove(key);
                Tags.Add(key, value);
                OnNodeTagChanged?.Invoke(this, key, value, NodeTagChange.Edit);

            }
            else
            {
                Tags.Add(key, value);
                OnNodeTagChanged?.Invoke(this, key, value, NodeTagChange.Add);
            }
        }

        public string GetTag(string key)
        {
            return Tags.GetValueOrDefault(key, "");
        }

        public void RemoveTag(string key, string value)
        {
            if (Tags.ContainsKey(key))
            {
                Tags.Remove(key);
                OnNodeTagChanged?.Invoke(this, key, value, NodeTagChange.Remove);
            }
        }

        protected Float4 LastBounds;

        public Float4 GetBounds() => LastBounds;

        public abstract Float4 GetBounds(IGraphics graphics);
        public abstract void Draw(IGraphics graphics);

        public bool IsUnder(IGraphics graphics, Float2 xy) 
            => GetBounds(graphics).Contains(xy);

        public bool IsUnder(IGraphics graphics, Float4 rect) 
            => GetBounds(graphics).IntersectsWith(rect);

        public void Move(float dx, float dy)
        {
            var oldPos = new Float2 { X = Origin.X, Y = Origin.Y };
            Origin.X += dx;
            Origin.Y += dy;

            this.AddTag("Position", $"{Origin.X}, {Origin.Y}");
            OnNodeMove?.Invoke(this, oldPos, Origin);
        }

        public int CompareTo(Node? other)
        {
            return -this.Depth.CompareTo(other?.Depth);
        }

        public static Node Create(ref List<Node> nodes, NodeType kind, int id, int src, int tgt, Dictionary<string, string> tags)
        {            
            var xy = (tags.ContainsKey("Position") ? tags["Position"] : "50,50").Split(",", 2);
            var pos = new Float2 { X = float.Parse(xy[0]), Y = float.Parse(xy[1]) };

            Node? node = null;

            switch (kind)
            {
                case NodeType.Dot:
                    {
                        node = new DotNode(pos);
                        node.Id = id;
                        node.Origin = pos;
                    }
                    break;
                case NodeType.Link:
                    {
                        var source = nodes.Find(x => x.Id == src);
                        var target = nodes.Find(x => x.Id == tgt);

                        node = new LinkNode(source, target, tags.ContainsKey("BothWays"));
                        node.Id = id;
                        node.Origin = pos;
                    }
                    break;
                case NodeType.Label:
                    {
                        var source = nodes.Find(x => x.Id == src);
                        node = new LabelNode(pos, source);
                        node.Id = id;
                        node.Origin = pos;
                    }
                    break;
                default:
                    throw new Exception("Unknown node type.");
            }

            foreach(var entry in tags)
            {
                node.AddTag(entry.Key, entry.Value);
            }

            return node;
        }
    }
}
