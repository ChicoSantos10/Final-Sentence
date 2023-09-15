using AI.BehaviourTree.Module;
using Enemies;
using Player;
using Scriptable_Objects;
using UnityEngine;
using Node = AI.PathFinding.Node;

namespace AI.BehaviourTree.Extras
{
    public class MoveToTargetNode : ActionNode
    {
        [SerializeField] PathFinderDatabase database;
        [SerializeField] float speed;

        [SerializeField, Tooltip("How close it needs to be")] float minDist = 0;
        [SerializeField, Tooltip("If set unit will always move until its close enough to attack")] bool useAttackRange = false;
        //[SerializeField, Tooltip("If set unit will always will always try to keep its distance")] bool keepDist = false;
        
        Vector3[] _path;
        Vector3 _target;
        Vector3 _targetPreviousPos;
        int _index;
        Rigidbody2D _rb;
        float _dist;
        Vector3 _nextPos;
        
        Transform _rig;
        AnimationController _controller;
        
        protected override State OnUpdate(Context context)
        {
            if (!context.Blackboard.TryGetValue("Target", out _target))
                return State.Failure;
            
            Vector3 entityPosition = context.Entity.transform.position;
            if (Vector2.Distance(_target, entityPosition) <= _dist)
                return State.Success;
            
            if(_target != _targetPreviousPos || _path == null)
            {
                GetPath(context, _target);
                _targetPreviousPos = _target;
            }
                
            if (_path == null)
                return State.Failure;

            _nextPos = Vector2.MoveTowards(entityPosition, _path[_index],
                speed * Time.deltaTime);

            Vector2 dir = _nextPos - entityPosition;
            
            if (dir.x > 0 && _rig.localScale.x > 0)
                AnimationController.Flip(_rig);
            else if (dir.x < 0 && _rig.localScale.x < 0)
                AnimationController.Flip(_rig);
            
            return State.Running;
        }

        protected override void OnPhysicsUpdate(Context context)
        {
            _rb.MovePosition(_nextPos);
            if (_rb.position == (Vector2)_path[_index] && _index < _path.Length - 1)
                _index++;
            
            _controller.SetSpeed(1);
        }

        public override void OnAwake(Context context)
        {
            _rb = context.Entity.GetComponent<Rigidbody2D>();
            _controller = context.Entity.GetComponentInChildren<AnimationController>();
            _rig = context.Entity.transform;

            _dist = useAttackRange ? context.Entity.GetComponent<Enemy>().AttackRange : minDist;
        }

        protected override void Initialize(Context context)
        {
            if (context.Blackboard.TryGetValue("Target", out _target))
                GetPath(context, _target);

            _targetPreviousPos = _target;
            _nextPos = context.Entity.transform.position;
        }

        void GetPath(Context context, Vector3 target)
        {
            database.TryFindPath(context.Entity.transform.position, target, out _path);
            _index = 0;
        }
    }
}
