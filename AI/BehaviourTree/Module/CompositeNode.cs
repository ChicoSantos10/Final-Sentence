using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace AI.BehaviourTree.Module
{
    public abstract class CompositeNode : Node
    {
        [HideInInspector] public List<Node> children = new List<Node>();

        int _current = 0;

        protected bool MoveNext()
        {
            return ++_current < children.Count;
        }

        public void Reset()
        {
            _current = 0;
        }

        public Node Current => children[_current];

        public override Node Clone()
        {
            CompositeNode node = Instantiate(this);

            node.children = children.ConvertAll(c =>
            {
                Node clone = c.Clone();
                clone.parent = node;
                return clone;
            });

            return node;
        }

        protected override void Initialize(Context context)
        {
            Reset();
        }

        protected override void OnTerminate(Context context)
        {
            Reset();
        }

#if UNITY_EDITOR

        public sealed override bool TryCreateOutputPorts(UnityEditor.Experimental.GraphView.Node node,
            Orientation orientation, out Port port)
        {
            port = node.InstantiatePort(orientation, Direction.Output,
                Port.Capacity.Multi, typeof(bool));

            return true;
        }

#endif
    }
}
