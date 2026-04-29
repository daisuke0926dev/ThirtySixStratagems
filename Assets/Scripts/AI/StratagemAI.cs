using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;
using ThirtySixStratagems.Stratagem;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.AI
{
    /// <summary>
    /// 計略AI
    /// AIによる計略選択と使用判断を担当
    /// </summary>
    public class StratagemAI : MonoBehaviour
    {
        public static StratagemAI Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private float _minSuccessRateThreshold = 40f;
        [SerializeField] private bool _logDecisions = true;

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

        #region Main Decision

        /// <summary>
        /// 計略使用を決定
        /// </summary>
        public AIAction DecideStratagem(string factionId, AIState state)
        {
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null) return null;

            // 使用可能な計略を取得
            var availableStratagems = GetAvailableStratagems(factionId);
            if (availableStratagems.Count == 0) return null;

            // 状況に応じて最適な計略を選択
            var rankedStratagems = RankStratagems(availableStratagems, faction, state);

            foreach (var ranked in rankedStratagems)
            {
                var target = FindBestTarget(ranked.Stratagem, factionId, state);
                if (target != null)
                {
                    Log($"[{factionId}] Selected stratagem: {ranked.Stratagem.NameJP} (Score: {ranked.Score:F1})");
                    return AIAction.CreateStratagemAction(factionId, ranked.Stratagem.StratagemId, target, (int)ranked.Score);
                }
            }

            return null;
        }

        /// <summary>
        /// 戦闘中の計略使用を決定
        /// </summary>
        public AIAction DecideBattleStratagem(string factionId, BattleState battle)
        {
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null || faction.StratagemPoints < 2) return null;

            // 戦闘向きの計略を取得
            var battleStratagems = GetBattleStratagems(factionId);
            if (battleStratagems.Count == 0) return null;

            // 戦況に応じて選択
            foreach (var stratagem in battleStratagems)
            {
                if (ShouldUseBattleStratagem(stratagem, battle, factionId))
                {
                    string targetId = GetBattleStratagemTarget(stratagem, battle, factionId);
                    if (!string.IsNullOrEmpty(targetId))
                    {
                        Log($"[{factionId}] Using battle stratagem: {stratagem.NameJP}");
                        return AIAction.CreateStratagemAction(factionId, stratagem.StratagemId, targetId, 80);
                    }
                }
            }

            return null;
        }

        #endregion

        #region Stratagem Selection

        /// <summary>
        /// 使用可能な計略を取得
        /// </summary>
        private List<StratagemData> GetAvailableStratagems(string factionId)
        {
            var available = new List<StratagemData>();

            if (StratagemManager.Instance?.Database == null) return available;

            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null) return available;

            foreach (var stratagem in StratagemManager.Instance.Database.AllStratagems)
            {
                // コストチェック
                if (faction.StratagemPoints < stratagem.CostSP) continue;
                if (faction.Gold < stratagem.CostGold) continue;

                available.Add(stratagem);
            }

            return available;
        }

        /// <summary>
        /// 戦闘向きの計略を取得
        /// </summary>
        private List<StratagemData> GetBattleStratagems(string factionId)
        {
            var available = GetAvailableStratagems(factionId);

            // 戦闘に有効な効果タイプでフィルター
            var battleEffects = new HashSet<StratagemEffectType>
            {
                StratagemEffectType.AttackBoost,
                StratagemEffectType.DefenseBoost,
                StratagemEffectType.Ambush,
                StratagemEffectType.MoraleReduce,
                StratagemEffectType.ForceRetreat
            };

            return available.Where(s => battleEffects.Contains(s.PrimaryEffect)).ToList();
        }

        /// <summary>
        /// 計略をランク付け
        /// </summary>
        private List<RankedStratagem> RankStratagems(List<StratagemData> stratagems, Faction faction, AIState state)
        {
            var ranked = new List<RankedStratagem>();

            foreach (var stratagem in stratagems)
            {
                float score = CalculateStratagemScore(stratagem, faction, state);
                if (score > 0)
                {
                    ranked.Add(new RankedStratagem { Stratagem = stratagem, Score = score });
                }
            }

            // スコアで降順ソート
            ranked.Sort((a, b) => b.Score.CompareTo(a.Score));

            return ranked;
        }

        /// <summary>
        /// 計略スコアを計算
        /// </summary>
        private float CalculateStratagemScore(StratagemData stratagem, Faction faction, AIState state)
        {
            float score = 0;

            // 基本成功率
            if (stratagem.BaseSuccessRate < _minSuccessRateThreshold) return 0;
            score += stratagem.BaseSuccessRate / 2;

            // 状況に応じたボーナス
            switch (state.OverallStrategy)
            {
                case StrategyType.Aggressive:
                    score += GetAggressiveBonus(stratagem);
                    break;

                case StrategyType.Defensive:
                    score += GetDefensiveBonus(stratagem);
                    break;

                case StrategyType.Balanced:
                    score += GetBalancedBonus(stratagem);
                    break;
            }

            // カテゴリボーナス
            score += GetCategoryBonus(stratagem.Category, state);

            // コスト効率
            float costEfficiency = 100f / (stratagem.CostSP * 10 + stratagem.CostGold);
            score *= costEfficiency;

            return score;
        }

        private float GetAggressiveBonus(StratagemData stratagem)
        {
            switch (stratagem.PrimaryEffect)
            {
                case StratagemEffectType.AttackBoost:
                case StratagemEffectType.Ambush:
                case StratagemEffectType.ForceRetreat:
                case StratagemEffectType.MoraleReduce:
                    return 30;

                case StratagemEffectType.ResourcePlunder:
                case StratagemEffectType.TerritoryControl:
                    return 20;

                default:
                    return 0;
            }
        }

        private float GetDefensiveBonus(StratagemData stratagem)
        {
            switch (stratagem.PrimaryEffect)
            {
                case StratagemEffectType.DefenseBoost:
                case StratagemEffectType.Escape:
                case StratagemEffectType.StealthMovement:
                    return 30;

                case StratagemEffectType.Disinformation:
                case StratagemEffectType.Reconnaissance:
                    return 20;

                default:
                    return 0;
            }
        }

        private float GetBalancedBonus(StratagemData stratagem)
        {
            switch (stratagem.PrimaryEffect)
            {
                case StratagemEffectType.Diplomacy:
                case StratagemEffectType.FactionConflict:
                case StratagemEffectType.LoyaltyReduce:
                    return 20;

                case StratagemEffectType.Reconnaissance:
                    return 15;

                default:
                    return 10;
            }
        }

        private float GetCategoryBonus(StratagemCategory category, AIState state)
        {
            // 敗戦計は劣勢時のみ高評価
            if (category == StratagemCategory.Defeat)
            {
                if (state.OverallStrategy == StrategyType.Defensive && state.ThreatLevel > 70)
                {
                    return 40;
                }
                return -20;
            }

            // 攻戦計は攻撃的戦略時に高評価
            if (category == StratagemCategory.Attack && state.OverallStrategy == StrategyType.Aggressive)
            {
                return 20;
            }

            return 0;
        }

        #endregion

        #region Target Selection

        /// <summary>
        /// 最適な対象を選択
        /// </summary>
        private string FindBestTarget(StratagemData stratagem, string factionId, AIState state)
        {
            switch (stratagem.TargetType)
            {
                case StratagemTarget.Self:
                    return factionId;

                case StratagemTarget.EnemyFaction:
                    return FindBestEnemyFaction(factionId, state);

                case StratagemTarget.EnemyArmy:
                    return FindBestEnemyArmy(factionId);

                case StratagemTarget.EnemyCharacter:
                    return FindBestEnemyCharacter(factionId, stratagem.PrimaryEffect);

                case StratagemTarget.EnemyTerritory:
                    return FindBestEnemyTerritory(factionId);

                case StratagemTarget.Any:
                    return FindBestEnemyFaction(factionId, state);

                default:
                    return null;
            }
        }

        private string FindBestEnemyFaction(string factionId, AIState state)
        {
            // 主要な脅威を優先
            if (!string.IsNullOrEmpty(state.PrimaryThreat))
            {
                return state.PrimaryThreat;
            }

            // 最も強い敵勢力を選択
            if (GameManager.Instance?.GameData == null) return null;

            string bestTarget = null;
            int maxTerritories = 0;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.Id == factionId) continue;
                if (faction.TerritoryIds.Count > maxTerritories)
                {
                    maxTerritories = faction.TerritoryIds.Count;
                    bestTarget = faction.Id;
                }
            }

            return bestTarget;
        }

        private string FindBestEnemyArmy(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return null;

            string bestTarget = null;
            int maxSoldiers = 0;

            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == factionId) continue;
                if (army.SoldierCount > maxSoldiers)
                {
                    maxSoldiers = army.SoldierCount;
                    bestTarget = army.Id;
                }
            }

            return bestTarget;
        }

        private string FindBestEnemyCharacter(string factionId, StratagemEffectType effect)
        {
            if (GameManager.Instance?.GameData == null) return null;

            string bestTarget = null;
            int bestScore = 0;

            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId == factionId) continue;

                int score = 0;

                // 効果タイプに応じた評価
                switch (effect)
                {
                    case StratagemEffectType.LoyaltyReduce:
                        // 忠誠度が低い武将を優先
                        score = 100 - character.Loyalty;
                        // 能力が高い武将を優先
                        score += (character.Strength + character.Intelligence + character.Leadership) / 3;
                        break;

                    case StratagemEffectType.CharacterCapture:
                        // 能力が高い武将を優先
                        score = (character.Strength + character.Intelligence + character.Leadership) / 2;
                        break;

                    default:
                        score = character.Intelligence;
                        break;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = character.Id;
                }
            }

            return bestTarget;
        }

        private string FindBestEnemyTerritory(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return null;

            string bestTarget = null;
            float bestScore = 0;

            var faction = GameManager.Instance.GetFaction(factionId);
            if (faction == null) return null;

            foreach (var territory in GameManager.Instance.GameData.Territories.Values)
            {
                if (territory.OwnerId == factionId) continue;

                // 隣接する自領地があるか
                bool isAdjacent = territory.AdjacentTerritoryIds
                    .Any(id => faction.TerritoryIds.Contains(id));

                if (!isAdjacent) continue;

                float score = territory.Economy + territory.Population / 1000f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = territory.Id;
                }
            }

            return bestTarget;
        }

        #endregion

        #region Battle Stratagem

        /// <summary>
        /// 戦闘中に計略を使用すべきか判断
        /// </summary>
        private bool ShouldUseBattleStratagem(StratagemData stratagem, BattleState battle, string factionId)
        {
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;
            var enemyUnit = isAttacker ? battle.Defender : battle.Attacker;

            // 劣勢時は計略使用を検討
            float powerRatio = (float)ourUnit.CurrentSoldiers / Mathf.Max(1, enemyUnit.CurrentSoldiers);

            switch (stratagem.PrimaryEffect)
            {
                case StratagemEffectType.AttackBoost:
                case StratagemEffectType.Ambush:
                    // 攻撃側で優勢〜互角時
                    return isAttacker && powerRatio >= 0.8f;

                case StratagemEffectType.DefenseBoost:
                    // 防御側で劣勢時
                    return !isAttacker && powerRatio < 1f;

                case StratagemEffectType.MoraleReduce:
                    // 敵の士気が高い時
                    return enemyUnit.Morale > 60;

                case StratagemEffectType.ForceRetreat:
                    // 劣勢時の起死回生
                    return powerRatio < 0.5f;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 戦闘計略の対象を取得
        /// </summary>
        private string GetBattleStratagemTarget(StratagemData stratagem, BattleState battle, string factionId)
        {
            bool isAttacker = battle.Attacker.FactionId == factionId;

            switch (stratagem.TargetType)
            {
                case StratagemTarget.Self:
                    return factionId;

                case StratagemTarget.EnemyArmy:
                    return isAttacker ? battle.Defender.ArmyId : battle.Attacker.ArmyId;

                case StratagemTarget.EnemyFaction:
                    return isAttacker ? battle.Defender.FactionId : battle.Attacker.FactionId;

                default:
                    return null;
            }
        }

        #endregion

        #region Helper

        private void Log(string message)
        {
            if (_logDecisions)
            {
                Debug.Log($"[StratagemAI] {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// ランク付けされた計略
    /// </summary>
    internal class RankedStratagem
    {
        public StratagemData Stratagem;
        public float Score;
    }
}
