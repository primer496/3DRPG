using UnityEngine;

namespace HSM {
    /// <summary>
    /// 挂在 <b>与 Animator 同一 GameObject</b> 上（通常是角色模型根下的子物体）。
    /// 在走路/跑步 clip 的「脚着地」帧添加 Animation Event，函数名 <see cref="OnFootPlant"/>，Int 参数：0=左脚，1=右脚。
    /// </summary>
    public class FootPlantAnimationEvents : MonoBehaviour {
        [Tooltip("父物体或身上的 PlayerStateDriver，用于写入落地脚缓存。")]
        public PlayerStateDriver player;

        /// <summary>
        /// Animation Event 调用的函数名必须与此完全一致。
        /// Int：0 = 左脚着地，1 = 右脚着地（与 Animator StopFoot 一致；若反了请改动画事件里的 0/1）。
        /// </summary>
        public void OnFootPlant(int foot) {
            if (player == null) return;
            player.ctx.RegisterFootPlantFromAnimation(foot);
        }
    }
}
