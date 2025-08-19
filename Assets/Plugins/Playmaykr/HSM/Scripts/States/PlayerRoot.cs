using Playmaykr.HSM.Core;

namespace Playmaykr.HSM.ConcreteStates
{
    public class PlayerRoot : State
    {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;

        private readonly PlayerContext _ctx;

        public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m)
        {
            _ctx = ctx;
            Grounded = new Grounded(m, this, ctx);
            Airborne = new Airborne(m, this, ctx);
        }
        
        protected override State GetInitialState()
        {
            return Grounded;
        }
        
        protected override State GetTransition()
        {
            return _ctx.grounded ? null : Airborne;
        }
    }
}