namespace HSM {
    public class PlayerRoot : State {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;
        public readonly HitReaction HitReaction;
        readonly PlayerContext ctx;

        public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null) {
            this.ctx = ctx;
            Grounded = new Grounded(m, this, ctx);
            Airborne = new Airborne(m, this, ctx);
            HitReaction = new HitReaction(m, this, ctx);
        }
        
        protected override State GetInitialState() => Grounded;
        protected override State GetTransition() {
            if (ctx.isHit && Machine.Root.Leaf() != HitReaction) {
                return HitReaction;
            }
            // 已在 Grounded.cs 中通过 !ctx.grounded 处理向 Airborne 的过渡。
            // 不要在 Root 重复检查，否则会导致空中时每帧重置 ActiveChild 引发无限重新起跳动作。
            return null;
        }
    }
}