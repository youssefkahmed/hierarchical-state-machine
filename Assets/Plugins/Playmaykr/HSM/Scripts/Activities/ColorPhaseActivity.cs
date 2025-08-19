using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Playmaykr.HSM.Activities
{
    public class ColorPhaseActivity : Activity
    {
        public Color enterColor = Color.red;
        public Color exitColor  = Color.yellow;
        
        private readonly Material _mat; // Cached instance to avoid repeated .material allocations
        
        public ColorPhaseActivity(Renderer r)
        {
            if (r!= null)
            {
                _mat = r.material; // Clone once per activity/state
            }
        }

        public override Task ActivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Inactive || !_mat)
            {
                return Task.CompletedTask;
            }
            
            Mode = ActivityMode.Activating;
            _mat.color = enterColor;
            Mode = ActivityMode.Active;
            return Task.CompletedTask;
        }

        public override Task DeactivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Active || !_mat)
            {
                return Task.CompletedTask;
            }
            
            Mode = ActivityMode.Deactivating;
            _mat.color = exitColor;
            Mode = ActivityMode.Inactive;
            return Task.CompletedTask;
        }
    }
}