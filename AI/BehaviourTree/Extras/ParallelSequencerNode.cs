using System;
using System.Collections.Generic;
using System.Linq;
using AI.BehaviourTree.Module;
using UnityEngine;

namespace AI.BehaviourTree.Extras
{
    public class ParallelSequencerNode : ParallelNode
    {
        protected override State ImmediateModeExecute(Context context)
        {
            State nextState = State.Success;
            
            foreach (Node _ in children.Where(child => child.Execute(context) == State.Failure))
            {
                nextState = State.Failure;
            }

            return nextState;
        }

        protected override State DelayedModeExecute(Context context)
        {
            State nextState = State.Success;
            List<State> states = new List<State>();

            if (RunningChildren!.Count > 0)
            {
                for (int index = RunningChildren.Count - 1; index >= 0; index--)
                {
                    Node child = RunningChildren[index];
                    State childState = child.Execute(context);

                    states.Add(childState);
                    
                    switch (childState)
                    {
                        case State.Success:
                        case State.Failure:
                            RunningChildren.RemoveAt(index);
                            break;
                        case State.Running:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                foreach (Node child in children)
                {
                    State childState = child.Execute(context);

                    states.Add(childState);
                    
                    switch (childState)
                    {
                        case State.Success:
                            break;
                        case State.Failure:
                            break;
                        case State.Running:
                            RunningChildren.Add(child);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (State current in states)
            {
                switch (current)
                {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        break;
                    case State.Failure:
                        nextState = State.Failure;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return nextState;
        }
    }
}
