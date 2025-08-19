using System;
using System.Collections.Generic;
using System.Threading;
using Playmaykr.HSM.Activities;
using Playmaykr.HSM.Sequences;
using UnityEngine;

namespace Playmaykr.HSM.Core
{
    public class TransitionSequencer
    {
        public readonly StateMachine Machine;

        private ISequence _sequence;                 // Current phase (deactivate or activate)
        private Action _nextPhase;                    // Switch structure between phases
        private (State from, State to)? _pending;     // Coalesce a single pending request
        private State _lastFrom;
        private State _lastTo;

        private CancellationTokenSource _cts;
        private bool _useSequential;

        public TransitionSequencer(StateMachine machine)
        {
            Machine = machine;
        }

        /// <summary>
        /// Requests a transition from one state to another.
        /// </summary>
        /// <param name="from">State to transition from</param>
        /// <param name="to">State to transition to</param>
        public void RequestTransition(State from, State to)
        {
            if (to == null || from == to)
            {
                return;
            }

            // There's a transition already in progress
            if (_sequence != null)
            {
                _pending = (from, to);
                return;
            }
            
            BeginTransition(from, to);
        }

        /// <summary>
        /// Updates the sequencer's state machine using deltaTime.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_sequence != null)
            {
                // While transitioning, we don't run normal updates
                if (!_sequence.Update())
                {
                    return;
                }

                if (_nextPhase != null)
                {
                    Action n = _nextPhase;
                    _nextPhase = null;
                    n();
                }
                else
                {
                    EndTransition();
                }
            }
            Machine.InternalTick(deltaTime);
        }

        /// <summary>
        /// Computes the Lowest Common Ancestor of two states.
        /// </summary>
        /// <param name="a">The first state</param>
        /// <param name="b">The second state</param>
        public static State Lca(State a, State b)
        {
            // Create a set of all parents of 'a'
            var ap = new HashSet<State>();
            for (State s = a; s != null; s = s.Parent)
            {
                ap.Add(s);
            }

            // Find the first parent of 'b' that is also a parent of 'a'
            for (State s = b; s != null; s = s.Parent)
            {
                if (ap.Contains(s))
                {
                    return s;
                }
            }

            // If no common ancestor found, return null
            return null;
        }

        /// <summary>
        /// Executes a sequence of transition activities from the 'from' state to the 'to' state.
        /// </summary>
        /// <param name="from">State to transition from</param>
        /// <param name="to">State to transition to</param>
        private void BeginTransition(State from, State to)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            State lca        = Lca(from, to);
            List<State> exitChain  = StatesToExit(from, lca);
            List<State> enterChain = StatesToEnter(to,  lca);
            
            // 1. Deactivate the “old branch”
            List<PhaseStep> exitSteps  = GatherPhaseSteps(exitChain, deactivate: true);
            _sequence = _useSequential
                ? new SequentialPhase(exitSteps, _cts.Token)
                : new ParallelPhase(exitSteps, _cts.Token);
            _sequence.Start();
            
            _nextPhase = () => 
            {
                // 2. ChangeState
                Machine.ChangeState(from, to);
                
                // 3. Activate the “new branch”
                List<PhaseStep> enterSteps = GatherPhaseSteps(enterChain, deactivate: false);
                _sequence = _useSequential
                    ? new SequentialPhase(enterSteps, _cts.Token)
                    : new ParallelPhase(enterSteps, _cts.Token);
                _sequence.Start();
            };
        }

        /// <summary>
        /// Ends the current transition sequence and activates any pending transitions.
        /// </summary>
        private void EndTransition()
        {
            _sequence = null;

            if (_pending.HasValue)
            {
                (State from, State to) p = _pending.Value;
                _pending = null;
                BeginTransition(p.from, p.to);
            }
        }
        
        /// <summary>
        /// Returns a list of steps (actions) to perform when transitioning along a list of states (chain).
        /// </summary>
        /// <param name="chain">The chain of states to gather activities from.</param>
        /// <param name="deactivate">Whether to deactivate the list of gathered activities.</param>
        /// <returns></returns>
        private static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate)
        {
            var steps = new List<PhaseStep>();

            for (var i = 0; i < chain.Count; i++)
            {
                State st = chain[i];
                IReadOnlyList<IActivity> acts = st.Activities;
                for (var j = 0; j < acts.Count; j++)
                {
                    IActivity a = acts[j];
                    bool include = deactivate ? a.Mode == ActivityMode.Active : a.Mode == ActivityMode.Inactive;
                    if (!include)
                    {
                        continue;
                    }

                    Debug.Log($"[Phase {(deactivate?"Exit":"Enter")}] state={st.GetType().Name}, activity={a.GetType().Name}, mode={a.Mode}");

                    steps.Add(ct => deactivate ? a.DeactivateAsync(ct) : a.ActivateAsync(ct));
                }
            }
            return steps;
        }
        
        /// <summary>
        /// States to exit: from → ... up to (but excluding) the lowest common ancestor; bottom→up order.
        /// </summary>
        /// <param name="from">State to start exiting from.</param>
        /// <param name="lca">The lowest common ancestor between the 'from' state and the state being transitioned into.</param>
        private static List<State> StatesToExit(State from, State lca)
        {
            var list = new List<State>();
            for (State s = from; s != null && s != lca; s = s.Parent)
            {
                list.Add(s);
            }
            return list;
        }
        
        /// <summary>
        /// States to enter: path from 'to' up to (but excluding) the lowest common ancestor; returned in enter order (top→down).
        /// </summary>
        /// <param name="to">State to start entering.</param>
        /// <param name="lca">The lowest common ancestor between the 'to' state and the state being transitioned from.</param>
        private static List<State> StatesToEnter(State to, State lca)
        {
            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent)
            {
                stack.Push(s);
            }
            return new List<State>(stack);
        }
    }
}