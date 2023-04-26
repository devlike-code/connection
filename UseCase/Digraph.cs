using System.Xml.Linq;

namespace connection.UseCase
{
    public class Digraph
    {
        public class DigraphNode
        {
            private Digraph parent;

            public DigraphNode(Digraph digraph, GraphDataLine item)
            {
                parent = digraph;

                switch (item.Kind)
                {
                    case NodeType.Dot:
                        digraph.Nodes.Add(item.Id);
                        Id = item.Id;
                        Source = item.Id;
                        Target = item.Id;
                        Label = item.Tags.GetValueOrDefault("Label", "");
                        break;

                    default:
                    case NodeType.Link:
                        digraph.Links.Add(item.Id);
                        Id = item.Id;
                        Source = item.SourceId;
                        Target = item.TargetId;
                        Bidirectional = item.Tags.ContainsKey("BothWays");
                        Label = item.Tags.GetValueOrDefault("Label", "");

                        if (!digraph.EdgesOut.ContainsKey(Source))
                            digraph.EdgesOut.Add(Source, new());
                        digraph.EdgesOut[Source].Add(Target, this);

                        if (Bidirectional)
                        {
                            if (!digraph.EdgesOut.ContainsKey(Target))
                                digraph.EdgesOut.Add(Target, new());
                            digraph.EdgesOut[Target].Add(Source, this);
                        }

                        if (!digraph.EdgesIn.ContainsKey(Target))
                            digraph.EdgesIn.Add(Target, new());
                        digraph.EdgesIn[Target].Add(Source, this);

                        if (Bidirectional)
                        {
                            if (!digraph.EdgesIn.ContainsKey(Source))
                                digraph.EdgesIn.Add(Source, new());
                            digraph.EdgesIn[Source].Add(Target, this);
                        }

                        break;
                }
            }

            public readonly int Id;
            public readonly int Source;
            public readonly int Target;
            public readonly bool Bidirectional;
            public readonly string Label = "";

            public bool IsIdentity => (Source == Target) && (Source == Id);
            public bool IsRelation => Source != Target;

            public int CountOutgoing
            {
                get
                {
                    if (!parent.EdgesOut.ContainsKey(this.Id)) return 0;
                    return parent.EdgesOut[this.Id].Count;
                }
            }

            public int CountIncoming
            {
                get
                {
                    if (!parent.EdgesIn.ContainsKey(this.Id)) return 0;
                    return parent.EdgesIn[this.Id].Count;
                }
            }
        }

        public SparseSet Nodes;
        public SparseSet Links;
        public Dictionary<int, DigraphNode> Index;
        public Dictionary<int, Dictionary<int, DigraphNode>> EdgesOut;
        public Dictionary<int, Dictionary<int, DigraphNode>> EdgesIn;        

        public Digraph(GraphData graph, int maxSize = 100) 
        {
            Nodes = new(maxSize);
            Links = new(maxSize);
            Index = new();
            EdgesOut = new();
            EdgesIn = new();

            foreach (var item in graph.Data)
            {
                if (item.Kind == NodeType.Label) break;
                DigraphNode node = new DigraphNode(this, item);
                Index.Add(node.Id, node);
            }
        }
    }
}
