using Playmaykr.HSM.Core;
using UnityEngine;

namespace Playmaykr.HSM.ConcreteStates
{
    public class Idle : State
    {
        private readonly PlayerContext _ctx;

        public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            _ctx = ctx;
        }

        protected override State GetTransition()
        {
            return Mathf.Abs(_ctx.move.x) > 0.01f ? ((Grounded)Parent).Move : null;
        }

        protected override void OnEnter()
        {
            _ctx.velocity.x = 0f;
        }
    }
}