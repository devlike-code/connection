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
        private static int Count = 0;

        public event NodeMove? OnNodeMove;
        public event NodeTagChanged? OnNodeTagChanged;

        public Node? Source = null;
        public Node? Target = null;

        public int Depth = 0;
        public bool Dual = false;
        public Float2 Origin;
        public Dictionary<string, string> Tags = new();

        public Float2 RelativeLabelPosition;

        public Tint Color;
        public bool Alive;
        public bool CanConnect;
        public bool CanDelete;

        public readonly int Id;

        public Node(Float2 xy, bool canConnect, bool canDelete)
        {
            Alive = true;
            
            Origin = xy;
            CanConnect = canConnect;
            CanDelete = canDelete;

            Id = ++Count;

            Source = this;
            Target = this;
        }

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

            OnNodeMove?.Invoke(this, oldPos, Origin);
        }

        public int CompareTo(Node? other)
        {
            return -this.Depth.CompareTo(other?.Depth);
        }
    }
}
