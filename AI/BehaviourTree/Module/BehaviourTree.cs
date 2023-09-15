using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AI.BehaviourTree.Module
{
    [CreateAssetMenu]
    public class BehaviourTree : ScriptableObject
    {
        public Node root;
        State _state = State.Running;
        Context _context;

        // Stores all nodes for viewing in the editor
        public List<Node> nodes = new List<Node>();

        public Context Context => _context;

        public void Update()
        {
            if (_state == State.Running)
                _state = root.Execute(_context);
        }

        public void FixedUpdate()
        {
            if (_state == State.Running)
                Traverse(root, n => n.PhysicsUpdate(_context));
        }

        public BehaviourTree Create(AIBrain entity)
        {
            BehaviourTree tree = Instantiate(this);
            
            tree._context = new Context(new Blackboard(), entity);

            tree.root = root.Clone();

            tree.nodes = new List<Node>();

            Traverse(tree.root, node =>
            {
                node.OnAwake(tree._context);
                tree.nodes.Add(node);
            });

            return tree;
        }

        void Traverse(Node node, Action<Node> a)
        {
            a(node);
            IEnumerable<Node> children = GetChildren(node);
            foreach (Node child in children)
            {
                Traverse(child, a);
            }
        }

        public IEnumerable<Node> GetChildren(Node parent)
        {
            switch (parent)
            {
                case DecoratorNode dec:
                    return dec.child ? new List<Node> {dec.child} : new List<Node>();
                case CompositeNode comp:
                    return comp.children;
                case EntryNode entry:
                    return entry.child ? new List<Node> {entry.child} : new List<Node>();
                case ActionNode action:
                    return new List<Node>();
                default:
                    Debug.LogError("Trying to get the children of a parent node whose type is not defined");
                    return new List<Node>();
            }
        }

        public void Abort(Node startNode, AbortMode mode = AbortMode.All)
        {
            List<Node> parentChildren = GetChildren(startNode.parent).ToList();
            int childIndex = 0;
            foreach (Node child in parentChildren)
            {
                if (child == startNode) 
                    break;
                childIndex++;
            }

            if (mode.HasFlag(AbortMode.Right))
            {
                //abortNodes.AddRange(GetChildren(startNode.parent).Where(c => c != startNode && c.GetType() == typeof(CompositeNode)).Cast<CompositeNode>());

                startNode.parent.Terminate(_context);
                
                for (int i = childIndex + 1; i < parentChildren.Count; i++)
                {
                    Traverse(parentChildren[childIndex + 1], n => n.Terminate(_context));
                }
            }

            if (mode.HasFlag(AbortMode.Self))
            {
                //abortNodes.AddRange(GetChildren(startNode).Where(c => c.GetType() == typeof(CompositeNode)).Cast<CompositeNode>());
                Traverse(startNode, n => n.Terminate(_context));
            }
        }

        public void Abort(AbortMode mode = AbortMode.All)
        {
            Abort(root, mode);
        }

#if UNITY_EDITOR
        
        public Node CreateNode(Type type)
        {
            Node node = (Node) CreateInstance(type);
            node.name = ObjectNames.NicifyVariableName(type.Name);
            node.guid = GUID.Generate().ToString();
            node.tree = this;
            
            Undo.RecordObject(this, $"Create node in {name}");
            nodes.Add(node);
            
            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(node, this);
            
            Undo.RegisterCreatedObjectUndo(node, $"Created node in {name}");
            AssetDatabase.SaveAssets();

            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, $"Deleted node in {name}");
            nodes.Remove(node);
            
            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);
            
            AssetDatabase.SaveAssets();
        }
        
        public void RemoveChild(Node parent, Node child)
        {
            switch (parent)
            {
                case DecoratorNode dec:
                    Undo.RecordObject(dec, $"Removed child({dec}) from {parent}");
                    dec.child = null;
                    break;
                case CompositeNode comp:
                    Undo.RecordObject(comp, $"Removed child({comp}) from {parent}");
                    comp.children.Remove(child);
                    break;
                case EntryNode entry:
                    Undo.RecordObject(entry, $"Removed child({entry}) from {parent}");
                    entry.child = null;
                    break;
                default:
                    Debug.LogError("Trying to remove a child to a parent node whose type is not defined");
                    break;
            }

            child.parent = null;
        }
        
        // TODO: Do this in the actual node. This way removes switch statements and logic is better tied up to each node type. Problem with action nodes though
        public void AddChild(Node parent, Node child)
        {
            switch (parent)
            {
                case DecoratorNode dec:
                    Undo.RecordObject(dec, $"Added child: {dec} to {parent}");
                    dec.child = child;
                    break;
                case CompositeNode comp:
                    Undo.RecordObject(comp, $"Added child: {comp} to {parent}");
                    comp.children.Add(child);
                    break;
                case EntryNode entry:
                    Undo.RecordObject(entry, $"Added child: {entry} to {parent}");
                    entry.child = child;
                    break;
                default:
                    Debug.LogError("Trying to add a child to a parent node whose type is not defined");
                    break;
            }

            child.parent = parent;
        }
#endif
    }

    public class Context
    {
        public readonly Blackboard Blackboard;
        public readonly AIBrain Entity;

        public Context(Blackboard blackboard, AIBrain entity)
        {
            Blackboard = blackboard;
            Entity = entity;
        }
    }

    [Flags]
    public enum AbortMode
    {
        None = 0,
        Right = 1,
        Self = 1 << 1,
        All = ~0
    }

    public enum EventType
    {
        Set,
        ValueChanged,
    }
}
