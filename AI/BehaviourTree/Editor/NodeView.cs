using System;
using System.Linq;
using AI.BehaviourTree.Module;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace AI.BehaviourTree.Editor
{
    public sealed class NodeView : Node
    {
        public Module.Node Node;
        public Port Input;
        public Port Output;
        
        public UnityAction<NodeView> OnNodeSelected;
        public UnityAction<NodeView> OnNodeDeselected; 

        public NodeView(Module.Node node) : base("Assets/Scripts/AI/BehaviourTree/Editor/NodeVisual.uxml")
        {
            Node = node;

            title = node.name;
            viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            const Orientation orientation = Orientation.Vertical; 

            // Input
            // Only entry nodes do not have inputs
            if (Node.GetType() != typeof(EntryNode))
            {
                Input = InstantiatePort(orientation, Direction.Input, Port.Capacity.Single, typeof(bool));
                
                Input.portName = "";
                Input.portColor = Color.blue;
                Input.style.flexDirection = FlexDirection.Column;
                
                inputContainer.Add(Input);
            }
            
            // Outputs
            if (node.TryCreateOutputPorts(this, orientation, out Output))
            {
                Output.portName = "";
                Output.portColor = Color.red;
                Output.style.flexDirection = FlexDirection.ColumnReverse;
                
                outputContainer.Add(Output);
            }

            switch (node)
            {
                case ActionNode _:
                    AddToClassList("action");
                    break;
                case DecoratorNode _:
                    AddToClassList("decorator");
                    break;
                case CompositeNode _:
                    AddToClassList("composite");
                    break;
                case EntryNode _:
                    AddToClassList("entry");
                    break;
                default:
                    Debug.LogError("Node not recognized");
                    break;
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(Node, "Set node position");
            Node.position = newPos.min;
            //EditorUtility.SetDirty(Node); Uncomment if position gets lost on assembly reload
        }

        public override void OnSelected()
        {
            base.OnSelected();
            
            OnNodeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            
            OnNodeDeselected?.Invoke(this);
        }

        public void SortNodes()
        {
            if (Node is CompositeNode compositeNode)
            {
                compositeNode.children = compositeNode.children.OrderBy(n => n.position.x).ToList();
            }
        }

        public void UpdateState()
        {
            const string success = "success";
            const string running = "running";
            const string failure = "failure";
            
            RemoveFromClassList(success);
            RemoveFromClassList(running);
            RemoveFromClassList(failure);
            
            switch (Node.state)
            {
                case State.Success:
                    AddToClassList(success);
                    break;
                case State.Running:
                    if (Node.started)
                        AddToClassList(running);
                    break;
                case State.Failure:
                    AddToClassList(failure);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
