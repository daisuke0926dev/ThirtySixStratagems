using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.AI
{
    /// <summary>
    /// 戦闘AI
    /// 戦闘中の戦術的判断を担当
    /// </summary>
    public class BattleAI : MonoBehaviour
    {
        public static BattleAI Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private float _retreatThreshold = 0.3f;
        [SerializeField] private float _criticalMoraleThreshold = 20f;
        [SerializeField] private bool _logDecisions = true;

        // AIプロファイル参照用
        private Dictionary<string, AIProfile> _factionProfiles = new Dictionary<string, AIProfile>();

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

        #region Profile Management

        /// <summary>
        /// 勢力のAIプロファイルを設定
        /// </summary>
        public void SetFactionProfile(string factionId, AIProfile profile)
        {
            _factionProfiles[factionId] = profile;
        }

        /// <summary>
        /// 勢力のAIプロファイルを取得
        /// </summary>
        private AIProfile GetProfile(string factionId)
        {
            if (_factionProfiles.TryGetValue(factionId, out var profile))
            {
                return profile;
            }

            // AIManagerから取得を試みる
            var factionAI = AIManager.Instance?.GetFactionAI(factionId);
            if (factionAI != null)
            {
                return factionAI.Profile;
            }

            return AIProfile.CreateDefault();
        }

        #endregion

        #region Battle Decision

        /// <summary>
        /// 戦闘行動を決定
        /// </summary>
        public BattleDecision DecideBattleAction(BattleState battle, string factionId)
        {
            if (battle == null) return BattleDecision.Continue;

            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;
            var enemyUnit = isAttacker ? battle.Defender : battle.Attacker;
            var profile = GetProfile(factionId);

            // 状況分析
            var analysis = AnalyzeBattleSituation(ourUnit, enemyUnit);

            Log($"[{factionId}] Battle analysis: Power={analysis.PowerRatio:F2}, Morale={ourUnit.Morale}, Advantage={analysis.Advantage}");

            // 撤退判断
            if (ShouldRetreat(analysis, profile, isAttacker))
            {
                Log($"[{factionId}] Deciding to retreat");
                return BattleDecision.Retreat;
            }

            // 計略使用判断
            if (ShouldUseStratagem(analysis, profile, factionId))
            {
                var stratagemAction = StratagemAI.Instance?.DecideBattleStratagem(factionId, battle);
                if (stratagemAction != null)
                {
                    Log($"[{factionId}] Deciding to use stratagem: {stratagemAction.StratagemId}");
                    return BattleDecision.UseStratagem;
                }
            }

            // 継続判断
            Log($"[{factionId}] Deciding to continue battle");
            return BattleDecision.Continue;
        }

        /// <summary>
        /// 戦況を分析
        /// </summary>
        private BattleAnalysis AnalyzeBattleSituation(BattleUnit ourUnit, BattleUnit enemyUnit)
        {
            var analysis = new BattleAnalysis();

            // 戦力比
            analysis.PowerRatio = (float)ourUnit.CurrentSoldiers / Mathf.Max(1, enemyUnit.CurrentSoldiers);

            // 士気比較
            analysis.MoraleRatio = ourUnit.Morale / Mathf.Max(1, enemyUnit.Morale);

            // 戦闘力比較
            int ourPower = BattleCalculator.Instance?.CalculateCombatStats(ourUnit, enemyUnit, true).TotalPower ?? ourUnit.CurrentSoldiers;
            int enemyPower = BattleCalculator.Instance?.CalculateCombatStats(enemyUnit, ourUnit, true).TotalPower ?? enemyUnit.CurrentSoldiers;
            analysis.CombatPowerRatio = (float)ourPower / Mathf.Max(1, enemyPower);

            // 総合評価
            float overallRatio = (analysis.PowerRatio + analysis.MoraleRatio + analysis.CombatPowerRatio) / 3;

            if (overallRatio > 1.3f)
                analysis.Advantage = BattleAdvantage.Strong;
            else if (overallRatio > 0.9f)
                analysis.Advantage = BattleAdvantage.Even;
            else if (overallRatio > 0.5f)
                analysis.Advantage = BattleAdvantage.Weak;
            else
                analysis.Advantage = BattleAdvantage.Critical;

            return analysis;
        }

        /// <summary>
        /// 撤退すべきか判断
        /// </summary>
        private bool ShouldRetreat(BattleAnalysis analysis, AIProfile profile, bool isAttacker)
        {
            // 士気が致命的に低い
            if (analysis.MoraleRatio < 0.3f)
            {
                return true;
            }

            // 戦力が閾値以下
            if (analysis.PowerRatio < _retreatThreshold)
            {
                // 慎重なAIほど早めに撤退
                float cautionFactor = profile.Caution / 100f;
                float adjustedThreshold = _retreatThreshold + (cautionFactor * 0.2f);

                if (analysis.PowerRatio < adjustedThreshold)
                {
                    return true;
                }
            }

            // 攻撃側で劣勢なら撤退を検討
            if (isAttacker && analysis.Advantage == BattleAdvantage.Critical)
            {
                // 積極的なAIは粘る
                float aggressionChance = profile.Aggression / 100f;
                return UnityEngine.Random.value > aggressionChance * 0.5f;
            }

            return false;
        }

        /// <summary>
        /// 計略を使用すべきか判断
        /// </summary>
        private bool ShouldUseStratagem(BattleAnalysis analysis, AIProfile profile, string factionId)
        {
            // 計略使用傾向
            float stratagemTendency = profile.StratagemUse / 100f;

            // 劣勢時は計略使用を増加
            if (analysis.Advantage == BattleAdvantage.Weak || analysis.Advantage == BattleAdvantage.Critical)
            {
                stratagemTendency += 0.3f;
            }

            // SP確認
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null || faction.StratagemPoints < 2) return false;

            return UnityEngine.Random.value < stratagemTendency;
        }

        #endregion

        #region Tactical Decisions

        /// <summary>
        /// 攻撃対象を選択
        /// </summary>
        public string SelectAttackTarget(BattleState battle, string factionId)
        {
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var enemyUnit = isAttacker ? battle.Defender : battle.Attacker;

            // 基本的に敵を攻撃
            return enemyUnit.ArmyId;
        }

        /// <summary>
        /// 防御態勢を選択
        /// </summary>
        public DefensiveStance SelectDefensiveStance(BattleState battle, string factionId)
        {
            var profile = GetProfile(factionId);
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;

            // 士気が低いときは守備重視
            if (ourUnit.Morale < 40)
            {
                return DefensiveStance.Defensive;
            }

            // 慎重なAIは防御的
            if (profile.Caution > 70)
            {
                return DefensiveStance.Defensive;
            }

            // 攻撃的なAIは積極的
            if (profile.Aggression > 70)
            {
                return DefensiveStance.Aggressive;
            }

            return DefensiveStance.Normal;
        }

        /// <summary>
        /// 追撃を行うか判断
        /// </summary>
        public bool ShouldPursue(BattleResult result, string factionId)
        {
            if (result == null || !result.IsVictory(factionId)) return false;

            var profile = GetProfile(factionId);

            // 積極的なAIは追撃
            float pursuitChance = profile.Aggression / 100f;

            // 敵の残存兵力が多い場合は追撃有効
            if (result.DefenderRemainingeSoldiers > 100)
            {
                pursuitChance += 0.2f;
            }

            // 慎重なAIは追撃を控える
            pursuitChance -= (profile.Caution / 100f) * 0.3f;

            return UnityEngine.Random.value < pursuitChance;
        }

        /// <summary>
        /// 増援を要請すべきか判断
        /// </summary>
        public bool ShouldRequestReinforcement(BattleState battle, string factionId)
        {
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;
            var enemyUnit = isAttacker ? battle.Defender : battle.Attacker;

            float powerRatio = (float)ourUnit.CurrentSoldiers / Mathf.Max(1, enemyUnit.CurrentSoldiers);

            // 劣勢で防御側のとき増援検討
            if (!isAttacker && powerRatio < 0.7f)
            {
                return true;
            }

            // 士気が低い場合も増援検討
            if (ourUnit.Morale < 30)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Battle Flow Control

        /// <summary>
        /// 自動戦闘を行うか判断
        /// </summary>
        public bool ShouldAutoResolve(BattleState battle, string factionId)
        {
            var analysis = AnalyzeBattleSituation(
                battle.Attacker.FactionId == factionId ? battle.Attacker : battle.Defender,
                battle.Attacker.FactionId == factionId ? battle.Defender : battle.Attacker
            );

            // 圧倒的優勢なら自動解決
            if (analysis.Advantage == BattleAdvantage.Strong && analysis.PowerRatio > 2f)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ラウンド間の行動を決定
        /// </summary>
        public RoundIntervalAction DecideRoundIntervalAction(BattleState battle, string factionId)
        {
            var profile = GetProfile(factionId);
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;

            // 士気回復が必要か
            if (ourUnit.Morale < 30)
            {
                return RoundIntervalAction.BoostMorale;
            }

            // 計略準備
            if (profile.StratagemUse > 60 && StratagemAI.Instance != null)
            {
                return RoundIntervalAction.PrepareStratagem;
            }

            // 陣形変更（将来の実装用）
            return RoundIntervalAction.None;
        }

        #endregion

        #region Victory Conditions

        /// <summary>
        /// 勝利条件を判定
        /// </summary>
        public VictoryConditionStatus EvaluateVictoryConditions(BattleState battle, string factionId)
        {
            bool isAttacker = battle.Attacker.FactionId == factionId;
            var ourUnit = isAttacker ? battle.Attacker : battle.Defender;
            var enemyUnit = isAttacker ? battle.Defender : battle.Attacker;

            // 殲滅勝利の可能性
            if (enemyUnit.CurrentSoldiers < ourUnit.CurrentSoldiers * 0.1f)
            {
                return VictoryConditionStatus.AnnihilationPossible;
            }

            // 士気崩壊の可能性
            if (enemyUnit.Morale < _criticalMoraleThreshold)
            {
                return VictoryConditionStatus.MoraleCollapsePossible;
            }

            // 膠着状態
            if (battle.CurrentRound > 10)
            {
                return VictoryConditionStatus.Stalemate;
            }

            return VictoryConditionStatus.Ongoing;
        }

        #endregion

        #region Helper

        private void Log(string message)
        {
            if (_logDecisions)
            {
                Debug.Log($"[BattleAI] {message}");
            }
        }

        #endregion
    }

    #region Battle AI Enums

    /// <summary>
    /// 戦闘決定
    /// </summary>
    public enum BattleDecision
    {
        Continue,
        UseStratagem,
        Retreat,
        Surrender
    }

    /// <summary>
    /// 戦況優位
    /// </summary>
    public enum BattleAdvantage
    {
        Strong,     // 優勢
        Even,       // 互角
        Weak,       // 劣勢
        Critical    // 危機的
    }

    /// <summary>
    /// 防御態勢
    /// </summary>
    public enum DefensiveStance
    {
        Aggressive,
        Normal,
        Defensive
    }

    /// <summary>
    /// ラウンド間行動
    /// </summary>
    public enum RoundIntervalAction
    {
        None,
        BoostMorale,
        PrepareStratagem,
        ChangeFormation,
        CallReinforcement
    }

    /// <summary>
    /// 勝利条件状態
    /// </summary>
    public enum VictoryConditionStatus
    {
        Ongoing,
        AnnihilationPossible,
        MoraleCollapsePossible,
        Stalemate
    }

    #endregion

    #region Battle Analysis

    /// <summary>
    /// 戦況分析結果
    /// </summary>
    public class BattleAnalysis
    {
        public float PowerRatio;
        public float MoraleRatio;
        public float CombatPowerRatio;
        public BattleAdvantage Advantage;
    }

    #endregion
}
