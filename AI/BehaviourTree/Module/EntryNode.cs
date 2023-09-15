using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace AI.BehaviourTree.Module
{
    public sealed class EntryNode : Node
    {
        [HideInInspector] public Node child;

        protected override State OnUpdate(Context context) => child.Execute(context);

        protected override void Initialize(Context context)
        {
        }

        protected override void OnTerminate(Context context)
        {
        }

        public override Node Clone()
        {
            EntryNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }

#if UNITY_EDITOR
        public override bool TryCreateOutputPorts(UnityEditor.Experimental.GraphView.Node node, Orientation orientation, out Port port)
        {
            port = node.InstantiatePort(orientation, Direction.Output,
                Port.Capacity.Single, typeof(bool));

            return true;
        }
#endif
    }
}
