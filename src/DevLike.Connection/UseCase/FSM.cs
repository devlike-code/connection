
namespace connection.UseCase
{
    public delegate void StateTransitionSucceeded(string oldState, string transition, string newState);
    public delegate void StateTransitionFailed(string oldState, string transition);

    public class FSM
    {
        public event StateTransitionSucceeded OnStateTransitionSucceeded;
        public event StateTransitionFailed OnStateTransitionFailed;

        protected HashSet<string> Nodes = new();
        protected Dictionary<int, string> Index = new();
        protected Dictionary<string, Dictionary<string, string>> Transitions = new();
        
        protected string CurrentState = "";

        public FSM(GraphData data)
        {
            foreach (var item in data.Data)
            {
                switch (item.Kind)
                {
                    case NodeType.Dot:
                        {
                            var label = item.Tags.GetValueOrDefault("Label", "empty");

                            // TODO: replace with tag `EntryNode`
                            if (label.StartsWith("[") && label.EndsWith("]"))
                            {
                                label = label.Substring(0, label.Length - 1).Substring(1);
                                if (CurrentState == "")
                                {
                                    CurrentState = label;
                                }
                            }

                            Nodes.Add(label);
                            if (!Index.ContainsKey(item.Id))
                            {
                                Index.Add(item.Id, label);
                            }

                            if (!Transitions.ContainsKey(label))
                            {
                                Transitions.Add(label, new());
                            }
                        }
                        break;

                    case NodeType.Link:
                        {
                            var label = item.Tags.GetValueOrDefault("Label", "*");
                            var src = Index[item.SourceId];
                            var tgt = Index[item.TargetId];

                            Transitions[src].Add(label, tgt);
                            if (item.Tags.ContainsKey("BothWays"))
                            {
                                Transitions[tgt].Add(label, src);
                            }
                        }

                        break;
                }
            }
        }

        public void Trigger(string transition)
        {
            if (Transitions[CurrentState].ContainsKey(transition))
            {
                var newState = Transitions[CurrentState][transition];
                OnStateTransitionSucceeded?.Invoke(CurrentState, transition, newState);
                CurrentState = newState;
            }
            else
            {
                OnStateTransitionFailed?.Invoke(CurrentState, transition);
            }
        }
    }
}
