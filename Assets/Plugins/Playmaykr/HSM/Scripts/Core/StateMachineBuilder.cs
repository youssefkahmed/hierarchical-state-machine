using System.Collections.Generic;
using System.Reflection;

namespace Playmaykr.HSM.Core
{
    /// <summary>
    /// Automated the process of building and wiring a state machine's states. 
    /// </summary>
    public class StateMachineBuilder
    {
        private readonly State _root;
        
        public StateMachineBuilder(State root)
        {
            _root = root;
        }

        /// <summary>
        /// Builds the state machine from the root state, wiring all states together.
        /// </summary>
        /// <returns>The built state machine.</returns>
        public StateMachine Build()
        {
            var m = new StateMachine(_root);
            Wire(_root, m, new HashSet<State>());
            return m;
        }

        private static void Wire(State s, StateMachine m, HashSet<State> visited)
        {
            if (s == null)
            {
                return;
            }
            
            // State is already wired
            if (!visited.Add(s))
            {
                return;
            }
            
            // Using reflection to set the Machine field of each state to the parameter 'm', which is the state machine
            // being built.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            FieldInfo machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null)
            {
                machineField.SetValue(s, m);
            }

            foreach (FieldInfo fld in s.GetType().GetFields(flags))
            {
                // Only consider fields that are State
                if (!typeof(State).IsAssignableFrom(fld.FieldType))
                {
                    continue;
                }
                
                // Skip back-edge to parent
                if (fld.Name == "Parent")
                {
                    continue;
                }
                
                var childState = (State)fld.GetValue(s);
                if (childState == null)
                {
                    continue;
                }
                
                // Ensure it's actually our direct child
                if (!ReferenceEquals(childState.Parent, s))
                {
                    continue;
                }
                
                Wire(childState, m, visited); // Recurse into the child
            }
        }
    }
}