using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class LogNode : ActionNode
    {
        [SerializeField, TextArea] string message;

        protected override State OnUpdate(Context context)
        {
            Debug.Log(message);
            
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
