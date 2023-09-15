
using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace AI.BehaviourTree.Module
{
    public abstract class DecoratorNode : Node
    {
        [HideInInspector] public Node child;

        [SerializeField] AbortMode abortMode;
        [SerializeField] EventType evtType = EventType.ValueChanged;
        [FormerlySerializedAs("id")] [SerializeField, Tooltip("The name of the event to subscribe for aborting purposes")] string eventID;

        public override Node Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            node.child.parent = node;
            return node;
        }

        protected override void Initialize(Context context)
        {
            context.Blackboard.UnsubscribeEvent(eventID, OnValueChanged, evtType);
            context.Blackboard.SubscribeEvent(eventID, OnValueChanged, evtType);
        }

        void OnValueChanged()
        {
            tree.Abort(this, abortMode);
        }

#if UNITY_EDITOR

        public sealed override bool TryCreateOutputPorts(UnityEditor.Experimental.GraphView.Node node,
            Orientation orientation, out Port port)
        {
            port = node.InstantiatePort(orientation, Direction.Output,
                Port.Capacity.Single, typeof(bool));

            return true;
        }

#endif
    }
}
