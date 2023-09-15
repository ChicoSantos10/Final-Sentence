using System.Collections;
using AI.BehaviourTree.Module;
using Enemies;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class AttackNode : ActionNode
    {
        protected override State OnUpdate(Context context)
        {
            if (!context.Blackboard.TryGetValue("Target", out Vector3 targetPos))
                return State.Failure;
            
            context.Entity.GetComponent<Enemy>().Attack(targetPos - context.Entity.transform.position);

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
