using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Playmaykr.HSM.Activities
{
    public class DelayActivationActivity : Activity
    {
        private readonly float _seconds;

        public DelayActivationActivity(float seconds)
        {
            _seconds = seconds;
        }

        public override async Task ActivateAsync(CancellationToken ct)
        {
            Debug.Log($"Activating {GetType().Name} (mode={Mode}) after {_seconds} seconds");
            
            await Task.Delay(TimeSpan.FromSeconds(_seconds), ct);
            await base.ActivateAsync(ct);
        }
    }
}