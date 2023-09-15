using AI.BehaviourTree.Module;

namespace AI.BehaviourTree.Extras
{
    /// <summary>
    /// Repeats indefinitely
    /// </summary>
    public class RepeatNode : DecoratorNode
    {
        protected override State OnUpdate(Context context)
        {
            child.Execute(context);
            return State.Running;
        }


        protected override void OnTerminate(Context context)
        {
        }
    }
}
