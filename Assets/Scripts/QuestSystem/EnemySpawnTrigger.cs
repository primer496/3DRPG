using UnityEngine;
using TaskManager;
using FinalRPG.Utils;

namespace QuestSystem
{
    /// <summary>
    /// 当玩家接取 Phase 2 任务（击杀/安抚分支）时，在自身所在位置生成 Enemy。
    /// 挂载到场景中命名 EnemyBronPos 的空 GameObject 上即可。
    /// </summary>
    public class EnemySpawnTrigger : MonoBehaviour
    {
        private const string Phase2KillQuestId  = "ClearBlackForest_Phase2_Kill";
        private const string Phase2PeaceQuestId = "ClearBlackForest_Phase2_Peace";
        private const string EnemyPrefabPath     = "Prefab/Enemy";

        private bool _hasSpawned;

        private void Start()
        {
            QuestManager.Instance.OnQuestUpdated += HandleQuestUpdated;
        }

        private void OnDisable()
        {
            QuestManager.Instance.OnQuestUpdated -= HandleQuestUpdated;
        }

        private void HandleQuestUpdated(QuestInstance quest)
        {
            if (_hasSpawned || quest.isCompleted) return;

            if (quest.questData.id == Phase2KillQuestId || quest.questData.id == Phase2PeaceQuestId)
                SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            _hasSpawned = true;
            QuestManager.Instance.OnQuestUpdated -= HandleQuestUpdated;

            var enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
            if (enemyPrefab == null)
            {
                RPGLog.Error("Quest", $"EnemySpawnTrigger: 无法加载预制件 Resources/{EnemyPrefabPath}");
                return;
            }

            Instantiate(enemyPrefab, transform.position, transform.rotation);
            RPGLog.Debug("Quest", $"已在 {gameObject.name} 生成 Enemy");
        }
    }
}
