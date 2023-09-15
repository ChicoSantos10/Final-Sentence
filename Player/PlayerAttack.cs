using System;
using Combat;
using Enemies;
using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(AnimationController))]
    public class PlayerAttack : MonoBehaviour, ICombatUnit
    {
        [SerializeField] InputReader inputReader;  
        [SerializeField] GameObject player;
        [SerializeField] LayerMask enemyLayer;
        [SerializeField] PlayerInfo info;
        [SerializeField] float attackRange = 1;
        [SerializeField] EventChannel gameOverChannel;
        
        Vector2 _dir; //dir pra onde esta virado

        AnimationController _animator;
        
        public Stats Stats => info.Stats;

        void Awake()
        {
            _animator = GetComponent<AnimationController>();
        }

        public void TakeDamage(float damage)
        {
            VariableStat hp = info.Stats[VariableStat.StatType.Hp];
            
            // Already dead
            if (hp.CurrentValue <= 0)
                return;
            
            hp.Reduce(damage);

            //hit animation

            if(hp.CurrentValue <= 0)
            {
                //Destroy(player);
                
                gameOverChannel.Invoke();
                
                _animator.OnDeath();
            }
            else
                _animator.OnHit();
        }

        void OnEnable()
        {
            inputReader.OnAttackAction += PlayerAttack_OnAttackAction;
            inputReader.OnMovementStartedAction += PlayerAttack_OnMovementAction;
        }

        void OnDisable()
        {
            inputReader.OnAttackAction -= PlayerAttack_OnAttackAction;
            inputReader.OnMovementStartedAction -= PlayerAttack_OnMovementAction;
        }

        void PlayerAttack_OnAttackAction()
        {
            // Not enough stamina for attack
            if (info.Stats[VariableStat.StatType.Stamina].CurrentValue < 10)
                return;;
            
            _animator.Attack();
            
            // Reduce stamina TODO: Amount
            info.Stats[VariableStat.StatType.Stamina].Reduce(10);

            CombatManager.MeleeAttack(transform, _dir, attackRange, enemyLayer, Stats);
        }

        void GetAttackBox(out Vector2 topLeft, out Vector2 botRight)
        {
            Vector3 position = transform.position;
            topLeft = (Vector2) position + _dir * attackRange + new Vector2(attackRange, attackRange);
            botRight = (Vector2) position + _dir * attackRange - new Vector2(attackRange, attackRange);
        }

        void PlayerAttack_OnMovementAction(Vector2 dir)
        { 
            _dir = dir;
        }

#if UNITY_EDITOR

        public void OnDrawGizmos()
        {   
            GetAttackBox(out Vector2 a, out Vector2 b);
            Vector2 c = new Vector2(b.x, a.y);
            Vector2 d = new Vector2(a.x, b.y);
            
            Gizmos.color = Color.black;
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(c, b);
            Gizmos.DrawLine(b, d );
            Gizmos.DrawLine(d, a);
        }
        
#endif
    }
}
