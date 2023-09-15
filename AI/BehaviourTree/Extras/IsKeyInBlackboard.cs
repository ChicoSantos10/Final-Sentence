using AI.BehaviourTree.Module;
using Scriptable_Objects;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class IsKeyInBlackboard : DecoratorNode
    {
        [SerializeField] string key;
        
        protected override State OnUpdate(Context context)
        {
            if (!context.Blackboard.TryGetValue(key, out bool isSet) || !isSet) 
                return State.Failure;

            //return child.Execute(context); 
            child.Execute(context);
            return State.Success;
        }
    }
}
