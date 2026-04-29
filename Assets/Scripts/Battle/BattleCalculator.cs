using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.Battle
{
    /// <summary>
    /// 戦闘計算システム
    /// 詳細な戦闘力計算、損害計算を担当
    /// </summary>
    public class BattleCalculator : MonoBehaviour
    {
        public static BattleCalculator Instance { get; private set; }

        [Header("計算パラメータ")]
        [SerializeField] private float _baseHitRate = 0.3f;
        [SerializeField] private float _criticalMultiplier = 1.5f;
        [SerializeField] private float _flankingBonus = 0.3f;

        // 乱数シード（リプレイ用）
        private System.Random _battleRandom;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _battleRandom = new System.Random();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Round Calculation

        /// <summary>
        /// ラウンド結果を計算
        /// </summary>
        public BattleRoundResult CalculateRound(BattleState battle)
        {
            var attacker = battle.Attacker;
            var defender = battle.Defender;

            // 有効戦闘力を計算
            var attackerStats = CalculateCombatStats(attacker, defender, false);
            var defenderStats = CalculateCombatStats(defender, attacker, true);

            // 戦闘解決
            var combatResult = ResolveCombat(attackerStats, defenderStats);

            // 士気変動を計算
            var moraleChanges = CalculateMoraleChanges(combatResult, attacker, defender);

            // 特殊イベント判定
            string specialEvent = CheckSpecialEvents(battle, combatResult);

            return new BattleRoundResult
            {
                RoundNumber = battle.CurrentRound,
                AttackerPower = attackerStats.EffectivePower,
                DefenderPower = defenderStats.EffectivePower,
                AttackerCasualties = combatResult.AttackerCasualties,
                DefenderCasualties = combatResult.DefenderCasualties,
                AttackerMoraleChange = moraleChanges.AttackerChange,
                DefenderMoraleChange = moraleChanges.DefenderChange,
                SpecialEvent = specialEvent
            };
        }

        #endregion

        #region Combat Stats Calculation

        /// <summary>
        /// 戦闘統計を計算
        /// </summary>
        public CombatStats CalculateCombatStats(BattleUnit unit, BattleUnit opponent, bool isDefending)
        {
            var stats = new CombatStats
            {
                BasePower = unit.BaseCombatPower,
                SoldierCount = unit.CurrentSoldiers,
                Morale = unit.Morale
            };

            // 指揮官ボーナス
            stats.CommanderBonus = CalculateCommanderBonus(unit.CommanderId);

            // 地形ボーナス（防御側のみ）
            if (isDefending)
            {
                stats.TerrainBonus = unit.TerrainBonus;
            }

            // 計略効果ボーナス
            stats.StratagemBonus = CalculateStratagemBonus(unit);

            // 士気修正
            stats.MoraleModifier = CalculateMoraleModifier(unit.Morale);

            // 兵力修正（初期兵力との比率）
            stats.SoldierModifier = (float)unit.CurrentSoldiers / Mathf.Max(1, unit.InitialSoldiers);

            // 有効戦闘力を計算
            stats.EffectivePower = CalculateEffectivePower(stats, isDefending);

            // クリティカル率
            stats.CriticalRate = CalculateCriticalRate(unit, opponent);

            return stats;
        }

        /// <summary>
        /// 指揮官ボーナスを計算
        /// </summary>
        private int CalculateCommanderBonus(string commanderId)
        {
            if (string.IsNullOrEmpty(commanderId)) return 0;

            var commander = GameManager.Instance?.GetCharacter(commanderId);
            if (commander == null) return 0;

            // 武力の10% + 統率の5%
            return commander.Strength / 10 + commander.Leadership / 5;
        }

        /// <summary>
        /// 計略効果ボーナスを計算
        /// </summary>
        private int CalculateStratagemBonus(BattleUnit unit)
        {
            int bonus = 0;

            foreach (var effect in unit.ActiveEffects)
            {
                bonus += effect.PowerModifier;
            }

            return bonus;
        }

        /// <summary>
        /// 士気修正を計算
        /// </summary>
        private float CalculateMoraleModifier(int morale)
        {
            // 士気50を基準に±50%の修正
            return 1f + (morale - 50) * Constants.Balance.MoraleImpact / 100f;
        }

        /// <summary>
        /// 有効戦闘力を計算
        /// </summary>
        private int CalculateEffectivePower(CombatStats stats, bool isDefending)
        {
            float power = stats.BasePower;

            // 指揮官ボーナス
            power += stats.CommanderBonus;

            // 地形ボーナス
            if (isDefending)
            {
                power += stats.TerrainBonus;
            }

            // 計略効果（百分率）
            power *= (1f + stats.StratagemBonus / 100f);

            // 士気修正
            power *= stats.MoraleModifier;

            // 兵力修正
            power *= stats.SoldierModifier;

            // 防御ボーナス
            if (isDefending)
            {
                power *= Constants.Balance.DefenseBonus;
            }

            return Mathf.Max(1, Mathf.RoundToInt(power));
        }

        /// <summary>
        /// クリティカル率を計算
        /// </summary>
        private float CalculateCriticalRate(BattleUnit unit, BattleUnit opponent)
        {
            float rate = 0.05f; // 基本5%

            var commander = GameManager.Instance?.GetCharacter(unit.CommanderId);
            if (commander != null)
            {
                // 知力による補正
                rate += commander.Intelligence * 0.001f;
            }

            // 士気による補正
            rate += (unit.Morale - 50) * 0.001f;

            // 奇襲効果
            foreach (var effect in unit.ActiveEffects)
            {
                if (effect.EffectName == "奇襲")
                {
                    rate += 0.2f;
                    break;
                }
            }

            return Mathf.Clamp(rate, 0.01f, 0.5f);
        }

        #endregion

        #region Combat Resolution

        /// <summary>
        /// 戦闘を解決
        /// </summary>
        private CombatResolutionResult ResolveCombat(CombatStats attacker, CombatStats defender)
        {
            var result = new CombatResolutionResult();

            // 戦闘力比率
            float powerRatio = (float)attacker.EffectivePower / Mathf.Max(1, defender.EffectivePower);

            // 基本損害計算
            int baseAttackerDamage = CalculateBaseDamage(defender.EffectivePower, attacker.SoldierCount);
            int baseDefenderDamage = CalculateBaseDamage(attacker.EffectivePower, defender.SoldierCount);

            // 戦闘力比による損害修正
            float attackerDamageModifier = 1f / Mathf.Max(0.5f, powerRatio);
            float defenderDamageModifier = Mathf.Min(2f, powerRatio);

            // クリティカル判定
            bool attackerCritical = _battleRandom.NextDouble() < attacker.CriticalRate;
            bool defenderCritical = _battleRandom.NextDouble() < defender.CriticalRate;

            if (attackerCritical)
            {
                defenderDamageModifier *= _criticalMultiplier;
                result.AttackerCritical = true;
            }

            if (defenderCritical)
            {
                attackerDamageModifier *= _criticalMultiplier;
                result.DefenderCritical = true;
            }

            // 最終損害計算
            result.AttackerCasualties = Mathf.RoundToInt(baseAttackerDamage * attackerDamageModifier);
            result.DefenderCasualties = Mathf.RoundToInt(baseDefenderDamage * defenderDamageModifier);

            // 最低1、最大で現在兵力まで
            result.AttackerCasualties = Mathf.Clamp(result.AttackerCasualties, 1, attacker.SoldierCount);
            result.DefenderCasualties = Mathf.Clamp(result.DefenderCasualties, 1, defender.SoldierCount);

            // 勝敗判定
            result.AttackerWonRound = result.DefenderCasualties > result.AttackerCasualties;

            return result;
        }

        /// <summary>
        /// 基本損害を計算
        /// </summary>
        private int CalculateBaseDamage(int attackPower, int targetSoldiers)
        {
            // 基本損害 = 攻撃力 × 命中率 × 目標兵力の一定割合
            float damage = attackPower * _baseHitRate * targetSoldiers / 100f;

            // ランダム要素（±20%）
            float randomModifier = 0.8f + (float)_battleRandom.NextDouble() * 0.4f;
            damage *= randomModifier;

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        #endregion

        #region Morale Calculation

        /// <summary>
        /// 士気変動を計算
        /// </summary>
        private MoraleChangeResult CalculateMoraleChanges(CombatResolutionResult combat,
            BattleUnit attacker, BattleUnit defender)
        {
            var result = new MoraleChangeResult();

            // 損害比による士気変動
            float attackerLossRatio = (float)combat.AttackerCasualties / Mathf.Max(1, attacker.CurrentSoldiers);
            float defenderLossRatio = (float)combat.DefenderCasualties / Mathf.Max(1, defender.CurrentSoldiers);

            // 基本士気変動
            result.AttackerChange = combat.AttackerWonRound ? 3 : -5;
            result.DefenderChange = combat.AttackerWonRound ? -5 : 3;

            // 損害による追加減少
            result.AttackerChange -= Mathf.RoundToInt(attackerLossRatio * 10);
            result.DefenderChange -= Mathf.RoundToInt(defenderLossRatio * 10);

            // クリティカルによるボーナス/ペナルティ
            if (combat.AttackerCritical)
            {
                result.AttackerChange += 2;
                result.DefenderChange -= 3;
            }
            if (combat.DefenderCritical)
            {
                result.DefenderChange += 2;
                result.AttackerChange -= 3;
            }

            // 指揮官による士気維持
            var attackerCommander = GameManager.Instance?.GetCharacter(attacker.CommanderId);
            var defenderCommander = GameManager.Instance?.GetCharacter(defender.CommanderId);

            if (attackerCommander != null && result.AttackerChange < 0)
            {
                result.AttackerChange += attackerCommander.Leadership / 20;
            }
            if (defenderCommander != null && result.DefenderChange < 0)
            {
                result.DefenderChange += defenderCommander.Leadership / 20;
            }

            return result;
        }

        #endregion

        #region Special Events

        /// <summary>
        /// 特殊イベントをチェック
        /// </summary>
        private string CheckSpecialEvents(BattleState battle, CombatResolutionResult combat)
        {
            var events = new List<string>();

            // クリティカル
            if (combat.AttackerCritical)
            {
                events.Add("攻撃側が痛烈な一撃！");
            }
            if (combat.DefenderCritical)
            {
                events.Add("防御側が見事な反撃！");
            }

            // 士気崩壊寸前
            if (battle.Attacker.Morale <= 20 && battle.Attacker.Morale > 10)
            {
                events.Add("攻撃側の士気が危険水準！");
            }
            if (battle.Defender.Morale <= 20 && battle.Defender.Morale > 10)
            {
                events.Add("防御側の士気が危険水準！");
            }

            // 大損害
            if (combat.AttackerCasualties > battle.Attacker.CurrentSoldiers * 0.3f)
            {
                events.Add("攻撃側に大損害！");
            }
            if (combat.DefenderCasualties > battle.Defender.CurrentSoldiers * 0.3f)
            {
                events.Add("防御側に大損害！");
            }

            if (events.Count > 0)
            {
                return string.Join(" ", events);
            }

            return null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 戦闘予測を計算（事前シミュレーション）
        /// </summary>
        public BattlePrediction PredictBattle(Army attacker, Army defender, Territory territory)
        {
            var prediction = new BattlePrediction();

            // 簡易的な戦闘力計算
            int attackerPower = CalculateArmyPower(attacker);
            int defenderPower = defender != null
                ? CalculateArmyPower(defender)
                : territory.Defense * 10;

            // 防御ボーナスを加味
            defenderPower = Mathf.RoundToInt(defenderPower * Constants.Balance.DefenseBonus);

            prediction.AttackerPower = attackerPower;
            prediction.DefenderPower = defenderPower;

            // 勝率計算
            float powerRatio = (float)attackerPower / Mathf.Max(1, defenderPower);
            prediction.AttackerWinChance = Mathf.Clamp(powerRatio * 50f, 5f, 95f);

            // 予想損害
            prediction.ExpectedAttackerLosses = Mathf.RoundToInt(
                attacker.SoldierCount * Constants.Balance.WinnerLossRate / powerRatio);
            prediction.ExpectedDefenderLosses = defender != null
                ? Mathf.RoundToInt(defender.SoldierCount * Constants.Balance.LoserLossRate * powerRatio)
                : Mathf.RoundToInt(territory.Defense * 10 * Constants.Balance.LoserLossRate * powerRatio);

            return prediction;
        }

        /// <summary>
        /// 軍の戦闘力を計算
        /// </summary>
        private int CalculateArmyPower(Army army)
        {
            int power = army.SoldierCount / 100;

            var commander = GameManager.Instance?.GetCharacter(army.CommanderId);
            if (commander != null)
            {
                power += commander.Strength / 10 + commander.Leadership / 5;
            }

            float moraleModifier = 1f + (army.Morale - 50) * Constants.Balance.MoraleImpact / 100f;
            power = Mathf.RoundToInt(power * moraleModifier);

            return Mathf.Max(1, power);
        }

        /// <summary>
        /// 乱数シードを設定（リプレイ用）
        /// </summary>
        public void SetRandomSeed(int seed)
        {
            _battleRandom = new System.Random(seed);
        }

        #endregion
    }

    #region Calculator Data Classes

    /// <summary>
    /// 戦闘統計
    /// </summary>
    public class CombatStats
    {
        public int BasePower;
        public int SoldierCount;
        public int Morale;
        public int CommanderBonus;
        public int TerrainBonus;
        public int StratagemBonus;
        public float MoraleModifier;
        public float SoldierModifier;
        public int EffectivePower;
        public float CriticalRate;
    }

    /// <summary>
    /// 戦闘解決結果
    /// </summary>
    public class CombatResolutionResult
    {
        public int AttackerCasualties;
        public int DefenderCasualties;
        public bool AttackerWonRound;
        public bool AttackerCritical;
        public bool DefenderCritical;
    }

    /// <summary>
    /// 士気変動結果
    /// </summary>
    public class MoraleChangeResult
    {
        public int AttackerChange;
        public int DefenderChange;
    }

    /// <summary>
    /// 戦闘予測
    /// </summary>
    public class BattlePrediction
    {
        public int AttackerPower;
        public int DefenderPower;
        public float AttackerWinChance;
        public int ExpectedAttackerLosses;
        public int ExpectedDefenderLosses;
    }

    #endregion
}
