using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Playmaykr.HSM.Sequences 
{
    /// <summary>
    /// A type of <see cref="ISequence"/> that runs its tasks sequentially.
    /// </summary>
    public class SequentialPhase : ISequence
    {
        private readonly List<PhaseStep> _steps;
        private readonly CancellationToken _ct;

        private int _taskIndex = -1;
        private Task _currentTask;
        
        public bool IsDone { get; private set; }

        public SequentialPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            _steps = steps;
            _ct = ct;
        }
        
        /// <inheritdoc/>
        public void Start()
        {
            Next();
        }

        /// <inheritdoc/>
        public bool Update()
        {
            if (IsDone)
            {
                return true;
            }
            
            if (_currentTask == null || _currentTask.IsCompleted)
            {
                Next();
            }
            return IsDone;
        }

        /// <summary>
        /// Starts the next task in the sequence, if any.
        /// </summary>
        private void Next()
        {
            _taskIndex++;
            if (_taskIndex >= _steps.Count)
            { 
                IsDone = true;
                return;
            }
            _currentTask = _steps[_taskIndex](_ct);
        }
    }
}