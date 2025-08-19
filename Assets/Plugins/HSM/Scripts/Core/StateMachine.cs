using System.Collections.Generic;

namespace Playmaykr.HSM.Core
{
    /// <summary>
    /// Represents the core of a hierarchical state machine. Manages a bunch of nodes (states) and handles transitions
    /// between them.
    /// </summary>
    public class StateMachine
    {
        public readonly State Root;
        public readonly TransitionSequencer Sequencer;

        private bool _hasStarted;

        public StateMachine(State root)
        {
            Root = root;
            Sequencer = new TransitionSequencer(this);
        }

        public void Start()
        {
            if (_hasStarted)
            {
                return;
            }
            
            _hasStarted = true;
            Root.Enter();
        }

        public void Tick(float deltaTime)
        {
            if (!_hasStarted)
            {
                Start();
            }
            Sequencer.Tick(deltaTime);
        }
        
        internal void InternalTick(float deltaTime)
        {
            Root.Update(deltaTime);
        }
        
        /// <summary>
        /// Performs the actual switch from 'from' to 'to' by exiting up to the shared ancestor, then entering down to the target.
        /// </summary>
        public void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null)
            {
                return;
            }
            
            State lca = TransitionSequencer.Lca(from, to);
            
            // Exit current branch up to (but not including) LCA
            for (State s = from; s != lca; s = s.Parent)
            {
                s.Exit();
            }
            
            // Enter target branch from LCA down to target
            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent)
            {
                stack.Push(s);
            }
            
            while (stack.Count > 0)
            {
                stack.Pop().Enter();
            }
        }
    }
}