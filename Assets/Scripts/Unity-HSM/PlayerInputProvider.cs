using UnityEngine;
using UnityEngine.InputSystem;

namespace HSM {
    /// <summary>
    /// Default player intent source backed by Input System actions.
    /// </summary>
    public class PlayerInputProvider : IIntentProvider {
        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction runAction;
        public InputAction dodgeAction;
        public InputAction attackAction;

        public void WriteIntent(PlayerContext ctx) {
            if (ctx.inputLocked) {
                ctx.moveInput = Vector2.zero;
                ctx.jumpPressed = false;
                ctx.runHeld = false;
                ctx.dodgePressed = false;
                ctx.attackPressed = false;
                return;
            }

            ctx.moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            ctx.jumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();
            ctx.runHeld = runAction != null && runAction.IsPressed();
            ctx.dodgePressed = dodgeAction != null && dodgeAction.WasPressedThisFrame();
            ctx.attackPressed = attackAction != null && attackAction.WasPressedThisFrame();
        }
    }
}
