using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using InventorySystem.Model;
using TaskManager;
using HSM;

namespace DataImporter
{
    /// <summary>
    /// Excel -> CSV -> ScriptableObject pipeline.
    /// Menu: Tools / Data Importer / *
    /// Rules:
    ///   - Uses LoadAssetAtPath + SetDirty to overwrite existing SOs
    ///   - Creates missing SOs automatically when CSV has new entries
    ///   - NEVER calls AssetDatabase.DeleteAsset
    /// </summary>
    public static class GameConfigImporter
    {
        // ---- paths (relative to project root) ----
        private const string k_ItemsCSV      = "Assets/Data/CSV/ItemsConfig.csv";
        private const string k_QuestsCSV     = "Assets/Data/CSV/QuestsConfig_Quests.csv";
        private const string k_ObjectivesCSV = "Assets/Data/CSV/QuestsConfig_Objectives.csv";
        private const string k_RewardsCSV    = "Assets/Data/CSV/QuestsConfig_Rewards.csv";
        private const string k_ItemSODir     = "Assets/Resources/GameConfigs/PackageModel/";
        private const string k_QuestSODir    = "Assets/Resources/GameConfigs/Quest/";

        // ---- character csv paths ----
        private const string k_CharMovementCSV  = "Assets/Data/CSV/CharacterConfig_Movement.csv";
        private const string k_CharJumpCSV      = "Assets/Data/CSV/CharacterConfig_Jump.csv";
        private const string k_CharCombatCSV    = "Assets/Data/CSV/CharacterConfig_Combat.csv";
        private const string k_CharTraversalCSV = "Assets/Data/CSV/CharacterConfig_Traversal.csv";
        private const string k_CharConfigSetCSV = "Assets/Data/CSV/CharacterConfig_ConfigSets.csv";

        // ================================================================
        //  Menu items
        // ================================================================

        /// <summary>
        /// 一键全量导入：先调用 Python 将 Excel 转为 CSV，再依次覆写 ItemData 和 QuestData 的 SO 文件。
        /// </summary>
        [MenuItem("Tools/Data Importer/\u4e00\u952e\u8986\u5199 GameConfigs (Excel->CSV->SO)")]
        public static void ImportAll()
        {
            if (RunPythonExcelToCSV())
            {
                ImportItemData();
                ImportQuestData();
            }
        }

        /// <summary>仅执行 Excel → CSV 转换，不更新任何 SO。</summary>
        [MenuItem("Tools/Data Importer/\u4ec5\u6267\u884c Excel -> CSV")]
        public static void RunExcelToCSVOnly()
        {
            RunPythonExcelToCSV();
        }

        /// <summary>仅覆写 ItemData SO，不重新执行 Excel 转换。</summary>
        [MenuItem("Tools/Data Importer/\u4ec5\u8986\u5199 ItemData (CSV->SO)")]
        public static void ImportItemDataOnly()
        {
            ImportItemData();
        }

        /// <summary>仅覆写 QuestData SO，不重新执行 Excel 转换。</summary>
        [MenuItem("Tools/Data Importer/\u4ec5\u8986\u5199 QuestData (CSV->SO)")]
        public static void ImportQuestDataOnly()
        {
            ImportQuestData();
        }

        /// <summary>
        /// 一键覆写 Character Configs：先执行 Excel → CSV，再依次覆写 MovementConfig、JumpConfig、
        /// CombatConfig、TraversalConfig、ConfigSets 五类 SO。
        /// </summary>
        [MenuItem("Tools/Data Importer/\u4e00\u952e\u8986\u5199 Character Configs (Excel->CSV->SO)")]
        public static void ImportAllCharacter()
        {
            if (RunPythonExcelToCSV())
            {
                ImportCharacterMovement();
                ImportCharacterJump();
                ImportCharacterCombat();
                ImportCharacterTraversal();
                ImportCharacterConfigSets();
            }
        }

        /// <summary>仅覆写 ActorMovementConfig SO（Character 子菜单）。</summary>
        [MenuItem("Tools/Data Importer/Character/\u4ec5\u8986\u5199 MovementConfig")]
        public static void ImportCharacterMovementOnly() { ImportCharacterMovement(); }

        /// <summary>仅覆写 ActorJumpConfig SO（Character 子菜单）。</summary>
        [MenuItem("Tools/Data Importer/Character/\u4ec5\u8986\u5199 JumpConfig")]
        public static void ImportCharacterJumpOnly() { ImportCharacterJump(); }

        /// <summary>仅覆写 ActorCombatConfig SO（Character 子菜单）。</summary>
        [MenuItem("Tools/Data Importer/Character/\u4ec5\u8986\u5199 CombatConfig")]
        public static void ImportCharacterCombatOnly() { ImportCharacterCombat(); }

        /// <summary>仅覆写 ActorTraversalConfig SO（Character 子菜单）。</summary>
        [MenuItem("Tools/Data Importer/Character/\u4ec5\u8986\u5199 TraversalConfig")]
        public static void ImportCharacterTraversalOnly() { ImportCharacterTraversal(); }

        /// <summary>仅覆写 PlayerCapabilityConfigSet / EnemyCapabilityConfigSet SO（Character 子菜单）。</summary>
        [MenuItem("Tools/Data Importer/Character/\u4ec5\u8986\u5199 ConfigSets")]
        public static void ImportCharacterConfigSetsOnly() { ImportCharacterConfigSets(); }

        // ================================================================
        //  Excel -> CSV  (calls python excel_to_csv.py)
        // ================================================================

        /// <summary>
        /// 调用项目根目录的 excel_to_csv.py，将 ExcelConfig/ 下所有 xlsx 转为 CSV/（多 Sheet 加 _SheetName 后缀）。
        /// 成功返回 true，失败记录 LogError 并返回 false。
        /// </summary>
        private static bool RunPythonExcelToCSV()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string scriptPath = Path.Combine(projectRoot, "excel_to_csv.py");

            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError(
                    "[GameConfigImporter] excel_to_csv.py not found: " + scriptPath);
                return false;
            }

            var psi = new ProcessStartInfo("python", "\"" + scriptPath + "\"")
            {
                WorkingDirectory       = projectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding  = Encoding.UTF8,
            };

            using (var proc = Process.Start(psi))
            {
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError(
                        "[GameConfigImporter] excel_to_csv.py failed:\n" + stderr);
                    return false;
                }
                UnityEngine.Debug.Log("[GameConfigImporter] Excel->CSV:\n" + stdout);
            }
            return true;
        }

        // ================================================================
        //  CSV -> ItemData SOs
        // ================================================================

        /// <summary>
        /// 读取 ItemsConfig.csv，按 assetName 定位 PackageModel/ 下 ItemData SO，
        /// 覆写所有文本、枚举、布尔及数值字段。不创建也不删除任何 SO 文件。
        /// </summary>
        private static void ImportItemData()
        {
            string csvPath = AbsPath(k_ItemsCSV);
            if (!File.Exists(csvPath))
            {
                UnityEngine.Debug.LogError(
                    "[GameConfigImporter] Missing CSV: " + k_ItemsCSV);
                return;
            }

            var rows = ReadCSV(csvPath);
            if (rows.Count < 2)
            {
                UnityEngine.Debug.LogWarning(
                    "[GameConfigImporter] ItemsConfig.csv has no data rows");
                return;
            }

            var header   = rows[0];
            int updated  = 0;
            int skipped  = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;

                string assetName = GetCol(cols, header, "assetName");
                if (string.IsNullOrEmpty(assetName) || assetName.StartsWith("#")) continue;

                string assetPath = k_ItemSODir + assetName + ".asset";
                var so = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                if (so == null)
                {
                    // 自动创建缺失的 SO
                    EnsureDirectory(k_ItemSODir);
                    so = ScriptableObject.CreateInstance<ItemData>();
                    AssetDatabase.CreateAsset(so, assetPath);
                    UnityEngine.Debug.Log(
                        "[GameConfigImporter] Created new ItemData: " + assetPath);
                }
                // 同步内部名称与文件名，消除 Unity 的 "Main Object Name does not match filename" 警告
                so.name = assetName;

                so.itemID      = GetCol(cols, header, "itemID");
                so.itemName    = GetCol(cols, header, "itemName");
                so.description = GetCol(cols, header, "description");
                so.iconPath    = GetCol(cols, header, "iconPath");
                so.category    = ParseEnum<ItemCategory>(GetCol(cols, header, "category"));
                so.rarity      = ParseEnum<ItemRarity>(GetCol(cols, header, "rarity"));
                so.isStackable = ParseBool(GetCol(cols, header, "isStackable"));
                so.maxStack    = ParseInt(GetCol(cols, header, "maxStack"), 1);

                EditorUtility.SetDirty(so);
                updated++;
            }

            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format(
                "[GameConfigImporter] ItemData: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  CSV -> QuestData SOs
        // ================================================================

        /// <summary>
        /// 读取 Quests / Objectives / Rewards 三张 CSV，覆写 Quest/ 下 QuestData SO 的基础字段，
        /// 并完全重建目标列表和奖励列表。不创建也不删除任何 SO 文件。
        /// </summary>
        private static void ImportQuestData()
        {
            string questsPath     = AbsPath(k_QuestsCSV);
            string objectivesPath = AbsPath(k_ObjectivesCSV);
            string rewardsPath    = AbsPath(k_RewardsCSV);

            foreach (var p in new[] { questsPath, objectivesPath, rewardsPath })
            {
                if (!File.Exists(p))
                {
                    UnityEngine.Debug.LogError(
                        "[GameConfigImporter] Missing CSV: " + p);
                    return;
                }
            }

            var questRows     = ReadCSV(questsPath);
            var objectiveRows = ReadCSV(objectivesPath);
            var rewardRows    = ReadCSV(rewardsPath);

            if (questRows.Count < 2)
            {
                UnityEngine.Debug.LogWarning(
                    "[GameConfigImporter] Quests CSV has no data rows");
                return;
            }

            var qHeader = questRows[0];
            var oHeader = objectiveRows.Count > 0 ? objectiveRows[0] : new List<string>();
            var rHeader = rewardRows.Count > 0    ? rewardRows[0]    : new List<string>();

            // Build lookup maps: assetName -> rows
            var objectiveMap = BuildMap(objectiveRows, oHeader);
            var rewardMap    = BuildMap(rewardRows, rHeader);

            int updated = 0;
            int skipped = 0;

            for (int i = 1; i < questRows.Count; i++)
            {
                var cols = questRows[i];
                if (cols.Count == 0) continue;

                string assetName = GetCol(cols, qHeader, "assetName");
                if (string.IsNullOrEmpty(assetName) || assetName.StartsWith("#")) continue;

                string assetPath = k_QuestSODir + assetName + ".asset";
                var so = AssetDatabase.LoadAssetAtPath<QuestData>(assetPath);
                if (so == null)
                {
                    // 自动创建缺失的 SO
                    EnsureDirectory(k_QuestSODir);
                    so = ScriptableObject.CreateInstance<QuestData>();
                    AssetDatabase.CreateAsset(so, assetPath);
                    UnityEngine.Debug.Log(
                        "[GameConfigImporter] Created new QuestData: " + assetPath);
                }
                so.name = assetName;

                so.id          = GetCol(cols, qHeader, "id");
                so.title       = GetCol(cols, qHeader, "title");
                so.description = GetCol(cols, qHeader, "description");
                so.isOrdered   = ParseBool(GetCol(cols, qHeader, "isOrdered"));

                // Rebuild objectives list
                so.objectives = new List<QuestObjective>();
                if (objectiveMap.ContainsKey(assetName))
                {
                    foreach (var oRow in objectiveMap[assetName])
                    {
                        so.objectives.Add(new QuestObjective
                        {
                            targetType     = ParseEnum<TargetType>(GetCol(oRow, oHeader, "targetType")),
                            targetId       = GetCol(oRow, oHeader, "targetId"),
                            requiredAmount = ParseInt(GetCol(oRow, oHeader, "requiredAmount"), 1),
                            uiDescription  = GetCol(oRow, oHeader, "uiDescription"),
                        });
                    }
                }

                // Rebuild rewards list
                so.rewards = new List<QuestReward>();
                if (rewardMap.ContainsKey(assetName))
                {
                    foreach (var rRow in rewardMap[assetName])
                    {
                        so.rewards.Add(new QuestReward
                        {
                            rewardType = ParseEnum<RewardType>(GetCol(rRow, rHeader, "rewardType")),
                            rewardId   = GetCol(rRow, rHeader, "rewardId"),
                            amount     = ParseInt(GetCol(rRow, rHeader, "amount"), 0),
                        });
                    }
                }

                EditorUtility.SetDirty(so);
                updated++;
            }

            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format(
                "[GameConfigImporter] QuestData: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  Utilities
        // ================================================================

        /// <summary>将 "Assets/..." 相对路径转为磁盘绝对路径。</summary>
        private static string AbsPath(string relativeAssetPath)
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", relativeAssetPath));
        }

        /// <summary>
        /// 将 CSV 行按 assetName 列分组，返回 assetName → 行列表 字典，
        /// 供多对一关联数据（任务目标、奖励）按主键查询。
        /// </summary>
        private static Dictionary<string, List<List<string>>> BuildMap(
            List<List<string>> rows, List<string> header)
        {
            var map = new Dictionary<string, List<List<string>>>();
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Count == 0) continue;
                string key = GetCol(row, header, "assetName");
                if (!map.ContainsKey(key))
                    map[key] = new List<List<string>>();
                map[key].Add(row);
            }
            return map;
        }

        /// <summary>
        /// Parses a CSV file (UTF-8 BOM supported).
        /// Returns a list of rows; each row is a list of column strings.
        /// Trailing note-only rows (all-empty) are naturally skipped by callers.
        /// </summary>
        private static List<List<string>> ReadCSV(string path)
        {
            var result = new List<List<string>>();
            string text = File.ReadAllText(path, Encoding.UTF8);
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    result.Add(ParseCSVLine(line));
            }
            return result;
        }

        /// <summary>解析单行 CSV，正确处理双引号包裹字段及 ""（转义引号）。</summary>
        private static List<string> ParseCSVLine(string line)
        {
            var result  = new List<string>();
            bool inQ    = false;
            var  cur    = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQ)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        { cur.Append('"'); i++; }
                        else
                            inQ = false;
                    }
                    else
                        cur.Append(c);
                }
                else
                {
                    if (c == '"')   inQ = true;
                    else if (c == ',') { result.Add(cur.ToString()); cur.Clear(); }
                    else               cur.Append(c);
                }
            }
            result.Add(cur.ToString());
            return result;
        }

        /// <summary>按列名从行数据中取值；列不存在或越界时返回空字符串。</summary>
        private static string GetCol(List<string> row, List<string> header, string colName)
        {
            int idx = header.IndexOf(colName);
            if (idx < 0 || idx >= row.Count) return "";
            return row[idx].Trim();
        }

        /// <summary>将字符串解析为枚举（支持名称和整数字面量，大小写不敏感），无法识别时返回默认值并记录警告。</summary>
        private static T ParseEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            if (Enum.TryParse<T>(value, true, out T result)) return result;
            if (int.TryParse(value, out int intVal))
                return (T)Enum.ToObject(typeof(T), intVal);
            UnityEngine.Debug.LogWarning(
                "[GameConfigImporter] Unknown enum '" + value + "' for " + typeof(T).Name);
            return default(T);
        }

        /// <summary>解析布尔值：TRUE / 1 / YES（大小写不敏感）为 true，其余均为 false。</summary>
        private static bool ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            value = value.Trim().ToUpperInvariant();
            return value == "TRUE" || value == "1" || value == "YES";
        }

        /// <summary>解析整数，失败时返回 defaultVal。</summary>
        private static int ParseInt(string value, int defaultVal)
        {
            return int.TryParse(value, out int v) ? v : defaultVal;
        }

        /// <summary>解析浮点数（不变量区域设置，小数点为 .），失败时返回 defaultVal。</summary>
        private static float ParseFloat(string value, float defaultVal)
        {
            if (float.TryParse(value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float v)) return v;
            return defaultVal;
        }

        // ================================================================
        //  CSV -> ActorMovementConfig SOs
        // ================================================================
        /// <summary>
        /// 读取 CharacterConfig_Movement.csv，覆写 ActorMovementConfig SO 的移动速度、加速度、
        /// 跑步倍率及急停相关参数（共 8 个字段）。
        /// </summary>
        private static void ImportCharacterMovement()
        {
            var rows = LoadCSVOrWarn(k_CharMovementCSV);
            if (rows == null) return;
            var hdr = rows[0];
            int updated = 0, skipped = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;
                string ap = GetCol(cols, hdr, "assetPath");
                if (string.IsNullOrEmpty(ap) || ap.StartsWith("#")) continue;

                var so = AssetDatabase.LoadAssetAtPath<ActorMovementConfig>("Assets/" + ap + ".asset");
                if (so == null) { LogSkip(ap); skipped++; continue; }

                so.moveSpeed                = ParseFloat(GetCol(cols, hdr, "moveSpeed"), so.moveSpeed);
                so.accel                    = ParseFloat(GetCol(cols, hdr, "accel"), so.accel);
                so.runSpeedMultiplier       = ParseFloat(GetCol(cols, hdr, "runSpeedMultiplier"), so.runSpeedMultiplier);
                so.enableStopState          = ParseBool(GetCol(cols, hdr, "enableStopState"));
                so.stopDuration             = ParseFloat(GetCol(cols, hdr, "stopDuration"), so.stopDuration);
                so.stopEnterSpeedThreshold  = ParseFloat(GetCol(cols, hdr, "stopEnterSpeedThreshold"), so.stopEnterSpeedThreshold);
                so.stopEnterCrossFade       = ParseFloat(GetCol(cols, hdr, "stopEnterCrossFade"), so.stopEnterCrossFade);
                so.stopSpeedDecayTime       = ParseFloat(GetCol(cols, hdr, "stopSpeedDecayTime"), so.stopSpeedDecayTime);

                EditorUtility.SetDirty(so);
                updated++;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format("[GameConfigImporter] MovementConfig: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  CSV -> ActorJumpConfig SOs
        // ================================================================
        /// <summary>
        /// 读取 CharacterConfig_Jump.csv，覆写 ActorJumpConfig SO 的跳跃高度、起跳速度和离地检测时间（共 3 个字段）。
        /// </summary>
        private static void ImportCharacterJump()
        {
            var rows = LoadCSVOrWarn(k_CharJumpCSV);
            if (rows == null) return;
            var hdr = rows[0];
            int updated = 0, skipped = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;
                string ap = GetCol(cols, hdr, "assetPath");
                if (string.IsNullOrEmpty(ap) || ap.StartsWith("#")) continue;

                var so = AssetDatabase.LoadAssetAtPath<ActorJumpConfig>("Assets/" + ap + ".asset");
                if (so == null) { LogSkip(ap); skipped++; continue; }

                so.jumpHeight           = ParseFloat(GetCol(cols, hdr, "jumpHeight"), so.jumpHeight);
                so.jumpSpeed            = ParseFloat(GetCol(cols, hdr, "jumpSpeed"), so.jumpSpeed);
                so.jumpGroundDetachTime = ParseFloat(GetCol(cols, hdr, "jumpGroundDetachTime"), so.jumpGroundDetachTime);

                EditorUtility.SetDirty(so);
                updated++;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format("[GameConfigImporter] JumpConfig: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  CSV -> ActorCombatConfig SOs
        // ================================================================
        /// <summary>
        /// 读取 CharacterConfig_Combat.csv，覆写 ActorCombatConfig SO 的攻击范围、连击窗口、
        /// 受击反馈、根运动缩放及瞄准辅助参数（共 21 个字段）。
        /// </summary>
        private static void ImportCharacterCombat()
        {
            var rows = LoadCSVOrWarn(k_CharCombatCSV);
            if (rows == null) return;
            var hdr = rows[0];
            int updated = 0, skipped = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;
                string ap = GetCol(cols, hdr, "assetPath");
                if (string.IsNullOrEmpty(ap) || ap.StartsWith("#")) continue;

                var so = AssetDatabase.LoadAssetAtPath<ActorCombatConfig>("Assets/" + ap + ".asset");
                if (so == null) { LogSkip(ap); skipped++; continue; }

                so.attackRange                  = ParseFloat(GetCol(cols, hdr, "attackRange"), so.attackRange);
                so.comboResetTime               = ParseFloat(GetCol(cols, hdr, "comboResetTime"), so.comboResetTime);
                so.maxComboSteps                = ParseInt(GetCol(cols, hdr, "maxComboSteps"), so.maxComboSteps);
                so.useCombatRootMotion          = ParseBool(GetCol(cols, hdr, "useCombatRootMotion"));
                so.combatRootMotionPlanarScale  = ParseFloat(GetCol(cols, hdr, "combatRootMotionPlanarScale"), so.combatRootMotionPlanarScale);
                so.comboExitNormalizedTime      = ParseFloat(GetCol(cols, hdr, "comboExitNormalizedTime"), so.comboExitNormalizedTime);
                so.hitReactionExitNormalizedTime = ParseFloat(GetCol(cols, hdr, "hitReactionExitNormalizedTime"), so.hitReactionExitNormalizedTime);
                so.hitKnockbackSpeed            = ParseFloat(GetCol(cols, hdr, "hitKnockbackSpeed"), so.hitKnockbackSpeed);
                so.hitKnockbackDecay            = ParseFloat(GetCol(cols, hdr, "hitKnockbackDecay"), so.hitKnockbackDecay);
                so.hitStopDuration              = ParseFloat(GetCol(cols, hdr, "hitStopDuration"), so.hitStopDuration);
                so.hitStopRootMotionScale       = ParseFloat(GetCol(cols, hdr, "hitStopRootMotionScale"), so.hitStopRootMotionScale);
                so.aimAssistRadius              = ParseFloat(GetCol(cols, hdr, "aimAssistRadius"), so.aimAssistRadius);
                so.aimAssistAngle               = ParseFloat(GetCol(cols, hdr, "aimAssistAngle"), so.aimAssistAngle);
                so.combo1WindowStart            = ParseFloat(GetCol(cols, hdr, "combo1WindowStart"), so.combo1WindowStart);
                so.combo1WindowEnd              = ParseFloat(GetCol(cols, hdr, "combo1WindowEnd"), so.combo1WindowEnd);
                so.combo2WindowStart            = ParseFloat(GetCol(cols, hdr, "combo2WindowStart"), so.combo2WindowStart);
                so.combo2WindowEnd              = ParseFloat(GetCol(cols, hdr, "combo2WindowEnd"), so.combo2WindowEnd);
                so.combo3WindowStart            = ParseFloat(GetCol(cols, hdr, "combo3WindowStart"), so.combo3WindowStart);
                so.combo3WindowEnd              = ParseFloat(GetCol(cols, hdr, "combo3WindowEnd"), so.combo3WindowEnd);
                so.combo4WindowStart            = ParseFloat(GetCol(cols, hdr, "combo4WindowStart"), so.combo4WindowStart);
                so.combo4WindowEnd              = ParseFloat(GetCol(cols, hdr, "combo4WindowEnd"), so.combo4WindowEnd);

                EditorUtility.SetDirty(so);
                updated++;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format("[GameConfigImporter] CombatConfig: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  CSV -> ActorTraversalConfig SOs
        // ================================================================
        /// <summary>
        /// 读取 CharacterConfig_Traversal.csv，覆写 ActorTraversalConfig SO 的翻越（Vault）和攀爬（Climb）
        /// 全部参数（共 38 个字段），LayerMask 以整数存储并通过隐式转换赋值。
        /// </summary>
        private static void ImportCharacterTraversal()
        {
            var rows = LoadCSVOrWarn(k_CharTraversalCSV);
            if (rows == null) return;
            var hdr = rows[0];
            int updated = 0, skipped = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;
                string ap = GetCol(cols, hdr, "assetPath");
                if (string.IsNullOrEmpty(ap) || ap.StartsWith("#")) continue;

                var so = AssetDatabase.LoadAssetAtPath<ActorTraversalConfig>("Assets/" + ap + ".asset");
                if (so == null) { LogSkip(ap); skipped++; continue; }

                so.vaultDuration                    = ParseFloat(GetCol(cols, hdr, "vaultDuration"), so.vaultDuration);
                so.vaultEnterCrossFade              = ParseFloat(GetCol(cols, hdr, "vaultEnterCrossFade"), so.vaultEnterCrossFade);
                so.vaultExitCrossFade               = ParseFloat(GetCol(cols, hdr, "vaultExitCrossFade"), so.vaultExitCrossFade);
                so.vaultExitNormalizedTime          = ParseFloat(GetCol(cols, hdr, "vaultExitNormalizedTime"), so.vaultExitNormalizedTime);
                so.vaultLateDownStartNormalizedTime = ParseFloat(GetCol(cols, hdr, "vaultLateDownStartNormalizedTime"), so.vaultLateDownStartNormalizedTime);
                so.vaultLateDownSpeed               = ParseFloat(GetCol(cols, hdr, "vaultLateDownSpeed"), so.vaultLateDownSpeed);
                so.vaultMinMoveSpeed                = ParseFloat(GetCol(cols, hdr, "vaultMinMoveSpeed"), so.vaultMinMoveSpeed);
                so.vaultWallMask                    = ParseInt(GetCol(cols, hdr, "vaultWallMask"), so.vaultWallMask.value);
                so.vaultDetectDistance              = ParseFloat(GetCol(cols, hdr, "vaultDetectDistance"), so.vaultDetectDistance);
                so.vaultMaxFacingAngle              = ParseFloat(GetCol(cols, hdr, "vaultMaxFacingAngle"), so.vaultMaxFacingAngle);
                so.vaultMinHeight                   = ParseFloat(GetCol(cols, hdr, "vaultMinHeight"), so.vaultMinHeight);
                so.vaultMaxHeight                   = ParseFloat(GetCol(cols, hdr, "vaultMaxHeight"), so.vaultMaxHeight);
                so.vaultSampleMinHeight             = ParseFloat(GetCol(cols, hdr, "vaultSampleMinHeight"), so.vaultSampleMinHeight);
                so.vaultSampleMaxHeight             = ParseFloat(GetCol(cols, hdr, "vaultSampleMaxHeight"), so.vaultSampleMaxHeight);
                so.vaultHeightSamples               = ParseInt(GetCol(cols, hdr, "vaultHeightSamples"), so.vaultHeightSamples);
                so.vaultDebugLog                    = ParseBool(GetCol(cols, hdr, "vaultDebugLog"));
                so.climbWallMask                    = ParseInt(GetCol(cols, hdr, "climbWallMask"), so.climbWallMask.value);
                so.climbDetectDistance              = ParseFloat(GetCol(cols, hdr, "climbDetectDistance"), so.climbDetectDistance);
                so.climbMaxFacingAngle              = ParseFloat(GetCol(cols, hdr, "climbMaxFacingAngle"), so.climbMaxFacingAngle);
                so.climbSampleMinHeight             = ParseFloat(GetCol(cols, hdr, "climbSampleMinHeight"), so.climbSampleMinHeight);
                so.climbSampleMaxHeight             = ParseFloat(GetCol(cols, hdr, "climbSampleMaxHeight"), so.climbSampleMaxHeight);
                so.climbHeightSamples               = ParseInt(GetCol(cols, hdr, "climbHeightSamples"), so.climbHeightSamples);
                so.climbEnterCrossFade              = ParseFloat(GetCol(cols, hdr, "climbEnterCrossFade"), so.climbEnterCrossFade);
                so.climbExitCrossFade               = ParseFloat(GetCol(cols, hdr, "climbExitCrossFade"), so.climbExitCrossFade);
                so.climbExitNormalizedTime          = ParseFloat(GetCol(cols, hdr, "climbExitNormalizedTime"), so.climbExitNormalizedTime);
                so.climb17ExitNormalizedTime        = ParseFloat(GetCol(cols, hdr, "climb17ExitNormalizedTime"), so.climb17ExitNormalizedTime);
                so.climb17PlanarAssistSpeed         = ParseFloat(GetCol(cols, hdr, "climb17PlanarAssistSpeed"), so.climb17PlanarAssistSpeed);
                so.climb17MinPlanarSpeed            = ParseFloat(GetCol(cols, hdr, "climb17MinPlanarSpeed"), so.climb17MinPlanarSpeed);
                so.climbDebugLog                    = ParseBool(GetCol(cols, hdr, "climbDebugLog"));
                so.wallActionAlignDuration          = ParseFloat(GetCol(cols, hdr, "wallActionAlignDuration"), so.wallActionAlignDuration);
                so.wallActionAlignMinAngle          = ParseFloat(GetCol(cols, hdr, "wallActionAlignMinAngle"), so.wallActionAlignMinAngle);
                so.vaultReferenceWallHeight         = ParseFloat(GetCol(cols, hdr, "vaultReferenceWallHeight"), so.vaultReferenceWallHeight);
                so.climb05ReferenceWallHeight       = ParseFloat(GetCol(cols, hdr, "climb05ReferenceWallHeight"), so.climb05ReferenceWallHeight);
                so.climb10ReferenceWallHeight       = ParseFloat(GetCol(cols, hdr, "climb10ReferenceWallHeight"), so.climb10ReferenceWallHeight);
                so.climb17ReferenceWallHeight       = ParseFloat(GetCol(cols, hdr, "climb17ReferenceWallHeight"), so.climb17ReferenceWallHeight);
                so.climb20ReferenceWallHeight       = ParseFloat(GetCol(cols, hdr, "climb20ReferenceWallHeight"), so.climb20ReferenceWallHeight);
                so.wallActionHeightAdjustSpeed      = ParseFloat(GetCol(cols, hdr, "wallActionHeightAdjustSpeed"), so.wallActionHeightAdjustSpeed);
                so.wallActionMaxUpOffset            = ParseFloat(GetCol(cols, hdr, "wallActionMaxUpOffset"), so.wallActionMaxUpOffset);
                so.wallActionMaxDownOffset          = ParseFloat(GetCol(cols, hdr, "wallActionMaxDownOffset"), so.wallActionMaxDownOffset);

                EditorUtility.SetDirty(so);
                updated++;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format("[GameConfigImporter] TraversalConfig: {0} updated, {1} skipped", updated, skipped));
        }

        // ================================================================
        //  CSV -> PlayerCapabilityConfigSet / EnemyCapabilityConfigSet
        // ================================================================
        /// <summary>
        /// 读取 CharacterConfig_ConfigSets.csv，根据 soType 列（"Player" / "Enemy"）分别定位
        /// PlayerCapabilityConfigSet 或 EnemyCapabilityConfigSet SO，
        /// 仅覆写四个能力开关（enableLocomotion / enableCombat / enableJump / enableTraversal），
        /// 不触碰任何 SO 引用字段（movement / combat / jump / traversal）。
        /// </summary>
        private static void ImportCharacterConfigSets()
        {
            var rows = LoadCSVOrWarn(k_CharConfigSetCSV);
            if (rows == null) return;
            var hdr = rows[0];
            int updated = 0, skipped = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                var cols = rows[i];
                if (cols.Count == 0) continue;
                string ap   = GetCol(cols, hdr, "assetPath");
                string type = GetCol(cols, hdr, "soType");
                if (string.IsNullOrEmpty(ap) || ap.StartsWith("#")) continue;

                string fullPath = "Assets/" + ap + ".asset";
                bool   eLoco    = ParseBool(GetCol(cols, hdr, "enableLocomotion"));
                bool   eCombat  = ParseBool(GetCol(cols, hdr, "enableCombat"));
                bool   eJump    = ParseBool(GetCol(cols, hdr, "enableJump"));
                bool   eTrav    = ParseBool(GetCol(cols, hdr, "enableTraversal"));

                bool didUpdate = false;
                if (type.Equals("Player", StringComparison.OrdinalIgnoreCase))
                {
                    var so = AssetDatabase.LoadAssetAtPath<PlayerCapabilityConfigSet>(fullPath);
                    if (so == null) { LogSkip(ap); skipped++; continue; }
                    so.enableLocomotion = eLoco;
                    so.enableCombat     = eCombat;
                    so.enableJump       = eJump;
                    so.enableTraversal  = eTrav;
                    EditorUtility.SetDirty(so);
                    didUpdate = true;
                }
                else if (type.Equals("Enemy", StringComparison.OrdinalIgnoreCase))
                {
                    var so = AssetDatabase.LoadAssetAtPath<EnemyCapabilityConfigSet>(fullPath);
                    if (so == null) { LogSkip(ap); skipped++; continue; }
                    so.enableLocomotion = eLoco;
                    so.enableCombat     = eCombat;
                    so.enableJump       = eJump;
                    so.enableTraversal  = eTrav;
                    EditorUtility.SetDirty(so);
                    didUpdate = true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[GameConfigImporter] Unknown soType '" + type + "', skipped: " + ap);
                    skipped++;
                    continue;
                }
                if (didUpdate) updated++;
            }
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log(string.Format("[GameConfigImporter] ConfigSets: {0} updated, {1} skipped", updated, skipped));
        }

        // ----------------------------------------------------------------
        //  Shared helper: load CSV and return null if missing/empty
        // ----------------------------------------------------------------
        /// <summary>加载并解析指定相对路径的 CSV 文件；文件不存在或无数据行时记录日志并返回 null。</summary>
        private static List<List<string>> LoadCSVOrWarn(string relPath)
        {
            string path = AbsPath(relPath);
            if (!File.Exists(path))
            {
                UnityEngine.Debug.LogError("[GameConfigImporter] Missing CSV: " + relPath);
                return null;
            }
            var rows = ReadCSV(path);
            if (rows.Count < 2)
            {
                UnityEngine.Debug.LogWarning("[GameConfigImporter] CSV has no data rows: " + relPath);
                return null;
            }
            return rows;
        }

        /// <summary>统一输出"SO 未找到，已跳过"警告日志。</summary>
        private static void LogSkip(string assetPath)
        {
            UnityEngine.Debug.LogWarning("[GameConfigImporter] SO not found, skipped: Assets/" + assetPath + ".asset");
        }

        /// <summary>确保目标目录存在（相对于项目根目录的路径，如 "Assets/Resources/GameConfigs/PackageModel/"）。</summary>
        private static void EnsureDirectory(string relativeAssetDir)
        {
            string absPath = AbsPath(relativeAssetDir);
            if (!Directory.Exists(absPath))
                Directory.CreateDirectory(absPath);
        }
    }
}
