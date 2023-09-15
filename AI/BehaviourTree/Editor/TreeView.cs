using System;
using System.Collections.Generic;
using System.Linq;
using AI.BehaviourTree.Module;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Node = AI.BehaviourTree.Module.Node;
using Object = UnityEngine.Object;

namespace AI.BehaviourTree.Editor
{
    public class TreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits>{}

        Module.BehaviourTree _tree;

        public UnityAction<NodeView> OnNodeSelected;
        public UnityAction<NodeView> OnNodeDeselected;
        Vector2 _mousePos;

        public TreeView()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            Insert(0, new GridBackground());
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/AI/BehaviourTree/Editor/BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += () =>
            {
                Populate(_tree); // Possible null ref
                AssetDatabase.SaveAssets();
            };  
            
            RegisterCallback<MouseDownEvent>(
                evt => _mousePos = (evt.localMousePosition - new Vector2(viewTransform.position.x, viewTransform.position.y)) / scale);
        }

        public void Populate(Module.BehaviourTree tree)
        {
            graphViewChanged -= OnGraphViewChanged; 
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged; 
            
            _tree = tree;

            if (tree.root == null)
            {
                tree.root = (EntryNode) tree.CreateNode(typeof(EntryNode));
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }
            
            foreach (Node node in tree.nodes)
            {
                CreateNodeView(node);
            }
            
            // Edges
            foreach (Edge edge in from node in tree.nodes 
                from child in _tree.GetChildren(node) 
                let childView = GetNodeView(child) 
                let parentView = GetNodeView(node) 
                select parentView.Output.ConnectTo(childView.Input))
            {
                AddElement(edge);
            }
        }

        NodeView GetNodeView(Node node) => GetNodeByGuid(node.guid) as NodeView;

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (GraphElement element in change.elementsToRemove)
                {
                    switch (element)
                    {
                        case NodeView view:
                            _tree.DeleteNode(view.Node);
                            break;
                        case Edge edge:
                            GetInOutFromEdge(edge, out NodeView parentView, out NodeView childView);
                            _tree.RemoveChild(parentView.Node, childView.Node);
                            break;
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    GetInOutFromEdge(edge, out NodeView parentView, out NodeView childView);
                    
                    _tree.AddChild(parentView.Node, childView.Node);
                }
            }

            if (change.movedElements != null)
            {
                foreach (UnityEditor.Experimental.GraphView.Node node in (dynamic)nodes)
                {
                    (node as NodeView)?.SortNodes();
                }
            }
            
            return change;

            void GetInOutFromEdge(Edge edge, out NodeView parent, out NodeView child)
            {
                parent = (NodeView) edge.output.node;
                child = (NodeView) edge.input.node;
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Gets all node classes that aren't abstract or interfaces that derive from node
            IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom<Node>()
                .Where(type => !type.IsAbstract && !type.IsInterface && type != typeof(EntryNode))
                .OrderBy(t => t.BaseType?.Name);

            // Create the actions for the contextual menu
            foreach (Type type in types)
            {
                string baseName = ObjectNames.NicifyVariableName(type.BaseType?.Name);
                string nodeName = ObjectNames.NicifyVariableName(type.Name);
                
                evt.menu.AppendAction(
                    $"[{baseName}] {nodeName}", 
                    a => CreateNode(type, _mousePos));
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList(); 
        }

        void CreateNode(Type type)
        {
            CreateNode(type, Vector2.zero);
        }

        void CreateNode(Type type, Vector2 position)
        {
            Node node = _tree.CreateNode(type);

            node.position = position;
            
            CreateNodeView(node);
        }

        void CreateNodeView(Node node)
        {
            NodeView view = new NodeView(node)
            {
                OnNodeSelected = OnNodeSelected,
                OnNodeDeselected = OnNodeDeselected
            };
            AddElement(view);
        }

        public void UpdateNodeStateVisual()
        {
            foreach (UnityEditor.Experimental.GraphView.Node node in (dynamic) nodes)
            {
                ((NodeView)node).UpdateState();
            }
        }
    }
}
