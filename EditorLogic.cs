
namespace connection
{
    public abstract class EditorLogicNode
    {
        protected EditorLogic Logic { get; set; }

        public EditorLogicNode(EditorLogic logic) 
        {
            Logic = logic;
        }

        public abstract void OnEnter(EditorLogicNode from, string transition);

        public Dictionary<string, EditorLogicNode> Transitions = new();

        public void Connect(string transition, EditorLogicNode to)
        {
            Transitions.Add(transition, to);
        }

        public bool Trigger(string transition, out EditorLogicNode next)
        {
            if (Transitions.ContainsKey(transition))
            {
                next = Transitions[transition];
                return true;
            }
            else
            {
                next = this;
                return false;
            }
        }
    }

    public enum LogicOutputEvent
    {
        DeselectAll,
        CreateNode,
        CancelSelect,
        CancelConnect,
        StartRectSelect,
        EndRectSelect,
        StartConnect,
        EndConnect,
    }

    public class NodeModeLogicNode : EditorLogicNode
    {
        public NodeModeLogicNode(EditorLogic logic) : base(logic) { }

        public override void OnEnter(EditorLogicNode from, string transition)
        {
            if (from is NodeModeLogicNode)
            {
                if (transition == "esc")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.DeselectAll);
                }
                else if (transition == "dblclick empty")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.CreateNode);
                }
            }
            else if (from is RectSelectLogicNode)
            {
                if (transition == "esc")
                {                    
                    Logic.SendOutputEvent(LogicOutputEvent.CancelSelect);
                }
            }
            else if (from is ConnectLogicNode)
            {
                if (transition == "esc" || transition == "mouseup empty")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.CancelConnect);
                }
            }
            else if (from is InvalidationLogicNode)
            {
                // everything okay
            }
        }
    }

    public class RectSelectLogicNode : EditorLogicNode
    {
        public RectSelectLogicNode(EditorLogic logic) : base(logic) { }

        public override void OnEnter(EditorLogicNode from, string transition)
        {
            if (from is NodeModeLogicNode)
            {
                if (transition == "mousedown empty")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.StartRectSelect);
                }
            }
        }
    }

    public class ConnectLogicNode : EditorLogicNode
    {
        public ConnectLogicNode(EditorLogic logic) : base(logic) { }

        public override void OnEnter(EditorLogicNode from, string transition)
        {
            if (from is NodeModeLogicNode)
            {
                if (transition == "mousedown node")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.StartConnect);
                }
            }
        }
    }

    public class InvalidationLogicNode : EditorLogicNode
    {
        public InvalidationLogicNode(EditorLogic logic) : base(logic) { }

        public override void OnEnter(EditorLogicNode from, string transition)
        {
            if (from is ConnectLogicNode)
            {
                if (transition == "mouseup node")
                {
                    Logic.SendOutputEvent(LogicOutputEvent.EndConnect);
                }
            }
            else if (from is RectSelectLogicNode)
            {
                if (transition.StartsWith("mouseup"))
                {
                    Logic.SendOutputEvent(LogicOutputEvent.EndRectSelect);
                }
            }
        }
    }

    public delegate void TriggerOutputEvent(LogicOutputEvent e);

    public class EditorLogic
    {
        public EditorLogicNode CurrentNode = null;

        public TriggerOutputEvent? OnTriggerOutputEvent;
        
        protected NodeModeLogicNode modeNode;
        protected RectSelectLogicNode modeRectSelect;
        protected ConnectLogicNode modeConnect;
        protected InvalidationLogicNode invalidation;

        public bool IsRectSelecting() => CurrentNode == modeRectSelect;

        public bool IsConnecting() => CurrentNode == modeConnect;

        private Queue<string> queuedTransitions = new();
        public EditorLogic()
        {
            modeNode = new NodeModeLogicNode(this);
            modeRectSelect = new RectSelectLogicNode(this);
            modeConnect = new ConnectLogicNode(this);
            invalidation = new InvalidationLogicNode(this);

            modeNode.Connect("esc", modeNode);
            modeNode.Connect("dblclick empty", modeNode);
            modeNode.Connect("mousedown empty", modeRectSelect);
            modeNode.Connect("mousedown node", modeConnect);

            modeRectSelect.Connect("esc", modeNode);
            modeRectSelect.Connect("mouseup empty", invalidation);
            modeRectSelect.Connect("mouseup node", invalidation);

            modeConnect.Connect("esc", modeNode);
            modeConnect.Connect("mouseup empty", modeNode);
            modeConnect.Connect("mouseup node", invalidation);

            invalidation.Connect("", modeNode);

            CurrentNode = modeNode;
        }

        public void Trigger(string transition)
        {
            if (CurrentNode != null)
            {
                if (CurrentNode.Trigger(transition, out var next))
                {
                    Console.WriteLine($"{CurrentNode} -{transition}-> {next}");
                    next.OnEnter(CurrentNode, transition);
                    CurrentNode = next;

                    if (queuedTransitions.Count > 0)
                    {
                        Trigger(queuedTransitions.Dequeue());
                    }
                }
                else
                {
                    Console.WriteLine($"{CurrentNode} -{transition}-> X");
                }
            }
        }

        public void Queue(string transition)
        {
            queuedTransitions.Enqueue(transition);
        }

        public void SendOutputEvent(LogicOutputEvent e)
        {
            OnTriggerOutputEvent?.Invoke(e);
        }

        public void Reset()
        {
            CurrentNode = modeNode;
        }
    }
}
