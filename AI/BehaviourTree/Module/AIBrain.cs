using System;
using Unity.Profiling;
using UnityEngine;

namespace AI.BehaviourTree.Module
{
    public class AIBrain : MonoBehaviour
    {
        // profiling
        static readonly ProfilerMarker ProfilerMarkerUpdate = new ProfilerMarker(ProfilerCategory.Ai, "Behaviour Tree Update");
        static readonly ProfilerMarker ProfilerMarkerFixedUpdate = new ProfilerMarker(ProfilerCategory.Ai, "Behaviour Tree Fixed Update");
    
        public BehaviourTree tree;
        
        void Awake()
        {
            tree = tree.Create(this);
        }

        void Update()
        {
            using ProfilerMarker.AutoScope profiler = ProfilerMarkerUpdate.Auto();
            
            tree.Update();
        }

        void FixedUpdate()
        {
            using ProfilerMarker.AutoScope profiler = ProfilerMarkerFixedUpdate.Auto();

            tree.FixedUpdate();
        }
    }
}
