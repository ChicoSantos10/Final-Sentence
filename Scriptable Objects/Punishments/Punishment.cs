using Managers;
using Scriptable_Objects.Event_Channels;
using UnityEditor;
using UnityEngine;

namespace Scriptable_Objects.Punishments
{
    public abstract class Punishment : ScriptableObject
    {
        [SerializeField] EventChannel newDayEvent;

        public void Begin()
        {
            newDayEvent.Event += End;
            
            OnBegin();
        }

        protected abstract void OnBegin();

        void End()
        {
            newDayEvent.Event -= End;
            
            OnEnd();
        }
        
        protected abstract void OnEnd();

#if UNITY_EDITOR

        [MenuItem("Assets/Begin Punishment")]
        public static void BeginPunishment()
        {
            ((Punishment) Selection.activeObject).OnBegin();
        }
        
        [MenuItem("Assets/Begin Punishment", true)]
        public static bool BeginPunishmentValidate()
        {
            return Application.isPlaying && Selection.activeObject is Punishment;
        }
        
        [MenuItem("Assets/End Punishment")]
        public static void EndPunishment()
        {
            ((Punishment) Selection.activeObject).OnEnd();
        }
        
        [MenuItem("Assets/End Punishment", true)]
        public static bool EndPunishmentValidate()
        {
            return Application.isPlaying && Selection.activeObject is Punishment;
        }
        
#endif
    }
}
