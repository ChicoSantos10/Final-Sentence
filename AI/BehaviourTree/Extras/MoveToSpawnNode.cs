using System;
using AI.BehaviourTree.Module;
using Player;
using Scriptable_Objects;
using UnityEngine;
using Node = AI.PathFinding.Node;

namespace AI.BehaviourTree.Extras
{
    public class MoveToSpawnNode : ActionNode
    {
        [SerializeField] float minDist = 1;
        [SerializeField] PathFinderDatabase database;

        [SerializeField] float speed;

        Vector3 _spawnPos;
        Vector3[] _path;
        Rigidbody2D _rb;
        int _index;
        Vector2 _nextPosition;

        Transform _rig;
        AnimationController _controller;
        
        protected override State OnUpdate(Context context)
        {
            Vector3 entityPos = context.Entity.transform.position;
            if (Vector2.Distance(entityPos, _spawnPos) <= minDist)
                return State.Success;

            if (_path == null)
                return State.Failure;
            
            _nextPosition = Vector2.MoveTowards(entityPos, _path[_index],
                speed * Time.deltaTime);
            
            return State.Running;
        }

        public override void OnAwake(Context context)
        {
            _spawnPos = context.Entity.transform.position;
            _rb = context.Entity.GetComponent<Rigidbody2D>();
            _controller = context.Entity.GetComponentInChildren<AnimationController>();
            _rig = context.Entity.transform;
        }

        protected override void OnPhysicsUpdate(Context context)
        {
            _rb.MovePosition(_nextPosition);
            
            if (_nextPosition == (Vector2) _path[_index])
                _index++;
            
            Vector2 dir =  _nextPosition - (Vector2) context.Entity.transform.position;
            
            if (dir.x > 0 && _rig.localScale.x > 0)
                AnimationController.Flip(_rig);
            else if (dir.x < 0 && _rig.localScale.x < 0)
                AnimationController.Flip(_rig);
            
            _controller.SetSpeed(1);
        }

        protected override void Initialize(Context context)
        {
            database.TryFindPath(context.Entity.transform.position, _spawnPos, out _path);
            _index = 0;
        }
    }
}
