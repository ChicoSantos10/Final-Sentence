using System;
using System.Collections.Generic;
using AI.BehaviourTree.Module;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace AI.BehaviourTree.Extras
{
    public abstract class ParallelNode : CompositeNode
    {
        [SerializeField] protected FinishMode mode;
        
        protected List<Node> RunningChildren;
        
        public override void OnAwake(Context context)
        {
            RunningChildren = new List<Node>(children.Count);
        }
        
        protected override void OnTerminate(Context context)
        {
            foreach (Node child in RunningChildren)
            {
                child.Terminate(context);
            }
        }
        
        protected override State OnUpdate(Context context)
        {
            return mode switch
            {
                FinishMode.ImmediateMode => ImmediateModeExecute(context),
                FinishMode.Delayed => DelayedModeExecute(context),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected abstract State ImmediateModeExecute(Context context);
        protected abstract State DelayedModeExecute(Context context);

        protected enum FinishMode
        {
            ImmediateMode,
            Delayed
        }
    }
}
