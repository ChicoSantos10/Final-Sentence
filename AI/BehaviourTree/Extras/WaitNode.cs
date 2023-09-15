using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.Behaviour_Tree.Extras
{
    public class WaitNode : ActionNode
    {
        [SerializeField] float duration;
        float startTime;

        protected override State OnUpdate(Context context)
        {
            return Time.time - startTime > duration ? State.Success : State.Running;
        }

        protected override void Initialize(Context context)
        {
            startTime = Time.time;
        }

        protected override void OnTerminate(Context context)
        {
        }
    }
}
