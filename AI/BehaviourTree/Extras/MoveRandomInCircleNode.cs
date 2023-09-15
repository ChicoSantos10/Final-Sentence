using AI.BehaviourTree.Module;
using Scriptable_Objects;
using UnityEngine;
using Node = AI.PathFinding.Node;

namespace AI.BehaviourTree.Extras
{
    public class MoveRandomInCircleNode : ActionNode
    {
        [SerializeField] PathFinderDatabase database;
        [SerializeField] float radius;
        [SerializeField] float speed;

        Rigidbody2D _rb;
        Vector3[] _path;
        Vector3 _target;
        Vector3 _nextPos;
        int _index;
        Vector2 _nextPosition;
        Vector3 _spawnPos;
        
        protected override State OnUpdate(Context context)
        {
            Vector3 entityPos = context.Entity.transform.position;
            
            if (_path == null)
                return State.Failure;
            
            if (entityPos == _target)
                return State.Success;
            
            _nextPosition = Vector2.MoveTowards(entityPos, _path[_index],
                speed * Time.deltaTime);
            
            return State.Running;
        }

        protected override void OnPhysicsUpdate(Context context)
        {
            if (_path == null)
                return;
            
            _rb.MovePosition(_nextPosition);
            
            if (_rb.position == (Vector2)_path[_index] && _index < _path.Length - 1)
                _index++;
        }

        public override void OnAwake(Context context)
        {
            _rb = context.Entity.GetComponent<Rigidbody2D>();
            _spawnPos = context.Entity.transform.position;
        }

        protected override void Initialize(Context context)
        {
            _target = _spawnPos + (Vector3)Random.insideUnitCircle * radius;
            if (!database.TryFindPath(_spawnPos, _target, out _path)) 
                return;
            _index = 0;
            _nextPos = _path[_index]; 
        }
    }
}
