using Player;
using UnityEngine;

namespace Combat
{
    public static class CombatManager
    {
        static readonly Collider2D[] _collisions = new Collider2D[10];
        
        /// <summary>
        /// Attacks an area and does damage to everyone caught in it
        /// </summary>
        /// <param name="attacker">The transform of the attacker</param>
        /// <param name="dir">The direction of the attack</param>
        /// <param name="attackRange">The range of the attack</param>
        /// <param name="layer">The layer to check collisions to</param>
        /// <param name="stats">The stats of the attacker</param>
        public static void MeleeAttack(Transform attacker, Vector2 dir, float attackRange, LayerMask layer, Stats stats)
        {
            dir.Normalize();
            GetAttackBox(out Vector2 topLeft, out Vector2 botRight);
            int numberEnemiesHit = Physics2D.OverlapAreaNonAlloc(topLeft, botRight, _collisions, layer);

            for (int index = 0; index < numberEnemiesHit; index++)
            {
                Collider2D enemyCollider = _collisions[index];
                ICombatUnit unit = enemyCollider.gameObject.GetComponent<ICombatUnit>();

                float damage = ComputeDamage(stats[Stat.StatType.Attack], unit.Stats[Stat.StatType.Defense]);
                
                Debug.Log($"{unit} took {damage} points of damage");
                
                unit.TakeDamage(damage); 
            }
            
            void GetAttackBox(out Vector2 _topLeft, out Vector2 _botRight)
            {
                Vector3 attackerPosition = attacker.position;
                _topLeft = (Vector2) attackerPosition + dir * attackRange + new Vector2(attackRange, attackRange);
                _botRight = (Vector2) attackerPosition + dir * attackRange - new Vector2(attackRange, attackRange);
            }
            
        }
        
        /// <summary>
        /// Returns the damage dealt. Min is 1
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="defense"></param>
        /// <returns></returns>
        static float ComputeDamage(float attack, float defense) => Mathf.Max(attack - defense, 1); 
    }
}
