using UnityEngine;

namespace HSM {
    /// <summary>
    /// Base class for AI-driven intent providers.
    /// </summary>
    public abstract class AIIntentProvider : MonoBehaviour, IIntentProvider {
        public abstract void WriteIntent(PlayerContext ctx);
    }
}
