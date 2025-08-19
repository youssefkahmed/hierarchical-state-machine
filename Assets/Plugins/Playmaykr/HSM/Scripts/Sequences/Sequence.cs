using System.Threading;
using System.Threading.Tasks;

namespace Playmaykr.HSM.Sequences
{
    /// <summary>
    /// Represents a sequence of actions that can be executed in a state machine transition.
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        /// The Sequence's completion state.
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        /// Starts the sequence.
        /// </summary>
        void Start();
        /// <summary>
        /// Updates the sequence.
        /// </summary>
        /// <returns>True if the sequence is complete, false otherwise.</returns>
        bool Update();
    }
    
    // One activity operation (activate OR deactivate) to run for this phase.
    public delegate Task PhaseStep(CancellationToken ct);

    /// <summary>
    /// An empty <see cref="ISequence"/> that completes immediately.
    /// </summary>
    public class NoopPhase : ISequence
    {
        public bool IsDone { get; private set; }
        public void Start() => IsDone = true; // Completes immediately
        public bool Update() => IsDone;
    }
}