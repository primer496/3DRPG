# RPG 最小完整闭环 — 实施记录

> 日期：2026-06-05  
> 分支主题：补全玩家属性系统 + 打通背包-任务奖励链路  
> 原则：最小实现，不含存档系统

---

## 一、问题诊断

| # | 问题 | 根因 |
|---|------|------|
| 1 | 怪物死亡无经验/金币掉落 | `EnemyHealth.Die()` 只广播击杀事件，不发放奖励 |
| 2 | 敌人攻击不扣玩家 HP | `EnemyAttackDetector.ApplyHit()` 只设置击退动画，无伤害 |
| 3 | 玩家无属性系统 | 全项目无 HP/Exp/Gold/Level 定义 |
| 4 | 任务物品奖励不入背包 | `QuestManager.MarkQuestCompleted()` 的 Item 分支只广播 EventBus，未调用 `InventoryModel.AddItem()` |
| 5 | Yarn 对话 `<<GivePlayerItem>>` 无效 | 同 #4，只广播事件 |
| 6 | 任务 Currency/Experience 奖励空转 | 仅打印日志，无实际目标 |
| 7 | 无 HUD 显示 | 无任何 UI 展示玩家数值 |

---

## 二、新建文件

### 2.1 `Assets/Scripts/Player/PlayerStats.cs`

全局命名空间，MonoBehaviour 场景单例。

```
字段:
  maxHP = 100            (Inspector 可调)
  expToNextLevel = 100   (Inspector 初始值，升级公式: level * 100)
  _currentHP / _exp / _gold / _level = 1   (运行时)

事件 (供 UI 订阅):
  OnHPChanged(int current, int max)
  OnExpChanged(int exp, int expToNext)
  OnGoldChanged(int gold)
  OnLevelUp(int newLevel)

方法:
  TakeDamage(int) / Heal(int)      — HP 增减，0 血触发 Die()
  AddExp(int)                       — 经验累积，自动升级循环
  AddGold(int) / SpendGold(int)     — 金币增减，SpendGold 不足返回 false

升级收益: level++; maxHP += 20; currentHP = maxHP; expToNextLevel = level * 100
```

### 2.2 `Assets/Scripts/Player/PlayerHUDController.cs`

全局命名空间，MonoBehaviour。

```
依赖: 挂载于持有 UIDocument (source=PlayerHUD.uxml) 的 GameObject

OnEnable 订阅 PlayerStats 四个事件 → 刷新对应 UI 元素
OnDisable 取消订阅

UI 元素缓存:
  hp-bar (ProgressBar) → value = current/max * 100, title = "current/max"
  exp-bar (ProgressBar) → 同上
  level-label (Label)   → "Lv.X"
  gold-label (Label)    → "1234"
```

### 2.3 `Assets/UIToolKit/HUD/PlayerHUD.uxml`

UIToolkit UXML，三层结构（无 XML 注释，纯 ASCII，UTF-8 no BOM）：

```
hud-root (左上 280px 半透明面板)
  ├─ hp-section  → Label "HP" + ProgressBar (hp-bar)
  ├─ exp-section → Label "EXP" + ProgressBar (exp-bar) + Label (level-label)
  └─ gold-section → Label "Gold" + Label (gold-label)
```

> ⚠️ **踩坑记录**：初版 UXML 包含中文 XML 注释（如 `<!-- HP 条 -->`），Unity 解析报 `XmlException: Invalid character in the given encoding`。修复方式：删除所有 XML 注释，删除 `.meta` 缓存文件，用 PowerShell `UTF8Encoding($false)` 重写（Unity 要求 UTF-8 without BOM）。

### 2.4 `Assets/UIToolKit/HUD/PlayerHUD.uss`

样式：暗色半透明背景 (`rgba(0,0,0,0.65)`)，HP 红色条，EXP 绿色条，金币金色，等级黄色粗体。

---

## 三、修改文件详情

### 3.1 `Assets/Scripts/Unity-HSM/EnemyHealth.cs`

```diff
+ [Header("掉落奖励")]
+ public int expReward = 10;      // 击杀获得经验
+ public int goldReward = 5;      // 击杀获得金币

  private void Die()
  {
      // ...existing code: EventBus.Raise(Kill) + Yarn variable ...
+
+     // 向玩家发放经验与金币掉落
+     if (PlayerStats.Instance != null)
+     {
+         PlayerStats.Instance.AddExp(expReward);
+         PlayerStats.Instance.AddGold(goldReward);
+     }
+
      Destroy(gameObject);
  }
```

### 3.2 `Assets/Scripts/Unity-HSM/AttackDetector/EnemyAttackDetector.cs`

```diff
+ [Header("Damage")]
+ public int attackDamage = 10;   // 每次攻击造成的伤害

  private void ApplyHit(PlayerStateDriver driver)
  {
      driver.ctx.currentHitSource = attackOrigin.position;
      driver.ctx.isHit = true;
+
+     // 扣除玩家血量
+     PlayerStats.Instance?.TakeDamage(attackDamage);
  }
```

### 3.3 `Assets/Scripts/InventorySystem/ViewModel/InventoryViewModel.cs`

```diff
  public class InventoryViewModel : MonoBehaviour
  {
+     public static InventoryViewModel Instance { get; private set; }
+
      // ...

      private void Awake()
      {
+         Instance = this;
          inventoryModel = new InventoryModel();
      }

+     private void OnDestroy()
+     {
+         if (Instance == this) Instance = null;
+     }
  }
```

### 3.4 `Assets/Scripts/QuestSystem/QuestManager.cs`

新增 using：
```diff
+ using InventorySystem.Model;
```

`MarkQuestCompleted()` 奖励发放逻辑重写：

```diff
  foreach (var reward in quest.questData.rewards)
  {
      switch (reward.rewardType)
      {
          case RewardType.Item:
-             // 旧: 仅广播事件
-             EventBus.Instance.Raise(TargetType.Collect, reward.rewardId, reward.amount);
+             // 新: Resources 加载 ItemData → InventoryModel.AddItem()
+             var itemData = Resources.Load<ItemData>($"GameConfigs/PackageModel/{reward.rewardId}");
+             if (itemData != null && InventoryViewModel.Instance != null)
+                 InventoryViewModel.Instance.inventoryModel.AddItem(itemData, reward.amount);

          case RewardType.Currency:
-             // 旧: 仅打印日志
-             Debug.Log(...);
+             // 新: 加金币
+             PlayerStats.Instance?.AddGold(reward.amount);

          case RewardType.Experience:
-             // 旧: 仅打印日志
-             Debug.Log(...);
+             // 新: 加经验
+             PlayerStats.Instance?.AddExp(reward.amount);
      }
  }
```

### 3.5 `Assets/Scripts/QuestSystem/QuestYarnIntegration.cs`

新增 using：
```diff
+ using InventorySystem.Model;
```

`GivePlayerItem()` 重写：
```diff
  private void GivePlayerItem(string itemId, int amount)
  {
-     // 旧: 仅广播事件
-     EventBus.Instance.Raise(TargetType.Collect, itemId, amount);
+     // 新: Resources 加载 ItemData → InventoryModel.AddItem()
+     var itemData = Resources.Load<ItemData>($"GameConfigs/PackageModel/{itemId}");
+     if (itemData != null && InventoryViewModel.Instance != null)
+         InventoryViewModel.Instance.inventoryModel.AddItem(itemData, amount);
  }
```

---

## 四、架构关系图

```
┌──────────────────────────────────────────────────┐
│                PlayerStats (单例)                  │
│  HP / Exp / Gold / Level                          │
│  + TakeDamage  + AddExp  + AddGold                │
└──────┬────────────┬──────────────┬────────────────┘
       │            │              │
       │ 事件订阅   │ 直接调用      │ 直接调用
       ▼            ▼              ▼
┌─────────────┐ ┌──────────┐ ┌──────────────┐
│PlayerHUD    │ │EnemyHealth│ │QuestManager   │
│Controller   │ │.Die()     │ │.MarkQuest-    │
│(UI 刷新)    │ │+expReward │ │Completed()    │
└─────────────┘ │+goldReward│ │+Currency/Exp  │
                └──────────┘ └──────────────┘
                                     │
                              RewardType.Item
                                     │
                                     ▼
                            ┌─────────────────┐
                            │ InventoryModel   │
                            │ .AddItem()       │
                            │ (已有完整实现)    │
                            └─────────────────┘
                                     ▲
                                     │
                            ┌─────────────────┐
                            │QuestYarnIntegration│
                            │ <<GivePlayerItem>> │
                            └─────────────────┘
```

---

## 五、场景配置 Checklist

完成代码后需在 Unity 编辑器中手动操作：

- [ ] **Player 根对象** → 挂载 `PlayerStats` 组件
- [ ] **新建 UIDocument GameObject** (如 "PlayerHUD") → `Source Asset` 设为 `PlayerHUD`，挂载 `PlayerHUDController`
- [ ] 已有的 **InventoryViewModel** GameObject 无需改动（自动成为单例）
- [ ] 每个 **Enemy** Prefab → Inspector 中配置 `expReward` / `goldReward` / `attackDamage`
- [ ] 确认任务 SO 的 `rewardId` 与 `Assets/Resources/GameConfigs/PackageModel/` 下的 `.asset` 文件名一致

---

## 六、注意

1. **UXML 编码**：UXML 文件中的 XML 注释不得包含非 ASCII 字符（如中文），否则 Unity 解析报 `Invalid character in the given encoding` 错误。已修复为英文注释。

2. **csproj 刷新**：Unity 新建 .cs 文件后，需编辑器 regain focus 重新生成 `.csproj`，IDE 中的瞬态编译错误会自动消失。

3. **验证顺序**：受击扣 HP → 击杀加 Exp/Gold → 接任务 → 完成任务领物品/金币/经验 → Yarn 对话发物品。
