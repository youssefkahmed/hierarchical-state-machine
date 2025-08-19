using System.Collections.Generic;
using Playmaykr.HSM.Activities;

namespace Playmaykr.HSM.Core
{
    /// <summary>
    /// Represents a node in the <see cref="StateMachine"/>
    /// </summary>
    public abstract class State
    {
        public readonly StateMachine Machine;
        public readonly State Parent;
        public State ActiveChild;
        public IReadOnlyList<IActivity> Activities => _activities;

        private readonly List<IActivity> _activities = new();
        
        public State(StateMachine machine, State parent = null)
        {
            Machine = machine;
            Parent = parent;
        }

        /// <summary>
        /// Adds an activity to be activated when entering this state
        /// </summary>
        /// <param name="a">The activity to add.</param>
        public void AddActivity(IActivity a)
        {
            if (a != null)
            {
                _activities.Add(a);
            }
        }
        
        /// <summary>
        /// Gets the initial child to enter when this state starts (null = this is the leaf)
        /// </summary>
        protected virtual State GetInitialState()
        {
            return null;
        }
        
        /// <summary>
        /// Gets the target state to switch to this frame (null = stay in current state)
        /// </summary>
        protected virtual State GetTransition()
        {
            return null;
        }
        
        // Lifecycle hooks
        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnUpdate(float deltaTime) { }

        internal void Enter()
        {
            if (Parent != null)
            {
                Parent.ActiveChild = this;
            }
            OnEnter();
            State init = GetInitialState();
            init?.Enter();
        }
        
        internal void Exit()
        {
            ActiveChild?.Exit();
            ActiveChild = null;
            OnExit();
        }
        
        internal void Update(float deltaTime)
        {
            State t = GetTransition();
            if (t != null)
            {
                Machine.Sequencer.RequestTransition(this, t);
                return;
            }

            ActiveChild?.Update(deltaTime);
            OnUpdate(deltaTime);
        }
        
        /// <summary>
        /// Returns the deepest currently active descendant state (the leaf of the active path).
        /// </summary>
        public State Leaf()
        {
            State s = this;
            while (s.ActiveChild != null)
            {
                s = s.ActiveChild;
            }
            return s;
        }
        
        /// <summary>
        /// Yields this state and then each ancestor up to the root (self → parent → ... → root).
        /// </summary>
        public IEnumerable<State> PathToRoot()
        {
            for (State s = this; s != null; s = s.Parent)
            {
                yield return s;
            }
        }
    }
}