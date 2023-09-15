using System;
using AI.BehaviourTree.Module;
using Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace AI.BehaviourTree.Editor
{
    public class BehaviourTreeEditor : EditorWindow
    {
        TreeView _treeView;
        InspectorView _inspectorView;
        
        [MenuItem("Window/Behaviour Tree Editor %&b")]
        public static void OpenEditor()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
        }
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
        
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/AI/BehaviourTree/Editor/BehaviourTreeEditor.uxml");
            visualTree.CloneTree(root);
        
            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/AI/BehaviourTree/Editor/BehaviourTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            _treeView = root.Q<TreeView>();
            _inspectorView = root.Q<InspectorView>();

            _treeView.OnNodeSelected = _inspectorView.Update;
            _treeView.OnNodeDeselected = n => _inspectorView.RemoveEditor();
            
            OnSelectionChange();
        }

        void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
            EditorApplication.playModeStateChanged += OnplayModeStateChanged;
        }

        void OnplayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
        }

        void OnSelectionChange()
        {
            Module.BehaviourTree tree = Selection.activeObject as Module.BehaviourTree;
            GameObject selected = Selection.activeGameObject;

            if (!tree && selected)
            {
                Module.BehaviourTree objectTree = selected.GetComponent<AIBrain>()?.tree;

                if (objectTree)
                    tree = objectTree;
            }

            if (tree == null) 
                return;
            
            if ((Application.isPlaying || AssetDatabase.CanOpenForEdit(tree)) && _treeView != null)
                _treeView.Populate(tree);
        }

        void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                _treeView.UpdateNodeStateVisual();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int id, int line)
        {
            if (!(Selection.activeObject is Module.BehaviourTree)) 
                return false;
            
            OpenEditor();
            return true;
        }
    }
}