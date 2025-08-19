using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Playmaykr.HSM.Sequences
{
    /// <summary>
    /// A type of <see cref="ISequence"/> that runs its tasks in parallel.
    /// </summary>
    public class ParallelPhase : ISequence
    {
        public bool IsDone { get; private set; }

        private readonly List<PhaseStep> _steps;
        private readonly CancellationToken _ct;
        private List<Task> _tasks;
        
        public ParallelPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            _steps = steps;
            _ct = ct;
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (_steps == null || _steps.Count == 0)
            {
                IsDone = true;
                return;
            }
            
            _tasks = new List<Task>(_steps.Count);
            for (var i = 0; i < _steps.Count; i++)
            {
                _tasks.Add(_steps[i](_ct));
            }
        }

        /// <inheritdoc/>
        public bool Update() 
        {
            if (IsDone)
            {
                return true;
            }
            
            IsDone = _tasks == null || _tasks.TrueForAll(t => t.IsCompleted);
            return IsDone;
        }
    }
}