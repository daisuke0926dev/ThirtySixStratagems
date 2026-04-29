using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.Editor.Data
{
    /// <summary>
    /// Territoryモデルのテスト
    /// </summary>
    [TestFixture]
    public class TerritoryTests
    {
        #region CalculateIncome Tests

        [Test]
        public void CalculateIncome_WithNoBuildings_ReturnsBaseIncome()
        {
            // Arrange
            var territory = new Territory
            {
                Economy = 50,
                Population = 10000,
                Buildings = new List<Building>()
            };

            // Act
            int income = territory.CalculateIncome();

            // Assert
            // baseIncome = 50 * 10 = 500, populationBonus = 10000/1000 = 10
            Assert.AreEqual(510, income);
        }

        [Test]
        public void CalculateIncome_WithMarket_IncludesMarketBonus()
        {
            // Arrange
            var territory = new Territory
            {
                Economy = 50,
                Population = 10000,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Market, Level = 3 }
                }
            };

            // Act
            int income = territory.CalculateIncome();

            // Assert
            // baseIncome = 500, populationBonus = 10, marketBonus = 3 * 5 = 15
            Assert.AreEqual(525, income);
        }

        [Test]
        public void CalculateIncome_WithMultipleMarkets_SumsAllBonuses()
        {
            // Arrange
            var territory = new Territory
            {
                Economy = 50,
                Population = 10000,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Market, Level = 2 },
                    new Building { Type = BuildingType.Market, Level = 3 }
                }
            };

            // Act
            int income = territory.CalculateIncome();

            // Assert
            // marketBonus = (2 * 5) + (3 * 5) = 25
            Assert.AreEqual(535, income);
        }

        #endregion

        #region CalculateMaxRecruitment Tests

        [Test]
        public void CalculateMaxRecruitment_WithNoBarracks_ReturnsBaseValue()
        {
            // Arrange
            var territory = new Territory
            {
                Population = 10000,
                Buildings = new List<Building>()
            };

            // Act
            int maxRecruitment = territory.CalculateMaxRecruitment();

            // Assert
            // baseRecruitment = 10000 / 10 = 1000
            Assert.AreEqual(1000, maxRecruitment);
        }

        [Test]
        public void CalculateMaxRecruitment_WithBarracks_IncludesBonus()
        {
            // Arrange
            var territory = new Territory
            {
                Population = 10000,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Barracks, Level = 2 }
                }
            };

            // Act
            int maxRecruitment = territory.CalculateMaxRecruitment();

            // Assert
            // baseRecruitment = 1000, barracksBonus = 2 * 100 = 200
            Assert.AreEqual(1200, maxRecruitment);
        }

        #endregion

        #region CalculateFoodProduction Tests

        [Test]
        public void CalculateFoodProduction_WithNoFarm_ReturnsBaseValue()
        {
            // Arrange
            var territory = new Territory
            {
                Population = 10000,
                Buildings = new List<Building>()
            };

            // Act
            int foodProduction = territory.CalculateFoodProduction();

            // Assert
            // baseFoodProduction = 10000 / 5 = 2000
            Assert.AreEqual(2000, foodProduction);
        }

        [Test]
        public void CalculateFoodProduction_WithFarm_IncludesBonus()
        {
            // Arrange
            var territory = new Territory
            {
                Population = 10000,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Farm, Level = 3 }
                }
            };

            // Act
            int foodProduction = territory.CalculateFoodProduction();

            // Assert
            // baseFoodProduction = 2000, farmBonus = 3 * 50 = 150
            Assert.AreEqual(2150, foodProduction);
        }

        #endregion

        #region CalculateDefenseBonus Tests

        [Test]
        public void CalculateDefenseBonus_WithNoBuildings_ReturnsBaseDefense()
        {
            // Arrange
            var territory = new Territory
            {
                Defense = 50,
                Buildings = new List<Building>()
            };

            // Act
            int defenseBonus = territory.CalculateDefenseBonus();

            // Assert
            Assert.AreEqual(50, defenseBonus);
        }

        [Test]
        public void CalculateDefenseBonus_WithCastle_IncludesCastleBonus()
        {
            // Arrange
            var territory = new Territory
            {
                Defense = 50,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Castle, Level = 3 }
                }
            };

            // Act
            int defenseBonus = territory.CalculateDefenseBonus();

            // Assert
            // Defense = 50, castleBonus = 3 * 10 = 30
            Assert.AreEqual(80, defenseBonus);
        }

        [Test]
        public void CalculateDefenseBonus_WithWatchtower_IncludesWatchtowerBonus()
        {
            // Arrange
            var territory = new Territory
            {
                Defense = 50,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Watchtower, Level = 4 }
                }
            };

            // Act
            int defenseBonus = territory.CalculateDefenseBonus();

            // Assert
            // Defense = 50, watchtowerBonus = 4 * 5 = 20
            Assert.AreEqual(70, defenseBonus);
        }

        [Test]
        public void CalculateDefenseBonus_WithMultipleDefensiveBuildings_SumsBonuses()
        {
            // Arrange
            var territory = new Territory
            {
                Defense = 50,
                Buildings = new List<Building>
                {
                    new Building { Type = BuildingType.Castle, Level = 2 },
                    new Building { Type = BuildingType.Watchtower, Level = 3 }
                }
            };

            // Act
            int defenseBonus = territory.CalculateDefenseBonus();

            // Assert
            // Defense = 50, castleBonus = 20, watchtowerBonus = 15
            Assert.AreEqual(85, defenseBonus);
        }

        #endregion
    }
}
