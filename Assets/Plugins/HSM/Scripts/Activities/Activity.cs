using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Playmaykr.HSM.Activities
{
    public enum ActivityMode
    {
        Inactive,
        Activating,
        Active,
        Deactivating
    }

    /// <summary>
    /// Represents an activity that can be activated and deactivated during state machine transitions.
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// The activity's current activation state.
        /// </summary>
        ActivityMode Mode { get; }
        /// <summary>
        /// Activates the activity, transitioning it from Inactive to Active.
        /// </summary>
        Task ActivateAsync(CancellationToken ct);
        /// <summary>
        /// Deactivates the activity, transitioning it from Active to Inactive.
        /// </summary>
        Task DeactivateAsync(CancellationToken ct);
    }

    /// <summary>
    /// Base class for activities that can be used during state machine transitions.
    /// </summary>
    public abstract class Activity : IActivity
    {
        public ActivityMode Mode { get; protected set; } = ActivityMode.Inactive;

        /// <inheritdoc />
        public virtual async Task ActivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Inactive)
            {
                return;
            }
            
            Mode = ActivityMode.Activating;
            await Task.CompletedTask;
            Mode = ActivityMode.Active;
            
            Debug.Log($"Activated {GetType().Name} (mode={Mode})");
        }

        /// <inheritdoc />
        public virtual async Task DeactivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Active)
            {
                return;
            }
            
            Mode = ActivityMode.Deactivating;
            await Task.CompletedTask;
            Mode = ActivityMode.Inactive;
            Debug.Log($"Deactivated {GetType().Name} (mode={Mode})");
        }
    }
}