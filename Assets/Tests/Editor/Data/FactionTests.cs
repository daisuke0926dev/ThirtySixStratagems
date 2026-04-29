using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.Editor.Data
{
    /// <summary>
    /// Factionモデルのテスト
    /// </summary>
    [TestFixture]
    public class FactionTests
    {
        #region StratagemPoints Tests

        [Test]
        public void ConsumeStratagemPoints_WhenSufficient_ReturnsTrue()
        {
            // Arrange
            var faction = new Faction
            {
                StratagemPoints = 5,
                MaxStratagemPoints = 10
            };

            // Act
            bool result = faction.ConsumeStratagemPoints(3);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2, faction.StratagemPoints);
        }

        [Test]
        public void ConsumeStratagemPoints_WhenInsufficient_ReturnsFalse()
        {
            // Arrange
            var faction = new Faction
            {
                StratagemPoints = 2,
                MaxStratagemPoints = 10
            };

            // Act
            bool result = faction.ConsumeStratagemPoints(5);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(2, faction.StratagemPoints);
        }

        [Test]
        public void ConsumeStratagemPoints_WhenExactMatch_ReturnsTrue()
        {
            // Arrange
            var faction = new Faction
            {
                StratagemPoints = 3,
                MaxStratagemPoints = 10
            };

            // Act
            bool result = faction.ConsumeStratagemPoints(3);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, faction.StratagemPoints);
        }

        [Test]
        public void RecoverStratagemPoints_WhenBelowMax_Recovers()
        {
            // Arrange
            var faction = new Faction
            {
                StratagemPoints = 5,
                MaxStratagemPoints = 10
            };

            // Act
            faction.RecoverStratagemPoints(3);

            // Assert
            Assert.AreEqual(8, faction.StratagemPoints);
        }

        [Test]
        public void RecoverStratagemPoints_WhenExceedsMax_CapsAtMax()
        {
            // Arrange
            var faction = new Faction
            {
                StratagemPoints = 8,
                MaxStratagemPoints = 10
            };

            // Act
            faction.RecoverStratagemPoints(5);

            // Assert
            Assert.AreEqual(10, faction.StratagemPoints);
        }

        #endregion

        #region Relations Tests

        [Test]
        public void GetRelation_WhenNoRelation_ReturnsZero()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>()
            };

            // Act
            int relation = faction.GetRelation("unknown_faction");

            // Assert
            Assert.AreEqual(0, relation);
        }

        [Test]
        public void GetRelation_WhenHasRelation_ReturnsValue()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", 50 }
                }
            };

            // Act
            int relation = faction.GetRelation("faction_wei");

            // Assert
            Assert.AreEqual(50, relation);
        }

        [Test]
        public void ModifyRelation_WhenNoExistingRelation_CreatesNew()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>()
            };

            // Act
            faction.ModifyRelation("faction_wei", 25);

            // Assert
            Assert.AreEqual(25, faction.GetRelation("faction_wei"));
        }

        [Test]
        public void ModifyRelation_WhenExisting_ModifiesValue()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", 30 }
                }
            };

            // Act
            faction.ModifyRelation("faction_wei", 20);

            // Assert
            Assert.AreEqual(50, faction.GetRelation("faction_wei"));
        }

        [Test]
        public void ModifyRelation_ClampsBetweenMinusAndPlus100()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", 90 }
                }
            };

            // Act
            faction.ModifyRelation("faction_wei", 50);

            // Assert
            Assert.AreEqual(100, faction.GetRelation("faction_wei"));
        }

        [Test]
        public void ModifyRelation_NegativeClamps()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", -80 }
                }
            };

            // Act
            faction.ModifyRelation("faction_wei", -50);

            // Assert
            Assert.AreEqual(-100, faction.GetRelation("faction_wei"));
        }

        #endregion

        #region DiplomaticStatus Tests

        [Test]
        public void GetDiplomaticStatus_WhenAllied_ReturnsAlliance()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>(),
                Alliances = new List<Alliance>
                {
                    new Alliance { FactionId = "faction_wei", IsActive = true }
                }
            };

            // Act
            var status = faction.GetDiplomaticStatus("faction_wei");

            // Assert
            Assert.AreEqual(DiplomaticStatus.Alliance, status);
        }

        [Test]
        public void GetDiplomaticStatus_WhenLowRelation_ReturnsWar()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", -80 }
                },
                Alliances = new List<Alliance>()
            };

            // Act
            var status = faction.GetDiplomaticStatus("faction_wei");

            // Assert
            Assert.AreEqual(DiplomaticStatus.War, status);
        }

        [Test]
        public void GetDiplomaticStatus_WhenHostile_ReturnsHostile()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", -50 }
                },
                Alliances = new List<Alliance>()
            };

            // Act
            var status = faction.GetDiplomaticStatus("faction_wei");

            // Assert
            Assert.AreEqual(DiplomaticStatus.Hostile, status);
        }

        [Test]
        public void GetDiplomaticStatus_WhenFriendly_ReturnsFriendly()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", 80 }
                },
                Alliances = new List<Alliance>()
            };

            // Act
            var status = faction.GetDiplomaticStatus("faction_wei");

            // Assert
            Assert.AreEqual(DiplomaticStatus.Friendly, status);
        }

        [Test]
        public void GetDiplomaticStatus_WhenNeutral_ReturnsNeutral()
        {
            // Arrange
            var faction = new Faction
            {
                Relations = new Dictionary<string, int>
                {
                    { "faction_wei", 0 }
                },
                Alliances = new List<Alliance>()
            };

            // Act
            var status = faction.GetDiplomaticStatus("faction_wei");

            // Assert
            Assert.AreEqual(DiplomaticStatus.Neutral, status);
        }

        #endregion

        #region Stratagem Unlock Tests

        [Test]
        public void IsStratagemUnlocked_WhenNotUnlocked_ReturnsFalse()
        {
            // Arrange
            var faction = new Faction
            {
                UnlockedStratagemIds = new List<string>()
            };

            // Act
            bool result = faction.IsStratagemUnlocked("stratagem_001");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsStratagemUnlocked_WhenUnlocked_ReturnsTrue()
        {
            // Arrange
            var faction = new Faction
            {
                UnlockedStratagemIds = new List<string> { "stratagem_001" }
            };

            // Act
            bool result = faction.IsStratagemUnlocked("stratagem_001");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void UnlockStratagem_AddsToList()
        {
            // Arrange
            var faction = new Faction
            {
                UnlockedStratagemIds = new List<string>()
            };

            // Act
            faction.UnlockStratagem("stratagem_001");

            // Assert
            Assert.IsTrue(faction.IsStratagemUnlocked("stratagem_001"));
        }

        [Test]
        public void UnlockStratagem_WhenAlreadyUnlocked_DoesNotDuplicate()
        {
            // Arrange
            var faction = new Faction
            {
                UnlockedStratagemIds = new List<string> { "stratagem_001" }
            };

            // Act
            faction.UnlockStratagem("stratagem_001");

            // Assert
            Assert.AreEqual(1, faction.UnlockedStratagemIds.Count);
        }

        #endregion

        #region Alliance Tests

        [Test]
        public void Alliance_IsValid_WhenActiveAndNoExpiry_ReturnsTrue()
        {
            // Arrange
            var alliance = new Alliance
            {
                FactionId = "faction_wei",
                FormedTurn = 1,
                Duration = 0, // 無期限
                IsActive = true
            };

            // Act
            bool result = alliance.IsValid(100);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Alliance_IsValid_WhenNotActive_ReturnsFalse()
        {
            // Arrange
            var alliance = new Alliance
            {
                FactionId = "faction_wei",
                FormedTurn = 1,
                Duration = 0,
                IsActive = false
            };

            // Act
            bool result = alliance.IsValid(5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Alliance_IsValid_WhenWithinDuration_ReturnsTrue()
        {
            // Arrange
            var alliance = new Alliance
            {
                FactionId = "faction_wei",
                FormedTurn = 5,
                Duration = 10,
                IsActive = true
            };

            // Act
            bool result = alliance.IsValid(10);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Alliance_IsValid_WhenExpired_ReturnsFalse()
        {
            // Arrange
            var alliance = new Alliance
            {
                FactionId = "faction_wei",
                FormedTurn = 5,
                Duration = 10,
                IsActive = true
            };

            // Act
            bool result = alliance.IsValid(20);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
