using System;
using AI.BehaviourTree.Module;

namespace AI.BehaviourTree.Extras
{
    /// <summary>
    /// Goes through all children until one fails
    /// </summary>
    public class SequencerNode : CompositeNode
    {
        protected override State OnUpdate(Context context)
        {
            return Current.Execute(context) switch
            {
                State.Success => MoveNext() ? State.Running : State.Success,
                State.Running => State.Running,
                State.Failure => State.Failure,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
