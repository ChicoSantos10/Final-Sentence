using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptable_Objects.Punishments
{
    [CreateAssetMenu(menuName = nameof(Punishment) + "/" + nameof(WarPunishment))]
    public class WarPunishment : Punishment
    {
        [SerializeField] GameObject enemy;
        [SerializeField] int maxEnemies;
        
        protected override void OnBegin()
        {
            for (int i = 0; i < Random.Range(1, maxEnemies + 1); i++)
            {
                Vector2 position = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));

                Instantiate(enemy, position, quaternion.identity);
            }
        }

        protected override void OnEnd()
        {
        }
    }
}
