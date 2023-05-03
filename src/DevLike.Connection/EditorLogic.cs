
using DevLike.Connection.Nodes;

namespace DevLike.Connection;

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
    StartMoving,
    CancelMoving,
    CreateNode,
    CancelSelect,
    CancelConnect,
    StartRectSelect,
    EndRectSelect,
    StartConnect,
    EndConnect,
    EnterTextEdit,
    CancelTextEdit,
    CreateLoop,
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
            else if (transition == "dblclick node")
            {
                if (GraphInternals.Hovered.First() is LabelNode)
                    Logic.SendOutputEvent(LogicOutputEvent.EnterTextEdit);
                else
                    Logic.SendOutputEvent(LogicOutputEvent.CreateLoop);
            }
            else if (transition.StartsWith("mousedown right"))
            {
                Logic.SendOutputEvent(LogicOutputEvent.StartMoving);
            }
            else if (transition.StartsWith("mouseup right"))
            {
                Logic.SendOutputEvent(LogicOutputEvent.CancelMoving);
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
            if (transition == "esc" || transition == "mouseup left empty")
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
            if (transition == "mousedown left empty")
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
            if (transition == "mousedown left node")
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
            if (transition == "mouseup left node")
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
    public EditorLogicNode CurrentNode;

    public TriggerOutputEvent? OnTriggerOutputEvent;

    protected NodeModeLogicNode modeNode;
    protected RectSelectLogicNode modeRectSelect;
    protected ConnectLogicNode modeConnect;
    protected InvalidationLogicNode invalidation;

    public bool IsMoving = false;
    public Float2 MoveOrigin = Float2.Zero;

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
        modeNode.Connect("dblclick node", modeNode);
        modeNode.Connect("mousedown left empty", modeRectSelect);
        modeNode.Connect("mousedown left node", modeConnect);
        modeNode.Connect("mousedown right empty", modeNode);
        modeNode.Connect("mousedown right node", modeNode);
        modeNode.Connect("mouseup right empty", modeNode);
        modeNode.Connect("mouseup right node", modeNode);

        modeRectSelect.Connect("esc", modeNode);
        modeRectSelect.Connect("mouseup left empty", invalidation);
        modeRectSelect.Connect("mouseup left node", invalidation);

        modeConnect.Connect("esc", modeNode);
        modeConnect.Connect("mouseup left empty", modeNode);
        modeConnect.Connect("mouseup left node", invalidation);

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
