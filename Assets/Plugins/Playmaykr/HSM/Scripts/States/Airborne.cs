using Playmaykr.HSM.Activities;
using Playmaykr.HSM.Core;
using UnityEngine;

namespace Playmaykr.HSM.ConcreteStates 
{
    public class Airborne : State
    {
        private readonly PlayerContext _ctx;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            _ctx = ctx;
            
            AddActivity(new ColorPhaseActivity(ctx.renderer)
            {
                enterColor = Color.red, // Runs while Airborne is activating
            });
        }
        
        protected override State GetTransition()
        {
            return _ctx.grounded ? ((PlayerRoot)Parent).Grounded : null;
        }

        protected override void OnEnter()
        {
            // TODO: Update Animator through _ctx.anim
        }
    }
}