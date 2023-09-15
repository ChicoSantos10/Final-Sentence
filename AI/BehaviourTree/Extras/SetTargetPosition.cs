using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class SetTargetPosition : ActionNode
    {
        [SerializeField] bool isAbsolute;
        [SerializeField] Vector2 targetPosition;
        
        protected override State OnUpdate(Context context)
        {
            Vector2 target;

            if (isAbsolute)
                target = targetPosition;
            else
                target = (Vector2)context.Entity.transform.position + targetPosition;

            context.Blackboard.SetValue("Target", target);
            
            return State.Success;
        }

        protected override void Initialize(Context context)
        {
        }

        protected override void OnTerminate(Context context)
        {
        }
    }
}
