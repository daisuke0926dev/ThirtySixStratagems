using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Stratagem
{
    /// <summary>
    /// 計略発動条件チェッカー
    /// 各計略の使用可否と条件を詳細にチェック
    /// </summary>
    public class StratagemConditionChecker : MonoBehaviour
    {
        public static StratagemConditionChecker Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Main Check Methods

        /// <summary>
        /// 計略が使用可能かチェック（詳細結果を返す）
        /// </summary>
        public StratagemCheckResult CheckStratagem(StratagemData stratagem, string factionId,
            string characterId, string targetId = null)
        {
            var result = new StratagemCheckResult
            {
                StratagemId = stratagem.StratagemId,
                CanUse = true,
                FailedConditions = new List<ConditionCheckResult>()
            };

            var faction = GameManager.Instance?.GetFaction(factionId);
            var character = GameManager.Instance?.GetCharacter(characterId);

            if (faction == null)
            {
                result.CanUse = false;
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Faction",
                    IsMet = false,
                    Description = "勢力が見つかりません"
                });
                return result;
            }

            // コストチェック
            CheckCostConditions(result, stratagem, faction);

            // フェーズチェック
            CheckPhaseConditions(result, stratagem);

            // 対象チェック
            if (!string.IsNullOrEmpty(targetId))
            {
                CheckTargetConditions(result, stratagem, factionId, targetId);
            }
            else if (stratagem.TargetType != StratagemTarget.Self)
            {
                result.CanUse = false;
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "対象が指定されていません"
                });
            }

            // カスタム条件チェック
            CheckCustomConditions(result, stratagem, faction, character, targetId);

            // カテゴリ特有条件チェック
            CheckCategoryConditions(result, stratagem, faction, character);

            // 全条件が満たされているか確認
            result.CanUse = result.FailedConditions.Count == 0;

            return result;
        }

        /// <summary>
        /// 簡易チェック（使用可否のみ）
        /// </summary>
        public bool CanUseStratagem(StratagemData stratagem, string factionId,
            string characterId, string targetId = null)
        {
            var result = CheckStratagem(stratagem, factionId, characterId, targetId);
            return result.CanUse;
        }

        /// <summary>
        /// 使用可能な計略一覧を取得
        /// </summary>
        public List<StratagemAvailability> GetAvailableStratagems(string factionId, string characterId)
        {
            var availabilityList = new List<StratagemAvailability>();

            if (StratagemManager.Instance?.Database == null)
                return availabilityList;

            foreach (var stratagem in StratagemManager.Instance.Database.AllStratagems)
            {
                var result = CheckStratagem(stratagem, factionId, characterId, null);

                availabilityList.Add(new StratagemAvailability
                {
                    StratagemData = stratagem,
                    IsAvailable = result.CanUse || result.FailedConditions.TrueForAll(c => c.ConditionType == "Target"),
                    CheckResult = result
                });
            }

            return availabilityList;
        }

        #endregion

        #region Cost Conditions

        /// <summary>
        /// コスト条件をチェック
        /// </summary>
        private void CheckCostConditions(StratagemCheckResult result, StratagemData stratagem, Faction faction)
        {
            // 計略ポイントチェック
            if (faction.StratagemPoints < stratagem.CostSP)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "StratagemPoints",
                    IsMet = false,
                    CurrentValue = faction.StratagemPoints,
                    RequiredValue = stratagem.CostSP,
                    Description = $"計略ポイント不足（現在: {faction.StratagemPoints} / 必要: {stratagem.CostSP}）"
                });
            }

            // 金チェック
            if (stratagem.CostGold > 0 && faction.Gold < stratagem.CostGold)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Gold",
                    IsMet = false,
                    CurrentValue = faction.Gold,
                    RequiredValue = stratagem.CostGold,
                    Description = $"金不足（現在: {faction.Gold} / 必要: {stratagem.CostGold}）"
                });
            }
        }

        #endregion

        #region Phase Conditions

        /// <summary>
        /// フェーズ条件をチェック
        /// </summary>
        private void CheckPhaseConditions(StratagemCheckResult result, StratagemData stratagem)
        {
            if (TurnManager.Instance == null) return;

            var currentPhase = TurnManager.Instance.CurrentPhase;

            // 計略は主に外交フェーズで使用
            // ただしカテゴリによって異なる
            bool validPhase = false;

            switch (stratagem.Category)
            {
                case StratagemCategory.Winning:
                case StratagemCategory.Enemy:
                case StratagemCategory.Chaos:
                    // 外交・軍事フェーズで使用可能
                    validPhase = currentPhase == TurnPhase.Diplomacy || currentPhase == TurnPhase.Military;
                    break;

                case StratagemCategory.Attack:
                case StratagemCategory.Merge:
                    // 軍事フェーズで使用可能
                    validPhase = currentPhase == TurnPhase.Military;
                    break;

                case StratagemCategory.Defeat:
                    // いつでも使用可能（敗戦計は緊急用）
                    validPhase = true;
                    break;
            }

            if (!validPhase)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Phase",
                    IsMet = false,
                    Description = $"現在のフェーズ（{TurnManager.Instance.GetPhaseName(currentPhase)}）では使用できません"
                });
            }
        }

        #endregion

        #region Target Conditions

        /// <summary>
        /// 対象条件をチェック
        /// </summary>
        private void CheckTargetConditions(StratagemCheckResult result, StratagemData stratagem,
            string casterFactionId, string targetId)
        {
            switch (stratagem.TargetType)
            {
                case StratagemTarget.Self:
                    // 自分自身、常にOK
                    break;

                case StratagemTarget.EnemyFaction:
                    CheckEnemyFactionTarget(result, casterFactionId, targetId);
                    break;

                case StratagemTarget.EnemyArmy:
                    CheckEnemyArmyTarget(result, casterFactionId, targetId);
                    break;

                case StratagemTarget.EnemyCharacter:
                    CheckEnemyCharacterTarget(result, casterFactionId, targetId);
                    break;

                case StratagemTarget.EnemyTerritory:
                    CheckEnemyTerritoryTarget(result, casterFactionId, targetId);
                    break;

                case StratagemTarget.Any:
                    // どの対象でもOK
                    break;
            }
        }

        private void CheckEnemyFactionTarget(StratagemCheckResult result, string casterFactionId, string targetId)
        {
            var targetFaction = GameManager.Instance?.GetFaction(targetId);

            if (targetFaction == null)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "対象勢力が見つかりません"
                });
                return;
            }

            if (targetId == casterFactionId)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "自勢力を対象にできません"
                });
            }

            // 同盟国への計略は制限
            var casterFaction = GameManager.Instance?.GetFaction(casterFactionId);
            if (casterFaction?.AllianceIds.Contains(targetId) == true)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "同盟国には計略を使用できません"
                });
            }
        }

        private void CheckEnemyArmyTarget(StratagemCheckResult result, string casterFactionId, string targetId)
        {
            var army = GameManager.Instance?.GetArmy(targetId);

            if (army == null)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "対象軍が見つかりません"
                });
                return;
            }

            if (army.FactionId == casterFactionId)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "自軍を対象にできません"
                });
            }
        }

        private void CheckEnemyCharacterTarget(StratagemCheckResult result, string casterFactionId, string targetId)
        {
            var character = GameManager.Instance?.GetCharacter(targetId);

            if (character == null)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "対象武将が見つかりません"
                });
                return;
            }

            if (character.FactionId == casterFactionId)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "自勢力の武将を対象にできません"
                });
            }
        }

        private void CheckEnemyTerritoryTarget(StratagemCheckResult result, string casterFactionId, string targetId)
        {
            var territory = GameManager.Instance?.GetTerritory(targetId);

            if (territory == null)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "対象領地が見つかりません"
                });
                return;
            }

            if (territory.OwnerId == casterFactionId)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Target",
                    IsMet = false,
                    Description = "自領地を対象にできません"
                });
            }
        }

        #endregion

        #region Custom Conditions

        /// <summary>
        /// カスタム条件をチェック
        /// </summary>
        private void CheckCustomConditions(StratagemCheckResult result, StratagemData stratagem,
            Faction faction, Character caster, string targetId)
        {
            var context = CreateConditionContext(faction, caster, targetId);

            foreach (var condition in stratagem.Conditions)
            {
                if (!condition.IsMet(context))
                {
                    result.FailedConditions.Add(new ConditionCheckResult
                    {
                        ConditionType = condition.Type.ToString(),
                        IsMet = false,
                        Description = condition.Description ?? GetConditionDescription(condition, context)
                    });
                }
            }
        }

        /// <summary>
        /// 条件の説明を生成
        /// </summary>
        private string GetConditionDescription(StratagemCondition condition, ConditionContext context)
        {
            switch (condition.Type)
            {
                case ConditionType.MinSoldiers:
                    return $"最低兵力{condition.Value}が必要（現在: {context.ArmySoldiers}）";
                case ConditionType.MaxSoldiers:
                    return $"兵力{condition.Value}以下が必要（現在: {context.ArmySoldiers}）";
                case ConditionType.MinGold:
                    return $"金{condition.Value}以上が必要（現在: {context.FactionGold}）";
                case ConditionType.MinStratagemPoints:
                    return $"計略ポイント{condition.Value}以上が必要（現在: {context.StratagemPoints}）";
                case ConditionType.EnemyInWar:
                    return "敵と戦争状態である必要があります";
                case ConditionType.HasAlliance:
                    return "同盟を持っている必要があります";
                case ConditionType.TerritoryCount:
                    return $"領地{condition.Value}以上が必要（現在: {context.TerritoryCount}）";
                case ConditionType.CharacterIntelligence:
                    return $"知力{condition.Value}以上の武将が必要（現在: {context.CasterIntelligence}）";
                default:
                    return "条件を満たしていません";
            }
        }

        #endregion

        #region Category Conditions

        /// <summary>
        /// カテゴリ特有の条件をチェック
        /// </summary>
        private void CheckCategoryConditions(StratagemCheckResult result, StratagemData stratagem,
            Faction faction, Character caster)
        {
            switch (stratagem.Category)
            {
                case StratagemCategory.Winning:
                    // 勝戦計：優位な状況が必要
                    CheckWinningConditions(result, faction);
                    break;

                case StratagemCategory.Enemy:
                    // 敵戦計：敵の存在が必要
                    CheckEnemyConditions(result, faction);
                    break;

                case StratagemCategory.Attack:
                    // 攻戦計：軍が必要
                    CheckAttackConditions(result, faction);
                    break;

                case StratagemCategory.Chaos:
                    // 混戦計：特定条件なし
                    break;

                case StratagemCategory.Merge:
                    // 併戦計：複数勢力の存在が必要
                    CheckMergeConditions(result, faction);
                    break;

                case StratagemCategory.Defeat:
                    // 敗戦計：劣勢であること
                    CheckDefeatConditions(result, faction, stratagem.Number);
                    break;
            }
        }

        private void CheckWinningConditions(StratagemCheckResult result, Faction faction)
        {
            // 勝戦計は一定の領地数が必要
            if (faction.TerritoryIds.Count < 2)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Category",
                    IsMet = false,
                    Description = "勝戦計には最低2領地が必要です"
                });
            }
        }

        private void CheckEnemyConditions(StratagemCheckResult result, Faction faction)
        {
            // 敵戦計は敵勢力の存在が必要
            if (GameManager.Instance?.GameData == null) return;

            int enemyCount = 0;
            foreach (var f in GameManager.Instance.GameData.Factions.Values)
            {
                if (f.Id != faction.Id && f.TerritoryIds.Count > 0)
                {
                    enemyCount++;
                }
            }

            if (enemyCount == 0)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Category",
                    IsMet = false,
                    Description = "敵勢力が存在しません"
                });
            }
        }

        private void CheckAttackConditions(StratagemCheckResult result, Faction faction)
        {
            // 攻戦計は軍の存在が必要
            if (GameManager.Instance?.GameData == null) return;

            bool hasArmy = false;
            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == faction.Id && army.SoldierCount > 0)
                {
                    hasArmy = true;
                    break;
                }
            }

            if (!hasArmy)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Category",
                    IsMet = false,
                    Description = "攻戦計には軍が必要です"
                });
            }
        }

        private void CheckMergeConditions(StratagemCheckResult result, Faction faction)
        {
            // 併戦計は複数勢力が必要
            if (GameManager.Instance?.GameData == null) return;

            int aliveFactions = 0;
            foreach (var f in GameManager.Instance.GameData.Factions.Values)
            {
                if (f.TerritoryIds.Count > 0)
                {
                    aliveFactions++;
                }
            }

            if (aliveFactions < 3)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Category",
                    IsMet = false,
                    Description = "併戦計には3勢力以上が必要です"
                });
            }
        }

        private void CheckDefeatConditions(StratagemCheckResult result, Faction faction, int stratagemNumber)
        {
            // 敗戦計は劣勢時に使用
            // 第三十六計「走為上」（逃げるが勝ち）は常に使用可能

            if (stratagemNumber == 36)
            {
                return; // 走為上は常に使用可能
            }

            // 劣勢判定：領地が最も少ない、または1領地のみ
            if (GameManager.Instance?.GameData == null) return;

            int playerTerritories = faction.TerritoryIds.Count;
            bool isInferior = playerTerritories <= 1;

            if (!isInferior)
            {
                int maxEnemyTerritories = 0;
                foreach (var f in GameManager.Instance.GameData.Factions.Values)
                {
                    if (f.Id != faction.Id && f.TerritoryIds.Count > maxEnemyTerritories)
                    {
                        maxEnemyTerritories = f.TerritoryIds.Count;
                    }
                }

                isInferior = playerTerritories < maxEnemyTerritories;
            }

            if (!isInferior)
            {
                result.FailedConditions.Add(new ConditionCheckResult
                {
                    ConditionType = "Category",
                    IsMet = false,
                    Description = "敗戦計は劣勢時のみ使用可能です（領地数が敵より少ない必要があります）"
                });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 条件判定コンテキストを作成
        /// </summary>
        private ConditionContext CreateConditionContext(Faction faction, Character caster, string targetId)
        {
            var context = new ConditionContext
            {
                FactionGold = faction?.Gold ?? 0,
                StratagemPoints = faction?.StratagemPoints ?? 0,
                TerritoryCount = faction?.TerritoryIds.Count ?? 0,
                CasterIntelligence = caster?.Intelligence ?? 0,
                HasAlliance = faction?.AllianceIds.Count > 0,
                IsAtWar = false
            };

            // 軍の兵力
            if (!string.IsNullOrEmpty(targetId))
            {
                var army = GameManager.Instance?.GetArmy(targetId);
                if (army != null)
                {
                    context.ArmySoldiers = army.SoldierCount;
                }
            }

            return context;
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// 計略チェック結果
    /// </summary>
    public class StratagemCheckResult
    {
        public string StratagemId;
        public bool CanUse;
        public List<ConditionCheckResult> FailedConditions;

        /// <summary>
        /// 失敗理由をまとめて取得
        /// </summary>
        public string GetFailureReasons()
        {
            if (FailedConditions == null || FailedConditions.Count == 0)
                return null;

            var reasons = new List<string>();
            foreach (var condition in FailedConditions)
            {
                reasons.Add(condition.Description);
            }
            return string.Join("\n", reasons);
        }
    }

    /// <summary>
    /// 条件チェック結果
    /// </summary>
    public class ConditionCheckResult
    {
        public string ConditionType;
        public bool IsMet;
        public int CurrentValue;
        public int RequiredValue;
        public string Description;
    }

    /// <summary>
    /// 計略の使用可能状態
    /// </summary>
    public class StratagemAvailability
    {
        public StratagemData StratagemData;
        public bool IsAvailable;
        public StratagemCheckResult CheckResult;
    }

    #endregion
}
