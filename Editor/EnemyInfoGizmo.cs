using Enemies;
using Player;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class EnemyInfoGizmo 
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        static void DrawEnemyInfo(Enemy enemy, GizmoType gizmoType)
        {
            Handles.color = Color.white;

            Handles.Label(enemy.transform.position + Vector3.up + Vector3.left, $"{enemy.gameObject.name} Hp: {enemy.Stats[VariableStat.StatType.Hp]}");
        }
    }
}
