using UnityEngine;

namespace HSM {
    public interface IMove {
        Vector3 Velocity { get; set; }
        bool IsGrounded { get; }
        void Move(Vector3 moveDelta);
        void SetRotation(Quaternion rotation);
    }
}
