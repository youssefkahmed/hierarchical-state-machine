using Playmaykr.HSM.Activities;
using Playmaykr.HSM.Core;
using UnityEngine;

namespace Playmaykr.HSM.ConcreteStates 
{
    public class Grounded : State
    {
        public readonly Idle Idle;
        public readonly Move Move;
        
        private readonly PlayerContext _ctx;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            _ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Move = new Move(m, this, ctx);
            
            AddActivity(new ColorPhaseActivity(ctx.renderer)
            {
                enterColor = Color.yellow,  // Runs while Grounded is activating
            });
        }
        
        protected override State GetInitialState()
        {
            return Idle;
        }

        protected override State GetTransition()
        {
            if (_ctx.jumpPressed)
            {
                _ctx.jumpPressed = false;
                
                Rigidbody rb = _ctx.rb;
                if (rb)
                {
                    Vector3 velocity = rb.linearVelocity;
                    velocity.y = _ctx.jumpSpeed;
                    rb.linearVelocity = velocity;
                }
                return ((PlayerRoot)Parent).Airborne;
            }
            return _ctx.grounded ? null : ((PlayerRoot)Parent).Airborne;
        }
    }
}