namespace HSM {
    /// <summary>
    /// Writes per-frame intent values into context.
    /// </summary>
    public interface IIntentProvider {
        void WriteIntent(PlayerContext ctx);
    }
}
