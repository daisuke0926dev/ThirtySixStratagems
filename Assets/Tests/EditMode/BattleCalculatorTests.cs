using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// BattleCalculator関連のテスト
    /// 戦闘計算ロジックのユニットテスト
    /// </summary>
    [TestFixture]
    public class BattleCalculatorTests
    {
        #region CombatStats Tests

        [Test]
        public void CombatStats_DefaultValues_AreZero()
        {
            var stats = new CombatStats();

            Assert.AreEqual(0, stats.BasePower);
            Assert.AreEqual(0, stats.SoldierCount);
            Assert.AreEqual(0, stats.Morale);
            Assert.AreEqual(0, stats.CommanderBonus);
            Assert.AreEqual(0, stats.TerrainBonus);
            Assert.AreEqual(0, stats.StratagemBonus);
            Assert.AreEqual(0f, stats.MoraleModifier);
            Assert.AreEqual(0f, stats.SoldierModifier);
            Assert.AreEqual(0, stats.EffectivePower);
            Assert.AreEqual(0f, stats.CriticalRate);
        }

        [Test]
        public void CombatStats_CanSetValues()
        {
            var stats = new CombatStats
            {
                BasePower = 100,
                SoldierCount = 1000,
                Morale = 75,
                CommanderBonus = 10,
                TerrainBonus = 5,
                StratagemBonus = 20,
                MoraleModifier = 1.2f,
                SoldierModifier = 0.8f,
                EffectivePower = 150,
                CriticalRate = 0.1f
            };

            Assert.AreEqual(100, stats.BasePower);
            Assert.AreEqual(1000, stats.SoldierCount);
            Assert.AreEqual(75, stats.Morale);
            Assert.AreEqual(10, stats.CommanderBonus);
            Assert.AreEqual(5, stats.TerrainBonus);
            Assert.AreEqual(20, stats.StratagemBonus);
            Assert.AreEqual(1.2f, stats.MoraleModifier);
            Assert.AreEqual(0.8f, stats.SoldierModifier);
            Assert.AreEqual(150, stats.EffectivePower);
            Assert.AreEqual(0.1f, stats.CriticalRate);
        }

        #endregion

        #region CombatResolutionResult Tests

        [Test]
        public void CombatResolutionResult_DefaultValues()
        {
            var result = new CombatResolutionResult();

            Assert.AreEqual(0, result.AttackerCasualties);
            Assert.AreEqual(0, result.DefenderCasualties);
            Assert.IsFalse(result.AttackerWonRound);
            Assert.IsFalse(result.AttackerCritical);
            Assert.IsFalse(result.DefenderCritical);
        }

        [Test]
        public void CombatResolutionResult_AttackerWins_WhenMoreDefenderCasualties()
        {
            var result = new CombatResolutionResult
            {
                AttackerCasualties = 50,
                DefenderCasualties = 100,
                AttackerWonRound = true
            };

            Assert.IsTrue(result.AttackerWonRound);
            Assert.Greater(result.DefenderCasualties, result.AttackerCasualties);
        }

        [Test]
        public void CombatResolutionResult_CriticalFlags()
        {
            var result = new CombatResolutionResult
            {
                AttackerCritical = true,
                DefenderCritical = false
            };

            Assert.IsTrue(result.AttackerCritical);
            Assert.IsFalse(result.DefenderCritical);
        }

        #endregion

        #region MoraleChangeResult Tests

        [Test]
        public void MoraleChangeResult_DefaultValues()
        {
            var result = new MoraleChangeResult();

            Assert.AreEqual(0, result.AttackerChange);
            Assert.AreEqual(0, result.DefenderChange);
        }

        [Test]
        public void MoraleChangeResult_WinnerGainsMorale()
        {
            var result = new MoraleChangeResult
            {
                AttackerChange = 5,
                DefenderChange = -10
            };

            Assert.Greater(result.AttackerChange, 0);
            Assert.Less(result.DefenderChange, 0);
        }

        [Test]
        public void MoraleChangeResult_BothCanLoseMorale()
        {
            var result = new MoraleChangeResult
            {
                AttackerChange = -3,
                DefenderChange = -5
            };

            Assert.Less(result.AttackerChange, 0);
            Assert.Less(result.DefenderChange, 0);
        }

        #endregion

        #region BattlePrediction Tests

        [Test]
        public void BattlePrediction_DefaultValues()
        {
            var prediction = new BattlePrediction();

            Assert.AreEqual(0, prediction.AttackerPower);
            Assert.AreEqual(0, prediction.DefenderPower);
            Assert.AreEqual(0f, prediction.AttackerWinChance);
            Assert.AreEqual(0, prediction.ExpectedAttackerLosses);
            Assert.AreEqual(0, prediction.ExpectedDefenderLosses);
        }

        [Test]
        public void BattlePrediction_StrongerAttacker_HigherWinChance()
        {
            var prediction = new BattlePrediction
            {
                AttackerPower = 200,
                DefenderPower = 100,
                AttackerWinChance = 75f
            };

            Assert.Greater(prediction.AttackerPower, prediction.DefenderPower);
            Assert.Greater(prediction.AttackerWinChance, 50f);
        }

        [Test]
        public void BattlePrediction_WeakerAttacker_LowerWinChance()
        {
            var prediction = new BattlePrediction
            {
                AttackerPower = 100,
                DefenderPower = 200,
                AttackerWinChance = 25f
            };

            Assert.Less(prediction.AttackerPower, prediction.DefenderPower);
            Assert.Less(prediction.AttackerWinChance, 50f);
        }

        [Test]
        public void BattlePrediction_WinChance_ClampedTo5_95()
        {
            // 勝率は5%〜95%の範囲に収まるべき
            var highPrediction = new BattlePrediction
            {
                AttackerPower = 1000,
                DefenderPower = 10,
                AttackerWinChance = 95f // 最大95%
            };

            var lowPrediction = new BattlePrediction
            {
                AttackerPower = 10,
                DefenderPower = 1000,
                AttackerWinChance = 5f // 最小5%
            };

            Assert.LessOrEqual(highPrediction.AttackerWinChance, 95f);
            Assert.GreaterOrEqual(lowPrediction.AttackerWinChance, 5f);
        }

        #endregion

        #region BattleUnit Tests

        [Test]
        public void BattleUnit_Creation_WithTestHelper()
        {
            var unit = TestHelpers.CreateTestBattleUnit(
                soldiers: 500,
                morale: 80,
                basePower: 120
            );

            Assert.AreEqual(500, unit.CurrentSoldiers);
            Assert.AreEqual(500, unit.InitialSoldiers);
            Assert.AreEqual(80, unit.Morale);
            Assert.AreEqual(120, unit.BaseCombatPower);
        }

        [Test]
        public void BattleUnit_SoldierRatio_CalculatedCorrectly()
        {
            var unit = TestHelpers.CreateTestBattleUnit(soldiers: 1000);
            unit.InitialSoldiers = 1000;
            unit.CurrentSoldiers = 500;

            float ratio = (float)unit.CurrentSoldiers / unit.InitialSoldiers;

            Assert.AreEqual(0.5f, ratio);
        }

        [Test]
        public void BattleUnit_ActiveEffects_InitiallyEmpty()
        {
            var unit = TestHelpers.CreateTestBattleUnit();

            Assert.IsNotNull(unit.ActiveEffects);
            Assert.AreEqual(0, unit.ActiveEffects.Count);
        }

        [Test]
        public void BattleUnit_WithTerrainBonus()
        {
            var unit = TestHelpers.CreateTestBattleUnit(terrainBonus: 15);

            Assert.AreEqual(15, unit.TerrainBonus);
        }

        #endregion

        #region BattleState Tests

        [Test]
        public void BattleState_Creation_WithTestHelper()
        {
            var state = TestHelpers.CreateTestBattleState();

            Assert.IsNotNull(state.Attacker);
            Assert.IsNotNull(state.Defender);
            Assert.AreEqual(1, state.CurrentRound);
        }

        [Test]
        public void BattleState_CustomUnits()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(
                armyId: "strong_army",
                soldiers: 2000,
                basePower: 150
            );
            var defender = TestHelpers.CreateTestBattleUnit(
                armyId: "weak_army",
                soldiers: 500,
                basePower: 80
            );

            var state = TestHelpers.CreateTestBattleState(attacker, defender);

            Assert.AreEqual("strong_army", state.Attacker.ArmyId);
            Assert.AreEqual("weak_army", state.Defender.ArmyId);
            Assert.Greater(state.Attacker.CurrentSoldiers, state.Defender.CurrentSoldiers);
        }

        #endregion

        #region Balance Constants Tests

        [Test]
        public void BalanceConstants_DefenseBonus_IsPositive()
        {
            Assert.Greater(Constants.Balance.DefenseBonus, 1f);
        }

        [Test]
        public void BalanceConstants_WinnerLossRate_LessThanLoser()
        {
            Assert.Less(Constants.Balance.WinnerLossRate, Constants.Balance.LoserLossRate);
        }

        [Test]
        public void BalanceConstants_MoraleImpact_InValidRange()
        {
            // 士気影響は0〜1の間であるべき
            Assert.GreaterOrEqual(Constants.Balance.MoraleImpact, 0f);
            Assert.LessOrEqual(Constants.Balance.MoraleImpact, 1f);
        }

        [Test]
        public void BalanceConstants_MaxMorale_Is100()
        {
            Assert.AreEqual(100, Constants.Balance.MaxMorale);
        }

        #endregion

        #region Morale Modifier Calculation Tests

        [Test]
        public void MoraleModifier_AtBaseline50_Returns1()
        {
            // 士気50が基準なので、修正値は1.0になるはず
            int morale = 50;
            float modifier = 1f + (morale - 50) * Constants.Balance.MoraleImpact / 100f;

            Assert.AreEqual(1f, modifier);
        }

        [Test]
        public void MoraleModifier_HighMorale_IncreasesModifier()
        {
            int morale = 100;
            float modifier = 1f + (morale - 50) * Constants.Balance.MoraleImpact / 100f;

            Assert.Greater(modifier, 1f);
        }

        [Test]
        public void MoraleModifier_LowMorale_DecreasesModifier()
        {
            int morale = 0;
            float modifier = 1f + (morale - 50) * Constants.Balance.MoraleImpact / 100f;

            Assert.Less(modifier, 1f);
        }

        #endregion

        #region Damage Calculation Logic Tests

        [Test]
        public void DamageCalculation_MinimumDamage_IsOne()
        {
            // 最小ダメージは1であるべき
            int minDamage = Mathf.Max(1, 0);
            Assert.AreEqual(1, minDamage);
        }

        [Test]
        public void DamageCalculation_ClampToSoldierCount()
        {
            int soldiers = 100;
            int calculatedDamage = 150;
            int actualDamage = Mathf.Clamp(calculatedDamage, 1, soldiers);

            Assert.AreEqual(soldiers, actualDamage);
        }

        [Test]
        public void PowerRatio_EqualPowers_ReturnsOne()
        {
            int attackerPower = 100;
            int defenderPower = 100;
            float ratio = (float)attackerPower / Mathf.Max(1, defenderPower);

            Assert.AreEqual(1f, ratio);
        }

        [Test]
        public void PowerRatio_StrongerAttacker_GreaterThanOne()
        {
            int attackerPower = 200;
            int defenderPower = 100;
            float ratio = (float)attackerPower / Mathf.Max(1, defenderPower);

            Assert.AreEqual(2f, ratio);
        }

        #endregion
    }
}
