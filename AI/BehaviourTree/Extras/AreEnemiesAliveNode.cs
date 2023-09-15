using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.BehaviourTree.Module;

public class AreEnemiesAliveNode : ActionNode
{
    List<GameObject> enemiesAlive;

    protected override void Initialize(Context context)
    {
        context.Blackboard.TryGetValue(SpawnObjectsInCircleNode.enemiesID, out enemiesAlive);      
    }

    protected override State OnUpdate(Context context)
    {
        if (enemiesAlive == null)
            return State.Failure;

        foreach (GameObject enemy in enemiesAlive)
        {
            if(enemy != null)
            {
                return State.Success;
            }           
        }

        return State.Failure;      
    }
}
