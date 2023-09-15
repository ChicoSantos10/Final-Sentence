#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace AI.BehaviourTree.Module
{
    public abstract class ActionNode : Node
    {
#if UNITY_EDITOR

        public sealed override bool TryCreateOutputPorts(UnityEditor.Experimental.GraphView.Node node,
            Orientation orientation, out Port port)
        {
            port = null;
            
            return false;
        }
        
#endif
    }
}
