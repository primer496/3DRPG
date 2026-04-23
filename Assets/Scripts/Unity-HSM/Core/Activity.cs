using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HSM {
    public enum ActivityMode { Inactive, Activating, Active, Deactivating }

    public interface IActivity {
        ActivityMode Mode { get; }
        Task ActivateAsync(CancellationToken ct);
        Task DeactivateAsync(CancellationToken ct);
    }

    public abstract class Activity : IActivity {
        public ActivityMode Mode { get; protected set; } = ActivityMode.Inactive;

        public virtual async Task ActivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Inactive) return;
            
            Mode = ActivityMode.Activating;
            await Task.CompletedTask;
            Mode = ActivityMode.Active;
            Debug.Log($"Activated {GetType().Name} (mode={Mode})");
        }

        public virtual async Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active) return;
            
            Mode = ActivityMode.Deactivating;
            await Task.CompletedTask;
            Mode = ActivityMode.Inactive;
            Debug.Log($"Deactivated {GetType().Name} (mode={Mode})");
        }
    }

    public class AnimatorStateExitActivity : Activity {
        readonly Func<Animator> animatorProvider;
        readonly string stateName;
        readonly int layerIndex;
        readonly float timeoutSeconds;
        readonly bool requireSeenStateBeforeExit;
        readonly Func<bool> shouldWait;

        public AnimatorStateExitActivity(
            Func<Animator> animatorProvider,
            string stateName,
            int layerIndex = 0,
            float timeoutSeconds = 2f,
            bool requireSeenStateBeforeExit = true,
            Func<bool> shouldWait = null
        ) {
            this.animatorProvider = animatorProvider;
            this.stateName = stateName;
            this.layerIndex = layerIndex;
            this.timeoutSeconds = Mathf.Max(0.01f, timeoutSeconds);
            this.requireSeenStateBeforeExit = requireSeenStateBeforeExit;
            this.shouldWait = shouldWait;
        }

        public override async Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active) return;
            Mode = ActivityMode.Deactivating;

            bool needWait = shouldWait == null || shouldWait();
            if (needWait) {
                Animator anim = animatorProvider != null ? animatorProvider() : null;
                if (anim != null) {
                    float start = Time.time;
                    bool hasSeenTarget = !requireSeenStateBeforeExit;

                    while (true) {
                        ct.ThrowIfCancellationRequested();

                        var info = anim.GetCurrentAnimatorStateInfo(layerIndex);
                        bool inTargetState = info.IsName(stateName);
                        if (inTargetState) hasSeenTarget = true;

                        if (hasSeenTarget && !inTargetState) break;
                        if (Time.time - start >= timeoutSeconds) break;

                        await Task.Yield();
                    }
                }
            }

            Mode = ActivityMode.Inactive;
            Debug.Log($"Deactivated {GetType().Name} (mode={Mode})");
        }
    }
}