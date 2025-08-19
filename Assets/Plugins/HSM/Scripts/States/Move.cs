using Playmaykr.HSM.Core;
using UnityEngine;

namespace Playmaykr.HSM.ConcreteStates
{
    public class Move : State
    {
        private readonly PlayerContext _ctx;

        public Move(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            _ctx = ctx;
        }

        protected override State GetTransition()
        {
            if (!_ctx.grounded)
            {
                return ((PlayerRoot)Parent).Airborne;
            }
            
            return Mathf.Abs(_ctx.move.x) <= 0.01f ? ((Grounded)Parent).Idle : null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            float targetVelocity = _ctx.move.x * _ctx.moveSpeed;
            _ctx.velocity.x = Mathf.MoveTowards(_ctx.velocity.x, targetVelocity, _ctx.accel * deltaTime);
        }
    }
}