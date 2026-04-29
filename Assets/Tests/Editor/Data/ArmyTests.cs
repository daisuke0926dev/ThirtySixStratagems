using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.Editor.Data
{
    /// <summary>
    /// Armyモデルのテスト
    /// </summary>
    [TestFixture]
    public class ArmyTests
    {
        #region ModifySoldiers Tests

        [Test]
        public void ModifySoldiers_WhenPositive_AddsSoldiers()
        {
            // Arrange
            var army = new Army { Soldiers = 1000 };

            // Act
            army.ModifySoldiers(500);

            // Assert
            Assert.AreEqual(1500, army.Soldiers);
        }

        [Test]
        public void ModifySoldiers_WhenNegative_RemovesSoldiers()
        {
            // Arrange
            var army = new Army { Soldiers = 1000 };

            // Act
            army.ModifySoldiers(-300);

            // Assert
            Assert.AreEqual(700, army.Soldiers);
        }

        [Test]
        public void ModifySoldiers_CannotGoBelowZero()
        {
            // Arrange
            var army = new Army { Soldiers = 100 };

            // Act
            army.ModifySoldiers(-500);

            // Assert
            Assert.AreEqual(0, army.Soldiers);
        }

        #endregion

        #region ModifyMorale Tests

        [Test]
        public void ModifyMorale_WhenPositive_IncreasessMorale()
        {
            // Arrange
            var army = new Army { Morale = 50 };

            // Act
            army.ModifyMorale(20);

            // Assert
            Assert.AreEqual(70, army.Morale);
        }

        [Test]
        public void ModifyMorale_WhenNegative_DecreasesMorale()
        {
            // Arrange
            var army = new Army { Morale = 80 };

            // Act
            army.ModifyMorale(-30);

            // Assert
            Assert.AreEqual(50, army.Morale);
        }

        [Test]
        public void ModifyMorale_ClampsAt100()
        {
            // Arrange
            var army = new Army { Morale = 90 };

            // Act
            army.ModifyMorale(50);

            // Assert
            Assert.AreEqual(100, army.Morale);
        }

        [Test]
        public void ModifyMorale_ClampsAtZero()
        {
            // Arrange
            var army = new Army { Morale = 20 };

            // Act
            army.ModifyMorale(-50);

            // Assert
            Assert.AreEqual(0, army.Morale);
        }

        #endregion

        #region Supplies Tests

        [Test]
        public void ConsumeSupplies_ReducesSupplies()
        {
            // Arrange
            var army = new Army { Supplies = 100, Morale = 80 };

            // Act
            army.ConsumeSupplies(30);

            // Assert
            Assert.AreEqual(70, army.Supplies);
            Assert.AreEqual(80, army.Morale); // 士気変化なし
        }

        [Test]
        public void ConsumeSupplies_WhenDepleted_ReducesMorale()
        {
            // Arrange
            var army = new Army { Supplies = 20, Morale = 80 };

            // Act
            army.ConsumeSupplies(30);

            // Assert
            Assert.AreEqual(0, army.Supplies);
            Assert.AreEqual(70, army.Morale); // 士気-10
        }

        [Test]
        public void Resupply_AddsSupplies()
        {
            // Arrange
            var army = new Army { Supplies = 50 };

            // Act
            army.Resupply(30);

            // Assert
            Assert.AreEqual(80, army.Supplies);
        }

        #endregion

        #region Movement Tests

        [Test]
        public void StartMovement_SetsMovingState()
        {
            // Arrange
            var army = new Army
            {
                LocationTerritoryId = "territory_a",
                State = ArmyState.Idle
            };
            var path = new List<string> { "territory_a", "territory_b", "territory_c" };

            // Act
            army.StartMovement(path, "territory_c");

            // Assert
            Assert.AreEqual(ArmyState.Moving, army.State);
            Assert.AreEqual("territory_c", army.TargetTerritoryId);
            Assert.AreEqual(0, army.MovementProgress);
            Assert.AreEqual(3, army.MovementPath.Count);
        }

        [Test]
        public void AdvanceMovement_WhenNotMoving_ReturnsFalse()
        {
            // Arrange
            var army = new Army { State = ArmyState.Idle };

            // Act
            bool result = army.AdvanceMovement();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void AdvanceMovement_ProgressesAlongPath()
        {
            // Arrange
            var army = new Army
            {
                LocationTerritoryId = "territory_a",
                State = ArmyState.Moving,
                MovementPath = new List<string> { "territory_a", "territory_b", "territory_c" },
                TargetTerritoryId = "territory_c",
                MovementProgress = 0
            };

            // Act
            bool arrived = army.AdvanceMovement();

            // Assert
            Assert.IsFalse(arrived);
            Assert.AreEqual(1, army.MovementProgress);
            Assert.AreEqual("territory_b", army.LocationTerritoryId);
        }

        [Test]
        public void AdvanceMovement_WhenArrived_ReturnsTrue()
        {
            // Arrange
            var army = new Army
            {
                LocationTerritoryId = "territory_b",
                State = ArmyState.Moving,
                MovementPath = new List<string> { "territory_a", "territory_b", "territory_c" },
                TargetTerritoryId = "territory_c",
                MovementProgress = 2
            };

            // Act
            bool arrived = army.AdvanceMovement();

            // Assert
            Assert.IsTrue(arrived);
            Assert.AreEqual(ArmyState.Idle, army.State);
            Assert.AreEqual("territory_c", army.LocationTerritoryId);
            Assert.IsNull(army.TargetTerritoryId);
            Assert.AreEqual(0, army.MovementPath.Count);
        }

        #endregion

        #region Retreat Tests

        [Test]
        public void Retreat_SetsRetreatingState()
        {
            // Arrange
            var army = new Army
            {
                State = ArmyState.Moving,
                Morale = 80,
                MovementPath = new List<string> { "a", "b", "c" },
                TargetTerritoryId = "c"
            };

            // Act
            army.Retreat("territory_safe");

            // Assert
            Assert.AreEqual(ArmyState.Retreating, army.State);
            Assert.AreEqual("territory_safe", army.LocationTerritoryId);
            Assert.AreEqual(0, army.MovementPath.Count);
            Assert.IsNull(army.TargetTerritoryId);
            Assert.AreEqual(60, army.Morale); // 士気-20
        }

        #endregion

        #region Effects Tests

        [Test]
        public void AddEffect_AddsToActiveEffects()
        {
            // Arrange
            var army = new Army { ActiveEffects = new List<ActiveEffect>() };
            var effect = new ActiveEffect
            {
                SourceStratagemId = "stratagem_001",
                EffectType = StratagemEffectType.AttackBoost,
                EffectValue = 20,
                RemainingTurns = 3
            };

            // Act
            army.AddEffect(effect);

            // Assert
            Assert.AreEqual(1, army.ActiveEffects.Count);
        }

        [Test]
        public void HasEffect_WhenHasActiveEffect_ReturnsTrue()
        {
            // Arrange
            var army = new Army
            {
                ActiveEffects = new List<ActiveEffect>
                {
                    new ActiveEffect
                    {
                        EffectType = StratagemEffectType.DefenseBoost,
                        RemainingTurns = 2
                    }
                }
            };

            // Act
            bool result = army.HasEffect(StratagemEffectType.DefenseBoost);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void HasEffect_WhenNoEffect_ReturnsFalse()
        {
            // Arrange
            var army = new Army { ActiveEffects = new List<ActiveEffect>() };

            // Act
            bool result = army.HasEffect(StratagemEffectType.AttackBoost);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void HasEffect_WhenExpired_ReturnsFalse()
        {
            // Arrange
            var army = new Army
            {
                ActiveEffects = new List<ActiveEffect>
                {
                    new ActiveEffect
                    {
                        EffectType = StratagemEffectType.DefenseBoost,
                        RemainingTurns = 0
                    }
                }
            };

            // Act
            bool result = army.HasEffect(StratagemEffectType.DefenseBoost);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateEffects_DecrementsRemainingTurns()
        {
            // Arrange
            var army = new Army
            {
                ActiveEffects = new List<ActiveEffect>
                {
                    new ActiveEffect { RemainingTurns = 3 }
                }
            };

            // Act
            army.UpdateEffects();

            // Assert
            Assert.AreEqual(2, army.ActiveEffects[0].RemainingTurns);
        }

        [Test]
        public void UpdateEffects_RemovesExpiredEffects()
        {
            // Arrange
            var army = new Army
            {
                ActiveEffects = new List<ActiveEffect>
                {
                    new ActiveEffect { RemainingTurns = 1 },
                    new ActiveEffect { RemainingTurns = 3 }
                }
            };

            // Act
            army.UpdateEffects();

            // Assert
            Assert.AreEqual(1, army.ActiveEffects.Count);
            Assert.AreEqual(2, army.ActiveEffects[0].RemainingTurns);
        }

        #endregion

        #region Merge Tests

        [Test]
        public void Merge_CombinesSoldiers()
        {
            // Arrange
            var army1 = new Army { Soldiers = 1000, Supplies = 100, Morale = 80, OfficerIds = new List<string>() };
            var army2 = new Army { Soldiers = 500, Supplies = 50, Morale = 60, OfficerIds = new List<string>() };

            // Act
            army1.Merge(army2);

            // Assert
            Assert.AreEqual(1500, army1.Soldiers);
        }

        [Test]
        public void Merge_CombinesSupplies()
        {
            // Arrange
            var army1 = new Army { Soldiers = 1000, Supplies = 100, Morale = 80, OfficerIds = new List<string>() };
            var army2 = new Army { Soldiers = 500, Supplies = 50, Morale = 60, OfficerIds = new List<string>() };

            // Act
            army1.Merge(army2);

            // Assert
            Assert.AreEqual(150, army1.Supplies);
        }

        [Test]
        public void Merge_AveragesMorale()
        {
            // Arrange
            var army1 = new Army { Soldiers = 1000, Supplies = 100, Morale = 80, OfficerIds = new List<string>() };
            var army2 = new Army { Soldiers = 500, Supplies = 50, Morale = 60, OfficerIds = new List<string>() };

            // Act
            army1.Merge(army2);

            // Assert
            Assert.AreEqual(70, army1.Morale);
        }

        [Test]
        public void Merge_CombinesOfficers()
        {
            // Arrange
            var army1 = new Army
            {
                Soldiers = 1000,
                Supplies = 100,
                Morale = 80,
                OfficerIds = new List<string> { "officer_1" }
            };
            var army2 = new Army
            {
                Soldiers = 500,
                Supplies = 50,
                Morale = 60,
                OfficerIds = new List<string> { "officer_2" }
            };

            // Act
            army1.Merge(army2);

            // Assert
            Assert.AreEqual(2, army1.OfficerIds.Count);
            Assert.Contains("officer_1", army1.OfficerIds);
            Assert.Contains("officer_2", army1.OfficerIds);
        }

        #endregion

        #region Split Tests

        [Test]
        public void Split_CreatesNewArmy()
        {
            // Arrange
            var army = new Army
            {
                Id = "army_original",
                OwnerId = "faction_wei",
                Soldiers = 1000,
                Supplies = 100,
                Morale = 80,
                LocationTerritoryId = "territory_a"
            };

            // Act
            var newArmy = army.Split(300, "army_new");

            // Assert
            Assert.IsNotNull(newArmy);
            Assert.AreEqual("army_new", newArmy.Id);
            Assert.AreEqual("faction_wei", newArmy.OwnerId);
            Assert.AreEqual(300, newArmy.Soldiers);
            Assert.AreEqual(700, army.Soldiers);
        }

        [Test]
        public void Split_SplitsSuppliesEvenly()
        {
            // Arrange
            var army = new Army
            {
                Soldiers = 1000,
                Supplies = 100,
                Morale = 80,
                LocationTerritoryId = "territory_a"
            };

            // Act
            var newArmy = army.Split(300, "army_new");

            // Assert
            Assert.AreEqual(50, newArmy.Supplies);
            Assert.AreEqual(50, army.Supplies);
        }

        [Test]
        public void Split_InheritsMorale()
        {
            // Arrange
            var army = new Army
            {
                Soldiers = 1000,
                Supplies = 100,
                Morale = 75,
                LocationTerritoryId = "territory_a"
            };

            // Act
            var newArmy = army.Split(300, "army_new");

            // Assert
            Assert.AreEqual(75, newArmy.Morale);
        }

        [Test]
        public void Split_WhenAllSoldiers_ReturnsNull()
        {
            // Arrange
            var army = new Army { Soldiers = 1000, Supplies = 100, Morale = 80 };

            // Act
            var newArmy = army.Split(1000, "army_new");

            // Assert
            Assert.IsNull(newArmy);
        }

        [Test]
        public void Split_WhenZeroSoldiers_ReturnsNull()
        {
            // Arrange
            var army = new Army { Soldiers = 1000, Supplies = 100, Morale = 80 };

            // Act
            var newArmy = army.Split(0, "army_new");

            // Assert
            Assert.IsNull(newArmy);
        }

        #endregion
    }
}
