using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.AI
{
    /// <summary>
    /// 勢力AI
    /// 各勢力の戦略決定を担当
    /// </summary>
    public class FactionAI
    {
        private string _factionId;
        private AIProfile _profile;
        private AIState _state;

        // 評価結果キャッシュ
        private Dictionary<string, float> _territoryScores = new Dictionary<string, float>();
        private Dictionary<string, float> _threatLevels = new Dictionary<string, float>();

        public string FactionId => _factionId;
        public AIProfile Profile => _profile;
        public AIState State => _state;

        public FactionAI(string factionId, AIProfile profile)
        {
            _factionId = factionId;
            _profile = profile ?? AIProfile.CreateDefault();
            _state = new AIState();
        }

        /// <summary>
        /// プロファイルを設定
        /// </summary>
        public void SetProfile(AIProfile profile)
        {
            _profile = profile ?? AIProfile.CreateDefault();
        }

        #region Action Decision

        /// <summary>
        /// フェーズに応じたアクションを決定
        /// </summary>
        public List<AIAction> DecideActions(TurnPhase phase)
        {
            var actions = new List<AIAction>();

            // 状況分析
            AnalyzeSituation();

            switch (phase)
            {
                case TurnPhase.Internal:
                    actions.AddRange(DecideInternalActions());
                    break;

                case TurnPhase.Diplomacy:
                    actions.AddRange(DecideDiplomacyActions());
                    break;

                case TurnPhase.Military:
                    actions.AddRange(DecideMilitaryActions());
                    break;
            }

            // 優先度でソート
            actions.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            return actions;
        }

        /// <summary>
        /// 内政フェーズのアクション
        /// </summary>
        private List<AIAction> DecideInternalActions()
        {
            var actions = new List<AIAction>();
            var faction = GameManager.Instance?.GetFaction(_factionId);

            if (faction == null) return actions;

            // 徴兵判断
            if (ShouldRecruit())
            {
                var recruitActions = DecideRecruitment();
                actions.AddRange(recruitActions);
            }

            return actions;
        }

        /// <summary>
        /// 外交フェーズのアクション
        /// </summary>
        private List<AIAction> DecideDiplomacyActions()
        {
            var actions = new List<AIAction>();

            // 計略使用判断
            if (ShouldUseStratagem())
            {
                var stratagemAction = DecideStratagemUse();
                if (stratagemAction != null)
                {
                    actions.Add(stratagemAction);
                }
            }

            // 外交判断
            if (_profile.Diplomacy > 50)
            {
                var diplomacyActions = DecideDiplomacy();
                actions.AddRange(diplomacyActions);
            }

            return actions;
        }

        /// <summary>
        /// 軍事フェーズのアクション
        /// </summary>
        private List<AIAction> DecideMilitaryActions()
        {
            var actions = new List<AIAction>();

            // 軍の移動/攻撃判断
            var militaryActions = DecideMilitaryMovement();
            actions.AddRange(militaryActions);

            return actions;
        }

        #endregion

        #region Situation Analysis

        /// <summary>
        /// 状況を分析
        /// </summary>
        private void AnalyzeSituation()
        {
            CalculateTerritoryScores();
            CalculateThreatLevels();
            UpdateAIState();
        }

        /// <summary>
        /// 領地スコアを計算
        /// </summary>
        private void CalculateTerritoryScores()
        {
            _territoryScores.Clear();

            if (GameManager.Instance?.GameData == null) return;

            foreach (var territory in GameManager.Instance.GameData.Territories.Values)
            {
                float score = 0;

                // 基本価値
                score += territory.Economy * 2;
                score += territory.Population / 1000f;
                score += territory.Defense;

                // 戦略的価値（隣接領地数）
                score += territory.AdjacentTerritoryIds.Count * 5;

                // 敵領地への攻撃価値
                if (territory.OwnerId != _factionId)
                {
                    // 隣接する自領地があれば攻撃しやすい
                    bool hasAdjacentOwn = territory.AdjacentTerritoryIds
                        .Any(id => GameManager.Instance.GetTerritory(id)?.OwnerId == _factionId);

                    if (hasAdjacentOwn)
                    {
                        score *= 1.5f;
                    }
                }

                _territoryScores[territory.Id] = score;
            }
        }

        /// <summary>
        /// 脅威レベルを計算
        /// </summary>
        private void CalculateThreatLevels()
        {
            _threatLevels.Clear();

            if (GameManager.Instance?.GameData == null) return;

            var faction = GameManager.Instance.GetFaction(_factionId);
            if (faction == null) return;

            foreach (var otherFaction in GameManager.Instance.GameData.Factions.Values)
            {
                if (otherFaction.Id == _factionId) continue;

                float threat = 0;

                // 軍事力
                int theirSoldiers = ArmyManager.Instance?.GetTotalSoldiers(otherFaction.Id) ?? 0;
                int ourSoldiers = ArmyManager.Instance?.GetTotalSoldiers(_factionId) ?? 0;

                if (ourSoldiers > 0)
                {
                    threat += (float)theirSoldiers / ourSoldiers * 50;
                }

                // 領地数
                threat += otherFaction.TerritoryIds.Count * 5;

                // 隣接状況
                bool isNeighbor = IsNeighboringFaction(otherFaction.Id);
                if (isNeighbor)
                {
                    threat *= 1.5f;
                }

                _threatLevels[otherFaction.Id] = threat;
            }
        }

        /// <summary>
        /// AI状態を更新
        /// </summary>
        private void UpdateAIState()
        {
            var faction = GameManager.Instance?.GetFaction(_factionId);
            if (faction == null) return;

            int ourSoldiers = ArmyManager.Instance?.GetTotalSoldiers(_factionId) ?? 0;
            int totalEnemySoldiers = 0;

            foreach (var f in GameManager.Instance.GameData.Factions.Values)
            {
                if (f.Id != _factionId)
                {
                    totalEnemySoldiers += ArmyManager.Instance?.GetTotalSoldiers(f.Id) ?? 0;
                }
            }

            // 全体的な優劣
            if (ourSoldiers > totalEnemySoldiers * 0.5f)
            {
                _state.OverallStrategy = StrategyType.Aggressive;
            }
            else if (ourSoldiers < totalEnemySoldiers * 0.2f)
            {
                _state.OverallStrategy = StrategyType.Defensive;
            }
            else
            {
                _state.OverallStrategy = StrategyType.Balanced;
            }

            // 主要な脅威を特定
            string maxThreat = null;
            float maxThreatLevel = 0;

            foreach (var kvp in _threatLevels)
            {
                if (kvp.Value > maxThreatLevel)
                {
                    maxThreatLevel = kvp.Value;
                    maxThreat = kvp.Key;
                }
            }

            _state.PrimaryThreat = maxThreat;
            _state.ThreatLevel = maxThreatLevel;
        }

        /// <summary>
        /// 隣接勢力かどうか
        /// </summary>
        private bool IsNeighboringFaction(string otherFactionId)
        {
            if (GameManager.Instance?.GameData == null) return false;

            foreach (var territory in GameManager.Instance.GameData.Territories.Values)
            {
                if (territory.OwnerId == _factionId)
                {
                    foreach (var adjId in territory.AdjacentTerritoryIds)
                    {
                        var adjTerritory = GameManager.Instance.GetTerritory(adjId);
                        if (adjTerritory?.OwnerId == otherFactionId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Recruitment Decision

        /// <summary>
        /// 徴兵すべきか判断
        /// </summary>
        private bool ShouldRecruit()
        {
            var faction = GameManager.Instance?.GetFaction(_factionId);
            if (faction == null) return false;

            // 経済重視度に応じて徴兵判断
            int goldThreshold = 500 - (_profile.Economy / 2);
            if (faction.Gold < goldThreshold) return false;

            // 脅威が高い場合は積極的に徴兵
            if (_state.ThreatLevel > 50) return true;

            // 攻撃性に応じて徴兵
            return UnityEngine.Random.Range(0, 100) < _profile.Aggression;
        }

        /// <summary>
        /// 徴兵を決定
        /// </summary>
        private List<AIAction> DecideRecruitment()
        {
            var actions = new List<AIAction>();
            var faction = GameManager.Instance?.GetFaction(_factionId);

            if (faction == null) return actions;

            // 各領地で徴兵可能か確認
            foreach (var territoryId in faction.TerritoryIds)
            {
                int maxRecruit = ResourceManager.Instance?.CalculateMaxRecruitment(territoryId) ?? 0;
                if (maxRecruit < 100) continue;

                int cost = ResourceManager.Instance?.CalculateRecruitmentCost(maxRecruit) ?? 0;
                if (faction.Gold < cost) continue;

                // 徴兵量を決定（経済重視度で調整）
                int recruitAmount = Mathf.RoundToInt(maxRecruit * (100 - _profile.Economy) / 100f);
                recruitAmount = Mathf.Max(100, recruitAmount);

                if (ResourceManager.Instance?.CalculateRecruitmentCost(recruitAmount) <= faction.Gold)
                {
                    actions.Add(AIAction.CreateRecruitAction(_factionId, territoryId, recruitAmount, 40));
                    break; // 1ターンに1領地のみ
                }
            }

            return actions;
        }

        #endregion

        #region Stratagem Decision

        /// <summary>
        /// 計略を使用すべきか判断
        /// </summary>
        private bool ShouldUseStratagem()
        {
            var faction = GameManager.Instance?.GetFaction(_factionId);
            if (faction == null) return false;

            // SP不足
            if (faction.StratagemPoints < 2) return false;

            // 計略使用頻度に応じて判断
            return UnityEngine.Random.Range(0, 100) < _profile.StratagemUse;
        }

        /// <summary>
        /// 計略使用を決定
        /// </summary>
        private AIAction DecideStratagemUse()
        {
            // StratagemAIに委譲
            if (StratagemAI.Instance != null)
            {
                return StratagemAI.Instance.DecideStratagem(_factionId, _state);
            }

            return null;
        }

        /// <summary>
        /// 最適な計略使用者を取得
        /// </summary>
        public string GetBestStratagemCaster()
        {
            if (GameManager.Instance?.GameData == null) return null;

            Character bestCaster = null;
            int bestIntelligence = 0;

            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId == _factionId && character.Intelligence > bestIntelligence)
                {
                    bestCaster = character;
                    bestIntelligence = character.Intelligence;
                }
            }

            return bestCaster?.Id;
        }

        #endregion

        #region Diplomacy Decision

        /// <summary>
        /// 外交を決定
        /// </summary>
        private List<AIAction> DecideDiplomacy()
        {
            var actions = new List<AIAction>();

            // TODO: 同盟提案、停戦交渉などの実装

            return actions;
        }

        #endregion

        #region Military Decision

        /// <summary>
        /// 軍事行動を決定
        /// </summary>
        private List<AIAction> DecideMilitaryMovement()
        {
            var actions = new List<AIAction>();
            var armies = ArmyManager.Instance?.GetArmiesByFaction(_factionId);

            if (armies == null || armies.Count == 0) return actions;

            foreach (var army in armies)
            {
                if (army.IsMoving) continue;

                var action = DecideArmyAction(army);
                if (action != null)
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        /// <summary>
        /// 個別の軍の行動を決定
        /// </summary>
        private AIAction DecideArmyAction(Army army)
        {
            var currentTerritory = GameManager.Instance?.GetTerritory(army.TerritoryId);
            if (currentTerritory == null) return null;

            // 攻撃可能な敵領地を探す
            var attackTargets = FindAttackTargets(currentTerritory);

            // 攻撃判断
            if (attackTargets.Count > 0 && ShouldAttack(army))
            {
                var bestTarget = SelectBestAttackTarget(attackTargets, army);
                if (bestTarget != null)
                {
                    return AIAction.CreateAttackAction(_factionId, army.Id, bestTarget.Id, 70);
                }
            }

            // 移動判断（戦略的移動）
            if (_state.OverallStrategy == StrategyType.Aggressive)
            {
                var moveTarget = FindStrategicMoveTarget(currentTerritory);
                if (moveTarget != null)
                {
                    return AIAction.CreateMoveAction(_factionId, army.Id, moveTarget.Id, 50);
                }
            }

            return null;
        }

        /// <summary>
        /// 攻撃すべきか判断
        /// </summary>
        private bool ShouldAttack(Army army)
        {
            // 士気が低い場合は攻撃しない
            if (army.Morale < 40) return false;

            // 兵力が少ない場合は慎重に
            if (army.SoldierCount < 500 && _profile.Caution > 50) return false;

            // 攻撃性と状況に応じて判断
            int attackChance = _profile.Aggression;

            if (_state.OverallStrategy == StrategyType.Aggressive)
            {
                attackChance += 20;
            }
            else if (_state.OverallStrategy == StrategyType.Defensive)
            {
                attackChance -= 30;
            }

            return UnityEngine.Random.Range(0, 100) < attackChance;
        }

        /// <summary>
        /// 攻撃可能な敵領地を探す
        /// </summary>
        private List<Territory> FindAttackTargets(Territory fromTerritory)
        {
            var targets = new List<Territory>();

            foreach (var adjId in fromTerritory.AdjacentTerritoryIds)
            {
                var adjTerritory = GameManager.Instance?.GetTerritory(adjId);
                if (adjTerritory != null && adjTerritory.OwnerId != _factionId)
                {
                    targets.Add(adjTerritory);
                }
            }

            return targets;
        }

        /// <summary>
        /// 最適な攻撃対象を選択
        /// </summary>
        private Territory SelectBestAttackTarget(List<Territory> targets, Army army)
        {
            Territory bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var target in targets)
            {
                float score = _territoryScores.GetValueOrDefault(target.Id, 0);

                // 防御力が低い方が有利
                score -= target.Defense * 2;

                // 守備軍がいない方が有利
                var defender = ArmyManager.Instance?.GetArmyAtTerritory(target.Id, target.OwnerId);
                if (defender == null)
                {
                    score += 50;
                }
                else
                {
                    // 戦力比を考慮
                    float powerRatio = (float)army.SoldierCount / defender.SoldierCount;
                    if (powerRatio < 1f)
                    {
                        score -= 50;
                    }
                    else
                    {
                        score += 20 * powerRatio;
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }

            // 慎重度に応じて閾値設定
            float threshold = _profile.Caution / 2f;
            if (bestScore < threshold) return null;

            return bestTarget;
        }

        /// <summary>
        /// 戦略的移動先を探す
        /// </summary>
        private Territory FindStrategicMoveTarget(Territory fromTerritory)
        {
            Territory bestTarget = null;
            float bestScore = 0;

            foreach (var adjId in fromTerritory.AdjacentTerritoryIds)
            {
                var adjTerritory = GameManager.Instance?.GetTerritory(adjId);
                if (adjTerritory == null) continue;

                // 自領地のみ
                if (adjTerritory.OwnerId != _factionId) continue;

                // 前線に近い領地を優先
                int enemyNeighbors = adjTerritory.AdjacentTerritoryIds
                    .Count(id => GameManager.Instance.GetTerritory(id)?.OwnerId != _factionId);

                float score = enemyNeighbors * 10;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = adjTerritory;
                }
            }

            return bestTarget;
        }

        #endregion
    }

    #region AI State

    /// <summary>
    /// AI状態
    /// </summary>
    public class AIState
    {
        public StrategyType OverallStrategy = StrategyType.Balanced;
        public string PrimaryThreat;
        public float ThreatLevel;
        public string PrimaryTarget;
    }

    /// <summary>
    /// 戦略タイプ
    /// </summary>
    public enum StrategyType
    {
        Aggressive,     // 攻撃的
        Defensive,      // 防御的
        Balanced,       // バランス
        Economic,       // 経済重視
        Diplomatic      // 外交重視
    }

    #endregion
}
