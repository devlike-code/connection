using connection.Nodes;

namespace connection
{
    // maybe remove?
    public static class GraphInternals
    {
        public static HashSet<int> Serialized = new();
        public static bool MinifySerialization;

        public static HashSet<Node> Moved = new();
        public static HashSet<Node> Hovered = new();
        public static HashSet<Node> Selected = new();
    }

    public enum EditorMode
    {
        Node,
        Text,
        Selection,
        Connection
    }

    public delegate void GraphAddNode(object sender, Node node);
    public delegate void GraphDeleteNodes(object sender, List<Node> nodes);
    public delegate void GraphHighlightNodes(object sender, List<Node> nodes);
    public delegate void GraphSelectionChanged(object sender, List<Node> nodes);
    public delegate void GraphEditorModeChanged(object sender, EditorMode mode);

    public class ConnectionGraphEditorTrait
    {
        public readonly List<Node> Nodes = new();

        public EditorLogic Logic = new();
        public Float4? SelectingRect = null;
        public Node? ConnectingStartNode = null;

        public event GraphAddNode? OnAddNode;
        public event GraphDeleteNodes? OnDeleteNodes;
        public event GraphHighlightNodes? OnHighlightNodes;
        public event GraphSelectionChanged? OnSelectionChanged;
        public event GraphEditorModeChanged? OnEditorModeChanged;

        public ConnectionGraphEditorTrait()
        {
            Logic.OnTriggerOutputEvent += PerformStateChange;
        }

        public void PerformStateChange(LogicOutputEvent e)
        {
            switch (e)
            {
                case LogicOutputEvent.DeselectAll: 
                    Select(); 
                    break;
                case LogicOutputEvent.CreateNode: 
                    AddDotNode(Rendering.Mouse); 
                    break;
                case LogicOutputEvent.CancelSelect:
                    Select();
                    SelectingRect = null; 
                    break;
                case LogicOutputEvent.CancelConnect: 
                    ConnectingStartNode = null;
                    break;
                case LogicOutputEvent.StartRectSelect: 
                    SelectingRect = new Float4 { X = Rendering.Mouse.X, Y = Rendering.Mouse.Y, W = 1, H = 1 };
                    break;
                case LogicOutputEvent.EndRectSelect:
                    if (SelectingRect.HasValue)
                    {
                        GraphInternals.Selected.Clear();
                        foreach (var n in Nodes)
                        {
                            if (n != null && n.Alive && n.GetBounds().IntersectsWith(SelectingRect.Value))
                            {
                                GraphInternals.Selected.Add(n);
                            }
                        }
                    }
                    SelectingRect = null;
                    Logic.Queue("");
                    break;
                case LogicOutputEvent.StartConnect:
                    ConnectingStartNode = GraphInternals.Hovered.First();
                    if (!ConnectingStartNode.CanConnect)
                    {
                        ConnectingStartNode = null;
                        Logic.Queue("esc");
                    }
                    break;
                case LogicOutputEvent.EndConnect:
                    {
                        var end = GraphInternals.Hovered.First();
                        if (end.CanConnect)
                        {
                            AddLinkNode(ConnectingStartNode, end);
                        }
                        ConnectingStartNode = null;
                        Logic.Queue("");
                    }
                    break;
            }
        }

        public Node AddDotNode(Float2 position)
        {
            var node = new DotNode(position);
            Nodes.Add(node);

            var label = new LabelNode(new Float2 { X = 10, Y = 10 }, node);
            Nodes.Add(label);

            return node;
        }

        public Node AddLinkNode(Node a, Node b)
        {
            var node = new LinkNode(a, b, false);
            Nodes.Add(node);

            var label = new LabelNode(new Float2 { X = 10, Y = 0 }, node);
            Nodes.Add(label);

            return node;
        }

        public void Select(params Node[] nodes)
        {
            GraphInternals.Selected.Clear();
            
            foreach (var node in nodes)
                GraphInternals.Selected.Add(node);

            OnSelectionChanged?.Invoke(this, nodes.ToList());
        }

        public void UpdateMousePosition(Float2 xy)
        {
            Rendering.Mouse = xy;
        }
    }
}