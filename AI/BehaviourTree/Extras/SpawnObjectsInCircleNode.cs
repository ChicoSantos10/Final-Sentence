using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.BehaviourTree.Module;

public class SpawnObjectsInCircleNode : ActionNode
{
    [SerializeField] List<GameObject> enemies;
    [SerializeField, Min (1)] int quantity;
    [SerializeField, Range (0.1f, 3.0f)] float radius;

    public const string enemiesID = "enemies";

   
    protected override State OnUpdate(Context context)
    {
        if (!context.Blackboard.TryGetValue(enemiesID, out List<GameObject> enemiesAlive))
        {
            enemiesAlive = new List<GameObject>();
            context.Blackboard.SetValue(enemiesID, enemiesAlive);
        }

        for (int i = 0; i < quantity; i++)
        {
        
            Vector2 position = Random.insideUnitCircle * radius;
            Vector2 worldPosition = (Vector2)context.Entity.transform.position + position;

            GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Count)], worldPosition, Quaternion.identity);
            enemiesAlive.Add(enemy);
        }
        return State.Success;
    }
}
