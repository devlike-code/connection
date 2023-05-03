using connection.Nodes;

using System.IO;
using System.Xml.Linq;

namespace connection
{
    // maybe remove?
    public static class GraphInternals
    {
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


    public class GraphEditorTrait
    {
        public List<Node> Nodes = new();
        
        public EditorLogic Logic = new();
        public bool IsLinkingBothWays = false;
        public Float4? SelectingRect = null;
        public Node? ConnectingStartNode = null;
        public Node? EditingLabelNode = null;

        public void LoadFromFile(string filename)
        {
            Reset();

            foreach (var line in GraphData.LoadFromFile(filename).Data)
            {
                var node = Node.Create(ref Nodes, line.Kind, line.Id, line.SourceId, line.TargetId, line.Tags);
                Nodes.Add(node);
            }
        }

        public void SaveToFile(string filename) 
        {
            Nodes.Sort((x, y) =>
            {
                if (x.ExportDepth == y.ExportDepth)
                    return x.RecurseExportDepth().CompareTo(y.RecurseExportDepth());
                else
                    return -x.ExportDepth.CompareTo(y.ExportDepth);
            });

            var export = new List<string>();

            export.Add($"type\t|\tid\t|\tsrc\t|\ttgt\t|\ttags");
            foreach (var node in Nodes)
                export.Add(node.Export());

            File.WriteAllLines(filename, export.ToArray());
        }

        public GraphEditorTrait()
        {
            Logic.OnTriggerOutputEvent += PerformStateChange;
        }

        public void PerformStateChange(LogicOutputEvent e)
        {
            switch (e)
            {
                case LogicOutputEvent.CancelTextEdit:
                    EditingLabelNode = null;
                    break;
                case LogicOutputEvent.DeselectAll:
                    EditingLabelNode = null;
                    ConnectingStartNode = null;
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
                            if (n != null && n.Alive && n.GetBounds().IntersectsWith(SelectingRect.Value.Normalized()))
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
                        
                        if ((end != ConnectingStartNode) && end.CanConnect)
                        {
                            AddLinkNode(ConnectingStartNode, end, IsLinkingBothWays);
                        }
                        ConnectingStartNode = null;
                        Logic.Queue("");
                    }
                    break;
                case LogicOutputEvent.StartMoving:
                    if (GraphInternals.Hovered.Count > 0)
                    {
                        GraphInternals.Selected.Clear();
                        GraphInternals.Selected.Add(GraphInternals.Hovered.First());
                    }

                    foreach (var node in GraphInternals.Selected)
                    {
                        if (node is LinkNode link)
                            link.SideMoved = false;
                    }

                    Logic.IsMoving = true;
                    Logic.MoveOrigin = Rendering.Mouse;
                    break;
                case LogicOutputEvent.CancelMoving:
                    Logic.IsMoving = false;
                    break;
                case LogicOutputEvent.EnterTextEdit:
                    {
                        var label = GraphInternals.Hovered.First() as LabelNode;
                        GraphInternals.Selected.Clear();
                        GraphInternals.Selected.Add(label);
                        EditingLabelNode = label;
                    }
                    break;
                case LogicOutputEvent.CreateLoop:
                    {
                        var node = GraphInternals.Hovered.First();
                        if (node.CanConnect)
                        {
                            AddLinkNode(node, node);
                        }                        
                        Logic.Queue("esc");
                    }
                    break;
            }
        }

        public Float2 UpdateMouseMoveDelta(Float2 newPosition)
        {
            var delta = newPosition - Logic.MoveOrigin;
            Logic.MoveOrigin = newPosition;

            foreach (var node in GraphInternals.Selected)
            {
                if (node is LinkNode link)
                    link.SideMoved = false;
            }

            foreach (var node in GraphInternals.Selected)
            {
                if (!(node is LabelNode) || (node is LabelNode && !GraphInternals.Selected.Contains(node.Source)))
                {
                    node.Move(delta.X, delta.Y);
                }
            }

            return delta;
        }

        public Node AddDotNode(Float2 position, bool withLabel = true)
        {
            var node = new DotNode(position);
            Nodes.Add(node);

            if (withLabel)
            {
                var label = new LabelNode(new Float2 { X = 10, Y = 10 }, node);
                Nodes.Add(label);
            }

            return node;
        }

        public Node AddLinkNode(Node a, Node b, bool bothWays = false, bool withLabel = true)
        {
            var node = new LinkNode(a, b, bothWays);
            Nodes.Add(node);

            if (withLabel)
            {
                var label = new LabelNode(new Float2 { X = 10, Y = 0 }, node);
                Nodes.Add(label);
            }

            return node;
        }

        public Node AddLabelNode(Node a)
        {
            var node = new LabelNode(new Float2 { X = 10, Y = 0 }, a);
            Nodes.Add(node);

            return node;
        }

        public void Select(params Node[] nodes)
        {
            GraphInternals.Selected.Clear();
            
            foreach (var node in nodes)
                GraphInternals.Selected.Add(node);
        }

        public void UpdateMousePosition(Float2 xy)
        {
            Rendering.Mouse = xy;
        }

        public void Reset()
        {
            SelectingRect = null;
            ConnectingStartNode = null;
            EditingLabelNode = null;
            Nodes.Clear();
            Logic.Reset();
            
            DotNode.Identifier = 'a';
            Node.Count = 0;
        }

        public void DeleteSelected()
        {
            HashSet<int> idsToDelete = new();
            foreach (var n in GraphInternals.Selected)
            {
                idsToDelete.Add(n.Id);
            }

            foreach (var n in Nodes)
            {
                if (idsToDelete.Contains(n.Id)) continue;
                if (n.Source != null)
                {
                    if (idsToDelete.Contains(n.Source.Id))
                    {
                        idsToDelete.Add(n.Id);
                    }
                }

                if (n.Target != null)
                {
                    if (idsToDelete.Contains(n.Target.Id))
                    {
                        idsToDelete.Add(n.Id);
                    }
                }
            }

            Nodes.RemoveAll(x => idsToDelete.Contains(x.Id));
            GraphInternals.Selected.Clear();
        }
    }
}