using System;
using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class IsSpawnTooFarNode : ActionNode
    {
        [SerializeField, Tooltip("How far is too far")] float dist = 5;

        Vector3 _spawnPos;

        public override void OnAwake(Context context)
        {
            _spawnPos = context.Entity.transform.position;
        }

        protected override State OnUpdate(Context context)
        {
            return Vector3.Distance(context.Entity.transform.position, _spawnPos) >= dist ? State.Success : State.Failure;
        }
    }
}
