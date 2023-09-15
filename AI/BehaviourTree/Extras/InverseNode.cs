using System;
using System.Diagnostics;
using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    /// <summary>
    /// Inverts the result of its child node
    /// </summary>
    public class InverseNode : DecoratorNode
    {
        protected override State OnUpdate(Context context)
        {
            return child.Execute(context) switch
            {
                State.Success => State.Failure,
                State.Running => State.Running,
                State.Failure => State.Success,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
