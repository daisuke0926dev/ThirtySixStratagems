using NUnit.Framework;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Tests.Editor.Battle
{
    /// <summary>
    /// 戦闘データクラスのテスト
    /// </summary>
    [TestFixture]
    public class BattleDataTests
    {
        #region CombatStats Tests

        [Test]
        public void CombatStats_WhenCreated_HasDefaultValues()
        {
            // Arrange & Act
            var stats = new CombatStats();

            // Assert
            Assert.AreEqual(0, stats.BasePower);
            Assert.AreEqual(0, stats.SoldierCount);
            Assert.AreEqual(0, stats.Morale);
            Assert.AreEqual(0, stats.CommanderBonus);
            Assert.AreEqual(0, stats.TerrainBonus);
            Assert.AreEqual(0, stats.StratagemBonus);
        }

        [Test]
        public void CombatStats_WhenSet_RetainsValues()
        {
            // Arrange & Act
            var stats = new CombatStats
            {
                BasePower = 100,
                SoldierCount = 5000,
                Morale = 80,
                CommanderBonus = 15,
                TerrainBonus = 10,
                StratagemBonus = 20,
                MoraleModifier = 1.3f,
                SoldierModifier = 0.9f,
                EffectivePower = 150,
                CriticalRate = 0.1f
            };

            // Assert
            Assert.AreEqual(100, stats.BasePower);
            Assert.AreEqual(5000, stats.SoldierCount);
            Assert.AreEqual(80, stats.Morale);
            Assert.AreEqual(15, stats.CommanderBonus);
            Assert.AreEqual(10, stats.TerrainBonus);
            Assert.AreEqual(20, stats.StratagemBonus);
            Assert.AreEqual(1.3f, stats.MoraleModifier, 0.001f);
            Assert.AreEqual(0.9f, stats.SoldierModifier, 0.001f);
            Assert.AreEqual(150, stats.EffectivePower);
            Assert.AreEqual(0.1f, stats.CriticalRate, 0.001f);
        }

        #endregion

        #region CombatResolutionResult Tests

        [Test]
        public void CombatResolutionResult_AttackerWins_WhenDefenderCasualtiesHigher()
        {
            // Arrange
            var result = new CombatResolutionResult
            {
                AttackerCasualties = 100,
                DefenderCasualties = 200,
                AttackerWonRound = true
            };

            // Assert
            Assert.IsTrue(result.AttackerWonRound);
            Assert.Greater(result.DefenderCasualties, result.AttackerCasualties);
        }

        [Test]
        public void CombatResolutionResult_DefenderWins_WhenAttackerCasualtiesHigher()
        {
            // Arrange
            var result = new CombatResolutionResult
            {
                AttackerCasualties = 300,
                DefenderCasualties = 150,
                AttackerWonRound = false
            };

            // Assert
            Assert.IsFalse(result.AttackerWonRound);
            Assert.Greater(result.AttackerCasualties, result.DefenderCasualties);
        }

        [Test]
        public void CombatResolutionResult_AttackerCritical_IsTracked()
        {
            // Arrange
            var result = new CombatResolutionResult
            {
                AttackerCritical = true,
                DefenderCritical = false
            };

            // Assert
            Assert.IsTrue(result.AttackerCritical);
            Assert.IsFalse(result.DefenderCritical);
        }

        [Test]
        public void CombatResolutionResult_BothCritical_IsValid()
        {
            // Arrange
            var result = new CombatResolutionResult
            {
                AttackerCritical = true,
                DefenderCritical = true,
                AttackerCasualties = 150,
                DefenderCasualties = 150
            };

            // Assert
            Assert.IsTrue(result.AttackerCritical);
            Assert.IsTrue(result.DefenderCritical);
            Assert.AreEqual(result.AttackerCasualties, result.DefenderCasualties);
        }

        #endregion

        #region MoraleChangeResult Tests

        [Test]
        public void MoraleChangeResult_WhenAttackerWins_HasPositiveAttackerChange()
        {
            // Arrange
            var result = new MoraleChangeResult
            {
                AttackerChange = 5,
                DefenderChange = -10
            };

            // Assert
            Assert.Greater(result.AttackerChange, 0);
            Assert.Less(result.DefenderChange, 0);
        }

        [Test]
        public void MoraleChangeResult_WhenDefenderWins_HasPositiveDefenderChange()
        {
            // Arrange
            var result = new MoraleChangeResult
            {
                AttackerChange = -8,
                DefenderChange = 3
            };

            // Assert
            Assert.Less(result.AttackerChange, 0);
            Assert.Greater(result.DefenderChange, 0);
        }

        [Test]
        public void MoraleChangeResult_CanBeBothNegative_WhenHeavyLosses()
        {
            // Arrange - both sides take heavy losses
            var result = new MoraleChangeResult
            {
                AttackerChange = -5,
                DefenderChange = -3
            };

            // Assert
            Assert.Less(result.AttackerChange, 0);
            Assert.Less(result.DefenderChange, 0);
        }

        #endregion

        #region BattlePrediction Tests

        [Test]
        public void BattlePrediction_HighAttackerPower_HasHighWinChance()
        {
            // Arrange
            var prediction = new BattlePrediction
            {
                AttackerPower = 200,
                DefenderPower = 100,
                AttackerWinChance = 75f,
                ExpectedAttackerLosses = 500,
                ExpectedDefenderLosses = 1500
            };

            // Assert
            Assert.Greater(prediction.AttackerPower, prediction.DefenderPower);
            Assert.Greater(prediction.AttackerWinChance, 50f);
            Assert.Less(prediction.ExpectedAttackerLosses, prediction.ExpectedDefenderLosses);
        }

        [Test]
        public void BattlePrediction_LowAttackerPower_HasLowWinChance()
        {
            // Arrange
            var prediction = new BattlePrediction
            {
                AttackerPower = 80,
                DefenderPower = 150,
                AttackerWinChance = 30f,
                ExpectedAttackerLosses = 1200,
                ExpectedDefenderLosses = 600
            };

            // Assert
            Assert.Less(prediction.AttackerPower, prediction.DefenderPower);
            Assert.Less(prediction.AttackerWinChance, 50f);
            Assert.Greater(prediction.ExpectedAttackerLosses, prediction.ExpectedDefenderLosses);
        }

        [Test]
        public void BattlePrediction_EqualPower_HasBalancedChance()
        {
            // Arrange
            var prediction = new BattlePrediction
            {
                AttackerPower = 100,
                DefenderPower = 100,
                AttackerWinChance = 50f,
                ExpectedAttackerLosses = 800,
                ExpectedDefenderLosses = 800
            };

            // Assert
            Assert.AreEqual(prediction.AttackerPower, prediction.DefenderPower);
            Assert.AreEqual(50f, prediction.AttackerWinChance, 5f);
        }

        [Test]
        public void BattlePrediction_WinChance_IsInValidRange()
        {
            // Arrange
            var predictions = new[]
            {
                new BattlePrediction { AttackerWinChance = 5f },
                new BattlePrediction { AttackerWinChance = 50f },
                new BattlePrediction { AttackerWinChance = 95f }
            };

            // Assert
            foreach (var prediction in predictions)
            {
                Assert.GreaterOrEqual(prediction.AttackerWinChance, 0f);
                Assert.LessOrEqual(prediction.AttackerWinChance, 100f);
            }
        }

        #endregion
    }
}
