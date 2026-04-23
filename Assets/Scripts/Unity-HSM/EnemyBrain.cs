using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HSM {
    /// <summary>
    /// Enemy intent provider driven by a minimal behavior tree.
    /// The tree only writes intent into PlayerContext.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBrain : AIIntentProvider, IMove {
        [Header("Ranges")]
        [Min(0f)]
        public float detectRange = 5f;

        [Header("Pacing")]
        [Min(0f)]
        public float turnSpeed = 12f;
        [Min(0.05f)]
        public float attackCooldown = 1.5f;

        [Header("Patrol Points")]
        public Transform patrolPoint1;
        public Transform patrolPoint2;
        public Transform patrolPoint3;

        [Header("Patrol Settings")]
        [Min(0f)]
        public float patrolReachDistance = 0.4f;
        [Min(0.1f)]
        public float patrolIdleDuration = 2f;

        public bool drawDebugGizmos = true;

        BehaviorNode rootNode;
        readonly EnemyBlackboard blackboard = new EnemyBlackboard();
        bool initialized;
        NavMeshAgent agent; 
        Transform detectedPlayer;
        Transform[] patrolPoints;

        public override void WriteIntent(PlayerContext ctx) {
            EnsureInitialized(ctx);

            SyncAgentWithTransform();

            UpdateDetection();
            blackboard.attackCooldownTimer = Mathf.Max(0f, blackboard.attackCooldownTimer - Time.deltaTime);
            ConfigureFacingProvider(ctx);
            ResetIntent(ctx);
            rootNode.Tick(this, ctx, blackboard, Time.deltaTime);
        }

        void UpdateDetection() {
            detectedPlayer = null;
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectRange);
            foreach (var col in colliders) {
                if (col.CompareTag("Player")) {
                    detectedPlayer = col.transform;
                    break;
                }
            }
        }

        // 处理 CharacterController 与 NavMeshAgent 的冲突和打架问题
        public Vector3 Velocity {
            get => agent != null ? agent.velocity : Vector3.zero;
            set {
                if (agent != null) agent.velocity = value;
            }
        }
        public bool IsGrounded => agent != null && agent.isOnNavMesh && !agent.isOnOffMeshLink;

        public void Move(Vector3 moveDelta) {
            if (agent != null && agent.isOnNavMesh) {
                // 核心修复：由于Agent内部在不停自发模拟移动，这里必须强行用当前真实位置抹除它的这部分增量
                // 这样才能确保接下来的代理位移仅仅是由 StateMachine 唯一决定的，没有任何额外相加
                agent.nextPosition = transform.position;

                // 如果外部要求停下（比如进入了攻击等冻结移动的状态）且传入量为0，主动归零
                if (moveDelta.sqrMagnitude < 0.00001f) {
                    agent.velocity = Vector3.zero;
                } else {
                    agent.Move(moveDelta);
                }
                transform.position = agent.nextPosition;
            } else {
                transform.position += moveDelta;
            }
        }

        public void SetRotation(Quaternion rotation) {
            transform.rotation = rotation;
        }

        void SyncAgentWithTransform() {
            if (agent == null) return;
            // Since agent is pure NavMesh move driver, it handles its own coordinates!
            if (!agent.isOnNavMesh) {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
                    agent.Warp(hit.position);
                }
            }
        }

        void Awake() {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null) {
                agent.updatePosition = false; // 依靠 agent.Move() 能够同时更新 Transform 和 Agent 内部坐标
                agent.updateRotation = false; // 由底层驱动旋转
            }
            BuildBehaviorTree();
        }

        void EnsureInitialized(PlayerContext ctx) {
            if (initialized) {
                return;
            }

            // 敌人不需要急停动作，也不需要寻找 StopFoot 参数
            ctx.enableStopState = false;

            // 动态对齐 NavMeshAgent 的停止距离，确保它一定能走到触发攻击的射程内，而不会站在攻击范围外看戏
            if (agent != null) {
                agent.stoppingDistance = Mathf.Max(0.1f, ctx.attackRange * 0.8f);
            }

            patrolPoints = GetPatrolPoints();

            blackboard.spawnPosition = transform.position;
            blackboard.patrolIdlePhase = true;
            blackboard.patrolPhaseTimer = patrolIdleDuration;
            initialized = true;
        }

        Transform[] GetPatrolPoints() {
            List<Transform> list = new List<Transform>();
            if (patrolPoint1 != null) list.Add(patrolPoint1);
            if (patrolPoint2 != null) list.Add(patrolPoint2);
            if (patrolPoint3 != null) list.Add(patrolPoint3);
            return list.ToArray();
        }

        void BuildBehaviorTree() {
            rootNode = new SelectorNode(
                new SequenceNode(
                    new ConditionNode((brain, ctx, bb) => brain.IsTargetInAttackRange(ctx)),
                    new SelectorNode(
                        new SequenceNode(
                            new ConditionNode((brain, ctx, bb) => brain.IsAttackReady(bb)),
                            // 修正：调用一次攻击即成功（已包含向目标朝向的Intent），处于CD时不再持续给朝向
                            new ActionNode((brain, ctx, bb, _) => brain.AttackMeleeOnce(ctx, bb))
                        ),
                        // AttackCooldownHold不再主动给旋转朝向，让角色可以停在原地或执行别的动画
                        new ActionNode((brain, ctx, bb, _) => brain.AttackCooldownHold(ctx))
                    )
                ),
                new SequenceNode(
                    new ConditionNode((brain, ctx, bb) => brain.IsTargetInDetectRange()),
                    new ActionNode((brain, ctx, bb, _) => brain.ChaseTarget(ctx, bb))
                ),
                new SelectorNode(
                    new SequenceNode(
                        new ConditionNode((brain, ctx, bb) => bb.patrolIdlePhase),
                        new ActionNode((brain, ctx, bb, dt) => brain.PatrolIdle(ctx, bb, dt))
                    ),
                    new ActionNode((brain, ctx, bb, dt) => brain.PatrolWalk(ctx, bb))
                )
            );
        }

        bool TryGetTargetDistance(out float distance) {
            distance = 0f;
            if (detectedPlayer == null) {
                return false;
            }

            // 使用平面(Planar)距离进行检测！避免因为玩家在斜坡较高处，导致3D距离被拉长使得敌人永远认为达不到攻击范围。
            Vector3 toTarget = detectedPlayer.position - transform.position;
            toTarget.y = 0f;
            distance = toTarget.magnitude;
            return true;
        }

        bool IsTargetInDetectRange() {
            return TryGetTargetDistance(out float distance) && distance <= detectRange;
        }

        bool IsTargetInAttackRange(PlayerContext ctx) {
            return TryGetTargetDistance(out float distance) && distance <= ctx.attackRange;
        }

        bool IsAttackReady(EnemyBlackboard board) {
            return board.attackCooldownTimer <= 0f;
        }

        BehaviorStatus AttackMeleeOnce(PlayerContext ctx, EnemyBlackboard board) {
            if (detectedPlayer == null) {
                return BehaviorStatus.Failure;
            }

            if (agent != null && agent.isOnNavMesh) {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            Vector3 toTarget = detectedPlayer.position - transform.position;
            toTarget.y = 0f;
            UpdateRotationIntent(ctx, toTarget);
            ctx.moveInput = Vector2.zero;
            ctx.runHeld = false;
            ctx.attackPressed = true;
            board.attackCooldownTimer = attackCooldown;
            return BehaviorStatus.Success;
        }

        BehaviorStatus AttackCooldownHold(PlayerContext ctx) {
            if (detectedPlayer == null || !TryGetTargetDistance(out float distance)) {
                if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
                return BehaviorStatus.Failure;
            }

            if (agent != null && agent.isOnNavMesh) {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // 之前在这里每帧执行朝向会导致敌人原地打转
            // 原本注释“只在进入攻击状态的那一刻做一次朝向”应由 AttackMeleeOnce 负责完成
            ctx.moveInput = Vector2.zero;
            ctx.runHeld = false;

            return BehaviorStatus.Running;
        }

        BehaviorStatus ChaseTarget(PlayerContext ctx, EnemyBlackboard board) {
            if (detectedPlayer == null || agent == null) {
                return BehaviorStatus.Failure;
            }

            if (agent.isOnNavMesh) {
                agent.isStopped = false;
            }

            // 清除之前的巡逻目标缓存，以防脱战后继续往玩家之前的位置走
            board.hasPatrolDestination = false; 

            agent.SetDestination(detectedPlayer.position);

            // 兜底设计：如果在斜坡等不可寻路区域，Agent 会给出局部路径(PathPartial)或停止(Velocity=0)
            // 此时如果玩家依然不在攻击范围内，应该直接用方向盘操控强行推向玩家。
            Vector3 desiredDir = agent.desiredVelocity;
            if (desiredDir.sqrMagnitude < 0.001f || agent.pathStatus == NavMeshPathStatus.PathPartial) {
                Vector3 toTarget = detectedPlayer.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f) {
                    desiredDir = toTarget.normalized;
                }
            }

            SimulatePathMovement(ctx, true, desiredDir);
            return BehaviorStatus.Running;
        }

        BehaviorStatus PatrolIdle(PlayerContext ctx, EnemyBlackboard board, float deltaTime) {
            ctx.moveInput = Vector2.zero;
            ctx.runHeld = false;
            board.patrolPhaseTimer -= deltaTime;
            if (board.patrolPhaseTimer > 0f) {
                return BehaviorStatus.Running;
            }

            board.patrolIdlePhase = false;
            board.hasPatrolDestination = false;
            return BehaviorStatus.Success;
        }

        BehaviorStatus PatrolWalk(PlayerContext ctx, EnemyBlackboard board) {
            if (agent == null) return BehaviorStatus.Failure;

            if (agent.isOnNavMesh) {
                agent.isStopped = false;
            }

            if (!board.hasPatrolDestination) {
                board.patrolDestination = ResolveNextPatrolDestination(board);
                board.hasPatrolDestination = true;
                agent.SetDestination(board.patrolDestination); // 使用NavMeshAgent自动寻路
            }

            if (!agent.pathPending && agent.remainingDistance <= patrolReachDistance) {
                board.patrolIdlePhase = true;
                board.patrolPhaseTimer = SamplePatrolIdleDuration();
                board.hasPatrolDestination = false;
                ctx.moveInput = Vector2.zero;
                agent.ResetPath();
                return BehaviorStatus.Success;
            }

            SimulatePathMovement(ctx, false, agent.desiredVelocity);
            return BehaviorStatus.Running;
        }

        void SimulatePathMovement(PlayerContext ctx, bool run, Vector3 desiredDir) {
            if (agent == null) return;

            // 根据状态机给定的行走/跑速度动态更新真实Agent的寻路速度上限
            agent.speed = run ? ctx.GetRunRealSpeed() : ctx.GetWalkRealSpeed();

            desiredDir.y = 0f;

            if (desiredDir.sqrMagnitude > 0.001f) {
                desiredDir.Normalize();
                UpdateRotationIntent(ctx, desiredDir);

                // 让状态机直接根据世界方向计算输入，就像摇杆一样
                Vector2 globalInput = new Vector2(desiredDir.x, desiredDir.z).normalized;
                ctx.moveInput = globalInput;
                ctx.runHeld = run;
            } else {
                ctx.moveInput = Vector2.zero;
                ctx.runHeld = false;
            }
        }

        Vector3 ResolveNextPatrolDestination(EnemyBlackboard board) {
            if (patrolPoints != null && patrolPoints.Length > 0) {
                // Ensure index is positive
                int currentIndex = board.patrolPointIndex;
                if (currentIndex < 0) currentIndex = 0;

                int selected = currentIndex % patrolPoints.Length;
                board.patrolPointIndex = (selected + 1) % patrolPoints.Length;
                if (patrolPoints[selected] != null) {
                    return patrolPoints[selected].position;
                }
            }

            return board.spawnPosition;
        }

        float SamplePatrolIdleDuration() {
            return patrolIdleDuration;
        }

        void OnDrawGizmosSelected() {
            if (!drawDebugGizmos) {
                return;
            }

            Vector3 center = transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, detectRange));

            Gizmos.color = Color.green;
            Transform[] points = new Transform[] { patrolPoint1, patrolPoint2, patrolPoint3 };
            for (int i = 0; i < points.Length; i++) {
                Transform point = points[i];
                if (point == null) {
                    continue;
                }

                Gizmos.DrawSphere(point.position, 0.08f);
            }
        }

        void ConfigureFacingProvider(PlayerContext ctx) {
            // Because Enemy moveInput is set as global direction relative to absolute zero rotation
            ctx.facingYawProvider = () => 0f;
        }

        void UpdateRotationIntent(PlayerContext ctx, Vector3 toTarget) {
            if (toTarget.sqrMagnitude <= 0.0001f) {
                return;
            }

            ctx.rotationTarget = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            ctx.rotationTurnSpeed = Mathf.Max(0f, turnSpeed);
            ctx.hasRotationTarget = true;
        }

        static void ResetIntent(PlayerContext ctx) {
            ctx.moveInput = Vector2.zero;
            ctx.jumpPressed = false;
            ctx.runHeld = false;
            ctx.dodgePressed = false;
            ctx.attackPressed = false;
        }
    }

    public enum BehaviorStatus {
        Success,
        Failure,
        Running
    }

    public sealed class EnemyBlackboard {
        public float attackCooldownTimer;
        public bool patrolIdlePhase = true;
        public float patrolPhaseTimer;
        public int patrolPointIndex;
        public Vector3 patrolDestination;
        public bool hasPatrolDestination;
        public Vector3 spawnPosition;
    }

    public abstract class BehaviorNode {
        public abstract BehaviorStatus Tick(EnemyBrain brain, PlayerContext ctx, EnemyBlackboard blackboard, float deltaTime);
    }

    public sealed class SequenceNode : BehaviorNode {
        readonly IReadOnlyList<BehaviorNode> children;

        public SequenceNode(params BehaviorNode[] children) {
            this.children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override BehaviorStatus Tick(EnemyBrain brain, PlayerContext ctx, EnemyBlackboard blackboard, float deltaTime) {
            for (int i = 0; i < children.Count; i++) {
                var result = children[i].Tick(brain, ctx, blackboard, deltaTime);
                if (result != BehaviorStatus.Success) {
                    return result;
                }
            }

            return BehaviorStatus.Success;
        }
    }

    public sealed class SelectorNode : BehaviorNode {
        readonly IReadOnlyList<BehaviorNode> children;

        public SelectorNode(params BehaviorNode[] children) {
            this.children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override BehaviorStatus Tick(EnemyBrain brain, PlayerContext ctx, EnemyBlackboard blackboard, float deltaTime) {
            for (int i = 0; i < children.Count; i++) {
                var result = children[i].Tick(brain, ctx, blackboard, deltaTime);
                if (result != BehaviorStatus.Failure) {
                    return result;
                }
            }

            return BehaviorStatus.Failure;
        }
    }

    public sealed class ConditionNode : BehaviorNode {
        readonly Func<EnemyBrain, PlayerContext, EnemyBlackboard, bool> predicate;

        public ConditionNode(Func<EnemyBrain, PlayerContext, EnemyBlackboard, bool> predicate) {
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public override BehaviorStatus Tick(EnemyBrain brain, PlayerContext ctx, EnemyBlackboard blackboard, float deltaTime) {
            return predicate(brain, ctx, blackboard) ? BehaviorStatus.Success : BehaviorStatus.Failure;
        }
    }

    public sealed class ActionNode : BehaviorNode {
        readonly Func<EnemyBrain, PlayerContext, EnemyBlackboard, float, BehaviorStatus> action;

        public ActionNode(Func<EnemyBrain, PlayerContext, EnemyBlackboard, float, BehaviorStatus> action) {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override BehaviorStatus Tick(EnemyBrain brain, PlayerContext ctx, EnemyBlackboard blackboard, float deltaTime) {
            return action(brain, ctx, blackboard, deltaTime);
        }
    }
}
