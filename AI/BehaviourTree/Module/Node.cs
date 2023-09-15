#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

namespace AI.BehaviourTree.Module
{
    public abstract class Node : ScriptableObject
    {
        [HideInInspector] public bool started = false;
        [HideInInspector] public State state = State.Running;
        [HideInInspector] public Node parent;
        [HideInInspector] public BehaviourTree tree;

        public State Execute(Context context)
        {
            if (!started)
            {
                Initialize(context);
                started = true;
            }

            state = OnUpdate(context);
            
            if (!HasFinished()) 
                return state;
            
            Terminate(context);

            return state;
        }

        public void PhysicsUpdate(Context context)
        {
            if (!started)
                return;
                
            OnPhysicsUpdate(context);
        }
        
        public void Terminate(Context context)
        {
            started = false;
            OnTerminate(context);
        }
        
        public virtual void OnAwake(Context context){}
        protected abstract State OnUpdate(Context context);
        protected virtual void Initialize(Context context){}
        protected virtual void OnTerminate(Context context){}
        protected virtual void OnPhysicsUpdate(Context context){}

        public virtual Node Clone() => Instantiate(this);

        bool HasFinished() => state == State.Failure || state == State.Success;
        
#if UNITY_EDITOR

        [HideInInspector] public Vector2 position;
        [HideInInspector] public string guid;

        /// <summary>
        /// Creates output ports to use in the editor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="orientation"></param>
        /// <param name="port">The port created</param>
        /// <returns>True if the node should have an output port</returns>
        public abstract bool TryCreateOutputPorts(UnityEditor.Experimental.GraphView.Node node, Orientation orientation,
            out Port port);
#endif
    }

    public enum State
    {
        Success,
        Running,
        Failure
    }
}
