# FinalRPG 交接说明（中文版）

这份文档给你和下一个 AI 快速接手项目用，尽量讲人话，不讲空话。

## 1. 这是个什么项目

- 引擎版本：`Unity 2022.3.62t7`（见 `ProjectSettings/ProjectVersion.txt`）
- 默认启动场景：`Assets/Scenes/SampleScene.unity`（见 `ProjectSettings/EditorBuildSettings.asset`）
- 核心玩法代码目录：`Assets/Scripts/Unity-HSM`
- 相机代码：`Assets/Scripts/Camera/ThirdPersonCamera.cs`
- 输入配置：`Assets/FinalRPG.inputactions`

一句话：这是一个基于 `CharacterController + HSM(层次状态机)` 的第三人称动作原型。

## 2. 现在代码是怎么跑起来的（核心链路）

核心链路是：

`意图输入(Intent) -> 上下文(PlayerContext) -> 状态机(HSM) -> 移动/动画`

### 具体对应文件

- `IIntentProvider`：定义“每帧往 `PlayerContext` 写意图”
- `PlayerInputProvider`：玩家输入（手柄/键鼠）转意图
- `EnemyBrain`：敌人 AI 转意图
- `PlayerStateDriver`：每帧驱动状态机、应用位移和旋转、同步动画参数
- `States/Move`、`States/Combat`：真正消费意图，决定动作表现

所以：**AI 不直接播动画、不直接切状态，只写意图**。动作系统自己执行。

## 3. 敌人 AI 当前进度（你最关心）

文件：`Assets/Scripts/Unity-HSM/EnemyBrain.cs`

已经做完“行为树意图化”的第一版，行为是你之前确认的那套：

1. 巡逻（站立/走路交替）
2. 发现玩家后跑动追击
3. 进入攻击距离后近战单次攻击
4. 攻击冷却期间保持面向并做轻微站位修正

### 行为优先级

- `Combat > Chase > Patrol`

### 已保留的边界（重要）

- 仍走 `IIntentProvider.WriteIntent(ctx)` 的统一入口
- 没有直接改 Animator 状态机
- 没有绕开 `Move/Combat/Grounded` 的既有状态逻辑

## 4. 你在 Unity 里要检查的关键挂载

在敌人对象上确认：

1. 有 `PlayerStateDriver`
2. `PlayerStateDriver.intentProviderOverride` 指向 `EnemyBrain`
3. `EnemyBrain.target` 指向玩家 Transform
4. `PlayerStateDriver.enemyConfigSet` 已配置（敌人能力配置）
5. 敌人 Animator 参数与 `AnimatorKeys` 契约一致

如果上面没配对，AI 逻辑可能看起来“写了但没动”。

## 5. 关键调参项（先调这些）

`EnemyBrain` 里优先调：

- `detectRange`：发现距离
- `attackRange`：攻击触发距离
- `attackCooldown`：攻击间隔
- `patrolIdleMin / patrolIdleMax`：巡逻停顿节奏
- `patrolReachDistance`：巡逻点到达判定
- `patrolRandomRadius`：无巡逻点时随机巡逻半径
- `turnSpeed`：转身速度

## 6. 目前已知风险/坑位

1. `Packages/manifest.json` 里有本机绝对路径包（`D:/BaiduNetdiskDownload/...`），换机器可能直接爆包。
2. 包源是 `https://packages.tuanjie.cn`，不同网络环境可能拉包失败。
3. 敌人如果误绑了玩家输入 `InputAction`，可能被设备输入污染。
4. 动画控制器如果参数名/状态名和 `AnimatorKeys` 不一致，会出现动作不播或切换异常。
5. 仓库资源改动很多（动画迁移/新增较多），后续合并要特别注意引用完整性。

## 7. 给下一个 AI 的推荐开工顺序

1. 先在 `SampleScene` 里确认挂载关系和 target 引用。
2. 进 Play 模式验证三段行为：巡逻 -> 追击 -> 攻击+冷却。
3. 调整上面那几个核心参数到手感稳定。
4. 最后再做增强（比如冷却期侧移、轻量感知优化），不要一上来重构大系统。

## 8. 如果你只想看一句结论

这版已经把“敌人决策”从硬编码 if-else 迁移成了“行为树式意图输出”，并且保持了和现有动作状态机解耦；现在最该做的是 **场景验证 + 调参收敛**，而不是继续加复杂功能。

