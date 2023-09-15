using System;
using System.Collections;
using System.Linq;
using AI.BehaviourTree.Module;
using AI.PathFinding;
using Combat;
using Player;
using Scriptable_Objects;
using Scriptable_Objects.Items;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    //[RequireComponent(typeof(AIBrain))]
    public class Enemy : MonoBehaviour, ICombatUnit
    {
        [SerializeField] PlayerInfo playerInfo;
        [SerializeField] Inventory inventory;
        [SerializeField] int soulsDropped = 1;
        [SerializeField] InventoryItem[] drops;
        [SerializeField] Stats stats;
        [SerializeField] float attackRange = 1;
        [SerializeField] LayerMask playerMask;
        [SerializeField] float visionRadius = 1;
        [SerializeField, Tooltip("Time to disappear after dying")] float timeToDisappear = 1;
        
        float _targetDetectionRadiusSqr;
        float _visionRadiusSqr;
    
        AIBrain _aiBrain;

        AnimationController _animationController;
        SpriteRenderer[] _renderers;

        IEnumerator _checkTargetRoutine;
        
        public Stats Stats => stats;

        public float AttackRange => attackRange;
        
        public event UnityAction<Enemy> OnKilled = delegate(Enemy enemy) {  };

        void Awake()
        {
            _animationController = GetComponent<AnimationController>();
            if (!_animationController)
                _animationController = GetComponentInChildren<AnimationController>();
            
            _aiBrain = GetComponent<AIBrain>();
            _renderers = GetComponents<SpriteRenderer>();
        }

        void Start()
        {
            _targetDetectionRadiusSqr = playerInfo.DetectionRadius * playerInfo.DetectionRadius;
            _visionRadiusSqr = visionRadius * visionRadius;

            bool isVisible = _renderers.Any(renderer => renderer.isVisible);

            if (!isVisible)
            {
                _aiBrain.enabled = false;
                _animationController.Disable();
                return;
            }
            
            StartCheckingTargets();
        }

        void StartCheckingTargets()
        {
            _checkTargetRoutine = CheckTargetInRange();
            StartCoroutine(_checkTargetRoutine);
        }

        IEnumerator CheckTargetInRange()
        {
            while (true)
            {
                if (_aiBrain.enabled)
                {
                    Vector3 targetPos = playerInfo.Player.position;
                    
                    _aiBrain.tree.Context.Blackboard.SetValue("Target", targetPos);
                    _aiBrain.tree.Context.Blackboard.SetValue("IsTargetInRange", IsInDistance(targetPos, transform.position));
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            // ReSharper disable once IteratorNeverReturns
        }
        
        bool IsInDistance(Vector3 targetPos, Vector3 entityPos)
        {
            return (targetPos - entityPos).sqrMagnitude <= _targetDetectionRadiusSqr + _visionRadiusSqr;
        }

        public void TakeDamage(float damage)
        {
            VariableStat hp = stats[VariableStat.StatType.Hp];
            
            // Already dead
            if (hp.CurrentValue <= 0)
                return;
            
            hp.Reduce(damage);
            
            if (hp > 0)
            {
                _animationController.OnHit();
                return;
            }

            Die();
        }

        protected virtual void Die()
        {
            playerInfo.Souls += soulsDropped;

            // TODO: How to add drops to inventory, what happens when inventory is full
            foreach (InventoryItem drop in drops)
            {
                inventory.AddItem(drop, 1);
            }

            _aiBrain.enabled = false;
            OnKilled.Invoke(this);
            
            _animationController.OnDeath();

            Destroy(gameObject, timeToDisappear);
        }

        public void Attack(Vector2 dir)
        {
            CombatManager.MeleeAttack(transform, dir, attackRange, playerMask, stats);
            
            _animationController.Attack();
        }

        void OnBecameVisible()
        {
            _aiBrain.enabled = true;
            
            StartCheckingTargets();

            _animationController.Enable();
            
            StopCoroutine(nameof(OnBecameInvisible));
        }

        IEnumerator OnBecameInvisible()
        {
            _animationController.Disable();
            StopCoroutine(_checkTargetRoutine);
            
            yield return new WaitForSeconds(5);
            
            _aiBrain.enabled = false;
        }
    }
}
