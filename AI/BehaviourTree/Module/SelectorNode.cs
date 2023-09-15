using System;

namespace AI.BehaviourTree.Module
{
    /// <summary>
    /// Goes through the children until one succeeds or there are no more children
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        protected override State OnUpdate(Context context)
        {
            return Current.Execute(context) switch
            {
                State.Success => State.Success,
                State.Running => State.Running,
                State.Failure => MoveNext() ? State.Running : State.Failure,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
