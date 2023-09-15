using AI.BehaviourTree.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView>{}

        UnityEditor.Editor _editor;
        
        public void Update(NodeView nodeView)
        {
            RemoveEditor();
            
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (_editor.target)
                {
                    _editor.OnInspectorGUI();
                }
            });
            
            Add(container);
        }

        public void RemoveEditor()
        {
            Clear();

            Object.DestroyImmediate(_editor);
        }
    }
}
