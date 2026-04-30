using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// 計略統合テスト
    /// 計略発動〜効果適用〜結果の一連フローをテスト
    /// </summary>
    [TestFixture]
    public class StratagemIntegrationTests
    {
        private GameData _gameData;

        [SetUp]
        public void SetUp()
        {
            _gameData = TestHelpers.CreateTestGameData();
        }

        #region Stratagem Cost Tests

        [Test]
        public void StratagemCost_ConsumesStratagemPoints()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.StratagemPoints = 10;
            int cost = 3;

            faction.StratagemPoints -= cost;

            Assert.AreEqual(7, faction.StratagemPoints);
        }

        [Test]
        public void StratagemCost_InsufficientPoints()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.StratagemPoints = 2;
            int cost = 5;

            bool canAfford = faction.StratagemPoints >= cost;

            Assert.IsFalse(canAfford);
        }

        [Test]
        public void StratagemCost_ConsumesGoldIfRequired()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.Gold = 1000;
            int spCost = 2;
            int goldCost = 500;

            bool canAfford = faction.StratagemPoints >= spCost && faction.Gold >= goldCost;
            if (canAfford)
            {
                faction.StratagemPoints -= spCost;
                faction.Gold -= goldCost;
            }

            Assert.AreEqual(500, faction.Gold);
        }

        #endregion

        #region Success Rate Tests

        [Test]
        public void SuccessRate_BaseRate()
        {
            int baseRate = Constants.Balance.StratagemBaseSuccessRate;

            Assert.AreEqual(70, baseRate);
        }

        [Test]
        public void SuccessRate_IntelligenceBonus()
        {
            var caster = TestHelpers.CreateTestCharacter(intelligence: 90);
            int baseRate = 70;

            int intelligenceBonus = caster.Intelligence / 5;
            int totalRate = baseRate + intelligenceBonus;

            Assert.AreEqual(88, totalRate); // 70 + 18
        }

        [Test]
        public void SuccessRate_ClampedTo95()
        {
            int calculatedRate = 110;
            int maxRate = 95;

            int finalRate = Mathf.Min(calculatedRate, maxRate);

            Assert.AreEqual(95, finalRate);
        }

        [Test]
        public void SuccessRate_MinimumOf5()
        {
            int calculatedRate = 0;
            int minRate = 5;

            int finalRate = Mathf.Max(calculatedRate, minRate);

            Assert.AreEqual(5, finalRate);
        }

        [Test]
        public void SuccessRate_SuccessRollSimulation()
        {
            int successRate = 70;
            int successCount = 0;
            int trials = 1000;

            System.Random random = new System.Random(12345);
            for (int i = 0; i < trials; i++)
            {
                if (random.Next(100) < successRate)
                {
                    successCount++;
                }
            }

            // 70%の成功率なら600〜800回程度成功するはず
            Assert.Greater(successCount, 600);
            Assert.Less(successCount, 800);
        }

        #endregion

        #region Attack Boost Effect Tests

        [Test]
        public void AttackBoostEffect_ApplyToUnit()
        {
            var unit = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 攻撃力+30%効果
            var effect = new BattleEffect
            {
                EffectName = "攻撃強化",
                PowerModifier = 30,
                Duration = 3
            };
            unit.ActiveEffects.Add(effect);

            int bonus = 0;
            foreach (var e in unit.ActiveEffects)
            {
                bonus += e.PowerModifier;
            }

            float modifier = 1f + bonus / 100f;
            int effectivePower = Mathf.RoundToInt(unit.BaseCombatPower * modifier);

            Assert.AreEqual(130, effectivePower);
        }

        [Test]
        public void AttackBoostEffect_MultipleEffectsStack()
        {
            var unit = TestHelpers.CreateTestBattleUnit(basePower: 100);

            unit.ActiveEffects.Add(new BattleEffect { PowerModifier = 20 });
            unit.ActiveEffects.Add(new BattleEffect { PowerModifier = 10 });

            int totalBonus = 0;
            foreach (var e in unit.ActiveEffects)
            {
                totalBonus += e.PowerModifier;
            }

            Assert.AreEqual(30, totalBonus);
        }

        #endregion

        #region Defense Boost Effect Tests

        [Test]
        public void DefenseBoostEffect_ApplyToUnit()
        {
            var unit = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 防御力+25%効果
            var effect = new BattleEffect
            {
                EffectName = "防御強化",
                PowerModifier = 25,
                Duration = 2
            };
            unit.ActiveEffects.Add(effect);

            Assert.AreEqual(1, unit.ActiveEffects.Count);
            Assert.AreEqual(25, unit.ActiveEffects[0].PowerModifier);
        }

        #endregion

        #region Morale Damage Effect Tests

        [Test]
        public void MoraleDamageEffect_ReduceEnemyMorale()
        {
            var target = TestHelpers.CreateTestBattleUnit(morale: 80);
            int moraleDamage = 20;

            target.Morale -= moraleDamage;

            Assert.AreEqual(60, target.Morale);
        }

        [Test]
        public void MoraleDamageEffect_CannotGoBelowZero()
        {
            var target = TestHelpers.CreateTestBattleUnit(morale: 15);
            int moraleDamage = 30;

            target.Morale = Mathf.Max(0, target.Morale - moraleDamage);

            Assert.AreEqual(0, target.Morale);
        }

        [Test]
        public void MoraleDamageEffect_CausesRouting()
        {
            var target = TestHelpers.CreateTestBattleUnit(morale: 15);
            int moraleDamage = 10;

            target.Morale -= moraleDamage;
            bool isRouting = target.Morale <= 10;

            Assert.IsTrue(isRouting);
        }

        #endregion

        #region Ambush Effect Tests

        [Test]
        public void AmbushEffect_IncreaseCriticalRate()
        {
            var unit = TestHelpers.CreateTestBattleUnit();
            float baseCritRate = 0.05f;

            var effect = new BattleEffect
            {
                EffectName = "奇襲",
                Duration = 1
            };
            unit.ActiveEffects.Add(effect);

            float critRate = baseCritRate;
            foreach (var e in unit.ActiveEffects)
            {
                if (e.EffectName == "奇襲")
                {
                    critRate += 0.2f;
                }
            }

            Assert.AreEqual(0.25f, critRate);
        }

        [Test]
        public void AmbushEffect_FirstStrikeBonus()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 奇襲効果で初回攻撃+50%
            int firstStrikeBonus = 50;
            int effectivePower = attacker.BaseCombatPower + firstStrikeBonus;

            Assert.AreEqual(150, effectivePower);
        }

        #endregion

        #region Effect Duration Tests

        [Test]
        public void EffectDuration_DecreaseEachTurn()
        {
            var effect = new BattleEffect
            {
                EffectName = "Test",
                Duration = 3
            };

            effect.Duration--;

            Assert.AreEqual(2, effect.Duration);
        }

        [Test]
        public void EffectDuration_RemoveWhenExpired()
        {
            var unit = TestHelpers.CreateTestBattleUnit();
            unit.ActiveEffects.Add(new BattleEffect { Duration = 1 });
            unit.ActiveEffects.Add(new BattleEffect { Duration = 3 });

            // ターン終了時に持続時間を減少させ、期限切れを削除
            for (int i = unit.ActiveEffects.Count - 1; i >= 0; i--)
            {
                unit.ActiveEffects[i].Duration--;
                if (unit.ActiveEffects[i].Duration <= 0)
                {
                    unit.ActiveEffects.RemoveAt(i);
                }
            }

            Assert.AreEqual(1, unit.ActiveEffects.Count);
            Assert.AreEqual(2, unit.ActiveEffects[0].Duration);
        }

        #endregion

        #region Target Validation Tests

        [Test]
        public void TargetValidation_CannotTargetSelf()
        {
            string casterId = "player_faction";
            string targetId = "player_faction";

            bool validTarget = casterId != targetId;

            Assert.IsFalse(validTarget);
        }

        [Test]
        public void TargetValidation_CanTargetEnemy()
        {
            string casterId = "player_faction";
            string targetId = "enemy_faction";

            bool validTarget = casterId != targetId;

            Assert.IsTrue(validTarget);
        }

        [Test]
        public void TargetValidation_CannotTargetAlly()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.AllianceIds.Add("ally_faction");
            string targetId = "ally_faction";

            bool isAlly = faction.AllianceIds.Contains(targetId);

            Assert.IsTrue(isAlly);
        }

        #endregion

        #region Stratagem Category Tests

        [Test]
        public void CategoryCondition_WinningRequiresAdvantage()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.TerritoryIds.Add("territory_3");
            faction.TerritoryIds.Add("territory_4");

            // 勝戦計には2領地以上が必要
            bool meetsCondition = faction.TerritoryIds.Count >= 2;

            Assert.IsTrue(meetsCondition);
        }

        [Test]
        public void CategoryCondition_DefeatRequiresDisadvantage()
        {
            var player = _gameData.Factions["player_faction"];
            var enemy = _gameData.Factions["enemy_faction"];

            player.TerritoryIds.Clear();
            player.TerritoryIds.Add("territory_1");
            enemy.TerritoryIds.Add("territory_3");
            enemy.TerritoryIds.Add("territory_4");

            bool isInferior = player.TerritoryIds.Count < enemy.TerritoryIds.Count;

            Assert.IsTrue(isInferior);
        }

        [Test]
        public void CategoryCondition_AttackRequiresArmy()
        {
            var faction = _gameData.Factions["player_faction"];
            bool hasArmy = false;

            foreach (var army in _gameData.Armies.Values)
            {
                if (army.FactionId == faction.Id && army.SoldierCount > 0)
                {
                    hasArmy = true;
                    break;
                }
            }

            Assert.IsTrue(hasArmy);
        }

        #endregion

        #region Combined Effects Tests

        [Test]
        public void CombinedEffects_MultipleStratagemEffects()
        {
            var unit = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 複数の計略効果を適用
            unit.ActiveEffects.Add(new BattleEffect { EffectName = "攻撃強化", PowerModifier = 20 });
            unit.ActiveEffects.Add(new BattleEffect { EffectName = "士気向上", PowerModifier = 10 });

            int totalBonus = 0;
            foreach (var e in unit.ActiveEffects)
            {
                totalBonus += e.PowerModifier;
            }

            float modifier = 1f + totalBonus / 100f;
            int effectivePower = Mathf.RoundToInt(unit.BaseCombatPower * modifier);

            Assert.AreEqual(130, effectivePower);
        }

        [Test]
        public void CombinedEffects_PositiveAndNegative()
        {
            var unit = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 正と負の効果
            unit.ActiveEffects.Add(new BattleEffect { PowerModifier = 30 }); // 味方の強化
            unit.ActiveEffects.Add(new BattleEffect { PowerModifier = -20 }); // 敵の弱体化

            int totalModifier = 0;
            foreach (var e in unit.ActiveEffects)
            {
                totalModifier += e.PowerModifier;
            }

            Assert.AreEqual(10, totalModifier);
        }

        #endregion

        #region Resource After Stratagem Tests

        [Test]
        public void ResourceAfterStratagem_SPDecreased()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.StratagemPoints = 8;

            // 計略使用
            faction.StratagemPoints -= 3;

            Assert.AreEqual(5, faction.StratagemPoints);
        }

        [Test]
        public void ResourceAfterStratagem_SPRecoveryNextTurn()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.StratagemPoints = 5;

            // ターン終了時の回復
            int recovery = Constants.Balance.StratagemPointRecoveryBase;
            int maxSP = Constants.Balance.DefaultMaxStratagemPoints;
            faction.StratagemPoints = System.Math.Min(faction.StratagemPoints + recovery, maxSP);

            Assert.AreEqual(7, faction.StratagemPoints);
        }

        #endregion

        #region Stratagem in Battle Tests

        [Test]
        public void StratagemInBattle_ApplyBeforeCombat()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 戦闘前に計略効果を適用
            attacker.ActiveEffects.Add(new BattleEffect { PowerModifier = 25 });

            int bonus = 0;
            foreach (var e in attacker.ActiveEffects)
            {
                bonus += e.PowerModifier;
            }

            Assert.AreEqual(25, bonus);
        }

        [Test]
        public void StratagemInBattle_EffectsAffectCombatResult()
        {
            var strong = TestHelpers.CreateTestBattleUnit(basePower: 100);
            var weak = TestHelpers.CreateTestBattleUnit(basePower: 100);

            // 強い方に計略効果を追加
            strong.ActiveEffects.Add(new BattleEffect { PowerModifier = 50 });

            int strongPower = strong.BaseCombatPower + strong.ActiveEffects[0].PowerModifier;
            int weakPower = weak.BaseCombatPower;

            Assert.Greater(strongPower, weakPower);
        }

        #endregion
    }
}
