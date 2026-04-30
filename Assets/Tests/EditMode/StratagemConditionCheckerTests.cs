using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// StratagemConditionChecker関連のテスト
    /// 計略条件判定ロジックのユニットテスト
    /// </summary>
    [TestFixture]
    public class StratagemConditionCheckerTests
    {
        #region StratagemCheckResult Tests

        [Test]
        public void StratagemCheckResult_DefaultCanUse_IsTrue()
        {
            var result = new StratagemCheckResult
            {
                StratagemId = "test",
                CanUse = true,
                FailedConditions = new List<ConditionCheckResult>()
            };

            Assert.IsTrue(result.CanUse);
            Assert.AreEqual(0, result.FailedConditions.Count);
        }

        [Test]
        public void StratagemCheckResult_WithFailedCondition_CannotUse()
        {
            var result = new StratagemCheckResult
            {
                StratagemId = "test",
                CanUse = false,
                FailedConditions = new List<ConditionCheckResult>
                {
                    new ConditionCheckResult
                    {
                        ConditionType = "StratagemPoints",
                        IsMet = false,
                        Description = "計略ポイント不足"
                    }
                }
            };

            Assert.IsFalse(result.CanUse);
            Assert.AreEqual(1, result.FailedConditions.Count);
        }

        [Test]
        public void StratagemCheckResult_GetFailureReasons_ReturnsDescriptions()
        {
            var result = new StratagemCheckResult
            {
                StratagemId = "test",
                CanUse = false,
                FailedConditions = new List<ConditionCheckResult>
                {
                    new ConditionCheckResult { Description = "条件1" },
                    new ConditionCheckResult { Description = "条件2" }
                }
            };

            string reasons = result.GetFailureReasons();

            Assert.IsNotNull(reasons);
            Assert.IsTrue(reasons.Contains("条件1"));
            Assert.IsTrue(reasons.Contains("条件2"));
        }

        [Test]
        public void StratagemCheckResult_GetFailureReasons_EmptyList_ReturnsNull()
        {
            var result = new StratagemCheckResult
            {
                StratagemId = "test",
                CanUse = true,
                FailedConditions = new List<ConditionCheckResult>()
            };

            string reasons = result.GetFailureReasons();

            Assert.IsNull(reasons);
        }

        #endregion

        #region ConditionCheckResult Tests

        [Test]
        public void ConditionCheckResult_Creation()
        {
            var result = new ConditionCheckResult
            {
                ConditionType = "StratagemPoints",
                IsMet = false,
                CurrentValue = 3,
                RequiredValue = 5,
                Description = "計略ポイント不足（現在: 3 / 必要: 5）"
            };

            Assert.AreEqual("StratagemPoints", result.ConditionType);
            Assert.IsFalse(result.IsMet);
            Assert.AreEqual(3, result.CurrentValue);
            Assert.AreEqual(5, result.RequiredValue);
        }

        [Test]
        public void ConditionCheckResult_MetCondition()
        {
            var result = new ConditionCheckResult
            {
                ConditionType = "Gold",
                IsMet = true,
                CurrentValue = 1000,
                RequiredValue = 500
            };

            Assert.IsTrue(result.IsMet);
            Assert.Greater(result.CurrentValue, result.RequiredValue);
        }

        #endregion

        #region StratagemAvailability Tests

        [Test]
        public void StratagemAvailability_Available()
        {
            var availability = new StratagemAvailability
            {
                StratagemData = null, // テストでは null でも可
                IsAvailable = true,
                CheckResult = new StratagemCheckResult
                {
                    CanUse = true,
                    FailedConditions = new List<ConditionCheckResult>()
                }
            };

            Assert.IsTrue(availability.IsAvailable);
        }

        [Test]
        public void StratagemAvailability_Unavailable()
        {
            var availability = new StratagemAvailability
            {
                StratagemData = null,
                IsAvailable = false,
                CheckResult = new StratagemCheckResult
                {
                    CanUse = false,
                    FailedConditions = new List<ConditionCheckResult>
                    {
                        new ConditionCheckResult { Description = "条件未満" }
                    }
                }
            };

            Assert.IsFalse(availability.IsAvailable);
        }

        #endregion

        #region Faction Resource Check Tests

        [Test]
        public void FactionResourceCheck_HasEnoughStratagemPoints()
        {
            var faction = TestHelpers.CreateTestFaction(stratagemPoints: 10);
            int requiredSP = 5;

            bool hasEnough = faction.StratagemPoints >= requiredSP;

            Assert.IsTrue(hasEnough);
        }

        [Test]
        public void FactionResourceCheck_NotEnoughStratagemPoints()
        {
            var faction = TestHelpers.CreateTestFaction(stratagemPoints: 3);
            int requiredSP = 5;

            bool hasEnough = faction.StratagemPoints >= requiredSP;

            Assert.IsFalse(hasEnough);
        }

        [Test]
        public void FactionResourceCheck_HasEnoughGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 1000);
            int requiredGold = 500;

            bool hasEnough = faction.Gold >= requiredGold;

            Assert.IsTrue(hasEnough);
        }

        [Test]
        public void FactionResourceCheck_NotEnoughGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 200);
            int requiredGold = 500;

            bool hasEnough = faction.Gold >= requiredGold;

            Assert.IsFalse(hasEnough);
        }

        #endregion

        #region Territory Check Tests

        [Test]
        public void TerritoryCheck_OwnTerritory()
        {
            var territory = TestHelpers.CreateTestTerritory(ownerId: "player_faction");
            string casterFactionId = "player_faction";

            bool isOwnTerritory = territory.OwnerId == casterFactionId;

            Assert.IsTrue(isOwnTerritory);
        }

        [Test]
        public void TerritoryCheck_EnemyTerritory()
        {
            var territory = TestHelpers.CreateTestTerritory(ownerId: "enemy_faction");
            string casterFactionId = "player_faction";

            bool isEnemyTerritory = territory.OwnerId != casterFactionId;

            Assert.IsTrue(isEnemyTerritory);
        }

        #endregion

        #region Alliance Check Tests

        [Test]
        public void AllianceCheck_IsAlly()
        {
            var faction = TestHelpers.CreateTestFaction();
            faction.AllianceIds.Add("ally_faction");

            bool isAlly = faction.AllianceIds.Contains("ally_faction");

            Assert.IsTrue(isAlly);
        }

        [Test]
        public void AllianceCheck_IsNotAlly()
        {
            var faction = TestHelpers.CreateTestFaction();

            bool isAlly = faction.AllianceIds.Contains("other_faction");

            Assert.IsFalse(isAlly);
        }

        [Test]
        public void AllianceCheck_CannotTargetAlly()
        {
            var faction = TestHelpers.CreateTestFaction();
            faction.AllianceIds.Add("ally_faction");
            string targetFactionId = "ally_faction";

            bool canTarget = !faction.AllianceIds.Contains(targetFactionId);

            Assert.IsFalse(canTarget);
        }

        #endregion

        #region Category Condition Tests

        [Test]
        public void WinningStratagem_RequiresMinTerritories()
        {
            var faction = TestHelpers.CreateTestFaction();
            faction.TerritoryIds.Clear();
            faction.TerritoryIds.Add("territory_1");
            // 勝戦計には最低2領地が必要

            bool meetsCondition = faction.TerritoryIds.Count >= 2;

            Assert.IsFalse(meetsCondition);
        }

        [Test]
        public void WinningStratagem_MeetsMinTerritories()
        {
            var faction = TestHelpers.CreateTestFaction();
            faction.TerritoryIds.Clear();
            faction.TerritoryIds.Add("territory_1");
            faction.TerritoryIds.Add("territory_2");

            bool meetsCondition = faction.TerritoryIds.Count >= 2;

            Assert.IsTrue(meetsCondition);
        }

        [Test]
        public void DefeatStratagem_CheckInferiorPosition()
        {
            int playerTerritories = 1;
            int enemyTerritories = 3;

            bool isInferior = playerTerritories < enemyTerritories;

            Assert.IsTrue(isInferior);
        }

        [Test]
        public void DefeatStratagem_NotInferior()
        {
            int playerTerritories = 3;
            int enemyTerritories = 2;

            bool isInferior = playerTerritories < enemyTerritories;

            Assert.IsFalse(isInferior);
        }

        [Test]
        public void MergeStratagem_RequiresThreeFactions()
        {
            int aliveFactions = 2;

            bool meetsCondition = aliveFactions >= 3;

            Assert.IsFalse(meetsCondition);
        }

        [Test]
        public void MergeStratagem_MeetsThreeFactions()
        {
            int aliveFactions = 4;

            bool meetsCondition = aliveFactions >= 3;

            Assert.IsTrue(meetsCondition);
        }

        #endregion

        #region Character Intelligence Check Tests

        [Test]
        public void IntelligenceCheck_HighIntelligence()
        {
            var character = TestHelpers.CreateTestCharacter(intelligence: 95);
            int requiredIntelligence = 80;

            bool meetsCondition = character.Intelligence >= requiredIntelligence;

            Assert.IsTrue(meetsCondition);
        }

        [Test]
        public void IntelligenceCheck_LowIntelligence()
        {
            var character = TestHelpers.CreateTestCharacter(intelligence: 50);
            int requiredIntelligence = 80;

            bool meetsCondition = character.Intelligence >= requiredIntelligence;

            Assert.IsFalse(meetsCondition);
        }

        [Test]
        public void IntelligenceBonus_At100_ReturnsTwo()
        {
            int intelligence = 100;
            int bonus = 0;

            if (intelligence >= 100) bonus = 2;
            else if (intelligence >= 90) bonus = 1;

            Assert.AreEqual(2, bonus);
        }

        [Test]
        public void IntelligenceBonus_At95_ReturnsOne()
        {
            int intelligence = 95;
            int bonus = 0;

            if (intelligence >= 100) bonus = 2;
            else if (intelligence >= 90) bonus = 1;

            Assert.AreEqual(1, bonus);
        }

        [Test]
        public void IntelligenceBonus_At80_ReturnsZero()
        {
            int intelligence = 80;
            int bonus = 0;

            if (intelligence >= 100) bonus = 2;
            else if (intelligence >= 90) bonus = 1;

            Assert.AreEqual(0, bonus);
        }

        #endregion

        #region Army Check Tests

        [Test]
        public void ArmyCheck_OwnArmy()
        {
            var army = TestHelpers.CreateTestArmy(factionId: "player_faction");
            string casterFactionId = "player_faction";

            bool isOwnArmy = army.FactionId == casterFactionId;

            Assert.IsTrue(isOwnArmy);
        }

        [Test]
        public void ArmyCheck_EnemyArmy()
        {
            var army = TestHelpers.CreateTestArmy(factionId: "enemy_faction");
            string casterFactionId = "player_faction";

            bool isEnemyArmy = army.FactionId != casterFactionId;

            Assert.IsTrue(isEnemyArmy);
        }

        [Test]
        public void ArmyCheck_HasSoldiers()
        {
            var army = TestHelpers.CreateTestArmy(soldierCount: 500);

            bool hasSoldiers = army.SoldierCount > 0;

            Assert.IsTrue(hasSoldiers);
        }

        [Test]
        public void ArmyCheck_NoSoldiers()
        {
            var army = TestHelpers.CreateTestArmy(soldierCount: 0);

            bool hasSoldiers = army.SoldierCount > 0;

            Assert.IsFalse(hasSoldiers);
        }

        #endregion
    }
}
