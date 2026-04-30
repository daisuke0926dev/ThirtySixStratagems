using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// ResourceManager関連のテスト
    /// リソース計算ロジックのユニットテスト
    /// </summary>
    [TestFixture]
    public class ResourceManagerTests
    {
        #region Resource Type Tests

        [Test]
        public void ResourceType_AllTypesExist()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Gold));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Food));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.StratagemPoints));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ResourceType), ResourceType.Soldiers));
        }

        #endregion

        #region Gold Income Calculation Tests

        [Test]
        public void GoldIncome_SingleTerritory_CalculatedCorrectly()
        {
            var territory = TestHelpers.CreateTestTerritory(economy: 50, population: 10000);

            // 基本収入 = 経済力 × 基本係数
            int baseIncome = territory.Economy * Constants.Balance.BaseIncomePerEconomy;
            // 人口ボーナス
            int populationBonus = territory.Population / Constants.Balance.PopulationIncomeBonus;
            int totalIncome = baseIncome + populationBonus;

            Assert.AreEqual(500, baseIncome); // 50 * 10
            Assert.AreEqual(10, populationBonus); // 10000 / 1000
            Assert.AreEqual(510, totalIncome);
        }

        [Test]
        public void GoldIncome_HighEconomy_HigherIncome()
        {
            var highEcon = TestHelpers.CreateTestTerritory(economy: 100, population: 5000);
            var lowEcon = TestHelpers.CreateTestTerritory(economy: 20, population: 5000);

            int highIncome = highEcon.Economy * Constants.Balance.BaseIncomePerEconomy;
            int lowIncome = lowEcon.Economy * Constants.Balance.BaseIncomePerEconomy;

            Assert.Greater(highIncome, lowIncome);
        }

        [Test]
        public void GoldIncome_ZeroEconomy_OnlyPopulationBonus()
        {
            var territory = TestHelpers.CreateTestTerritory(economy: 0, population: 5000);

            int baseIncome = territory.Economy * Constants.Balance.BaseIncomePerEconomy;
            int populationBonus = territory.Population / Constants.Balance.PopulationIncomeBonus;

            Assert.AreEqual(0, baseIncome);
            Assert.AreEqual(5, populationBonus);
        }

        #endregion

        #region Food Production Tests

        [Test]
        public void FoodProduction_BasedOnPopulation()
        {
            var territory = TestHelpers.CreateTestTerritory(population: 10000);

            int foodProduction = territory.Population / 100;

            Assert.AreEqual(100, foodProduction);
        }

        [Test]
        public void FoodProduction_LargePopulation_MoreFood()
        {
            var largePop = TestHelpers.CreateTestTerritory(population: 50000);
            var smallPop = TestHelpers.CreateTestTerritory(population: 5000);

            int largeFood = largePop.Population / 100;
            int smallFood = smallPop.Population / 100;

            Assert.Greater(largeFood, smallFood);
            Assert.AreEqual(500, largeFood);
            Assert.AreEqual(50, smallFood);
        }

        #endregion

        #region Food Consumption Tests

        [Test]
        public void FoodConsumption_PerSoldier()
        {
            int soldierCount = 1000;
            int consumption = soldierCount * Constants.Balance.FoodConsumptionPerSoldier;

            Assert.AreEqual(1000, consumption); // 1 per soldier
        }

        [Test]
        public void FoodConsumption_LargeArmy_HighConsumption()
        {
            int largeSoldiers = 5000;
            int smallSoldiers = 500;

            int largeConsumption = largeSoldiers * Constants.Balance.FoodConsumptionPerSoldier;
            int smallConsumption = smallSoldiers * Constants.Balance.FoodConsumptionPerSoldier;

            Assert.Greater(largeConsumption, smallConsumption);
        }

        [Test]
        public void FoodConsumption_ZeroSoldiers_ZeroConsumption()
        {
            int soldierCount = 0;
            int consumption = soldierCount * Constants.Balance.FoodConsumptionPerSoldier;

            Assert.AreEqual(0, consumption);
        }

        #endregion

        #region Stratagem Point Tests

        [Test]
        public void StratagemPoints_BaseRecovery()
        {
            int baseRecovery = Constants.Balance.StratagemPointRecoveryBase;

            Assert.AreEqual(2, baseRecovery);
        }

        [Test]
        public void StratagemPoints_MaxLimit()
        {
            int maxSP = Constants.Balance.DefaultMaxStratagemPoints;

            Assert.AreEqual(10, maxSP);
        }

        [Test]
        public void StratagemPoints_ClampToMax()
        {
            int currentSP = 9;
            int recovery = 3;
            int maxSP = Constants.Balance.DefaultMaxStratagemPoints;

            int newSP = System.Math.Min(currentSP + recovery, maxSP);

            Assert.AreEqual(10, newSP);
        }

        [Test]
        public void StratagemPoints_RecoveryWithIntelligenceBonus()
        {
            int baseRecovery = Constants.Balance.StratagemPointRecoveryBase;
            int intelligenceBonus = 2; // 知力100の場合

            int totalRecovery = baseRecovery + intelligenceBonus;

            Assert.AreEqual(4, totalRecovery);
        }

        #endregion

        #region Recruitment Tests

        [Test]
        public void Recruitment_MaxFromPopulation()
        {
            var territory = TestHelpers.CreateTestTerritory(population: 10000);

            int maxRecruitment = territory.Population / Constants.Balance.RecruitmentPopulationRatio;

            Assert.AreEqual(1000, maxRecruitment);
        }

        [Test]
        public void Recruitment_CostCalculation()
        {
            int soldierCount = 100;

            int cost = soldierCount * Constants.Balance.RecruitmentCostPerSoldier;

            Assert.AreEqual(1000, cost);
        }

        [Test]
        public void Recruitment_CanAfford()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 5000);
            int soldierCount = 100;
            int cost = soldierCount * Constants.Balance.RecruitmentCostPerSoldier;

            bool canAfford = faction.Gold >= cost;

            Assert.IsTrue(canAfford);
        }

        [Test]
        public void Recruitment_CannotAfford()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 500);
            int soldierCount = 100;
            int cost = soldierCount * Constants.Balance.RecruitmentCostPerSoldier;

            bool canAfford = faction.Gold >= cost;

            Assert.IsFalse(canAfford);
        }

        [Test]
        public void Recruitment_DecreasesPopulation()
        {
            var territory = TestHelpers.CreateTestTerritory(population: 10000);
            int recruitedSoldiers = 500;

            int newPopulation = territory.Population - recruitedSoldiers;

            Assert.AreEqual(9500, newPopulation);
        }

        #endregion

        #region Resource Operations Tests

        [Test]
        public void ResourceOperation_AddGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 1000);
            int addAmount = 500;

            faction.Gold += addAmount;

            Assert.AreEqual(1500, faction.Gold);
        }

        [Test]
        public void ResourceOperation_ConsumeGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 1000);
            int consumeAmount = 300;

            faction.Gold -= consumeAmount;

            Assert.AreEqual(700, faction.Gold);
        }

        [Test]
        public void ResourceOperation_HasEnoughGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 1000);
            int required = 500;

            bool hasEnough = faction.Gold >= required;

            Assert.IsTrue(hasEnough);
        }

        [Test]
        public void ResourceOperation_NotEnoughGold()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 200);
            int required = 500;

            bool hasEnough = faction.Gold >= required;

            Assert.IsFalse(hasEnough);
        }

        [Test]
        public void ResourceOperation_AddFood()
        {
            var faction = TestHelpers.CreateTestFaction(food: 500);
            int addAmount = 200;

            faction.Food += addAmount;

            Assert.AreEqual(700, faction.Food);
        }

        [Test]
        public void ResourceOperation_ConsumeStratagemPoints()
        {
            var faction = TestHelpers.CreateTestFaction(stratagemPoints: 8);
            int consumeAmount = 3;

            faction.StratagemPoints -= consumeAmount;

            Assert.AreEqual(5, faction.StratagemPoints);
        }

        #endregion

        #region Multiple Resource Check Tests

        [Test]
        public void MultipleResources_AllSufficient()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 1000, food: 500, stratagemPoints: 5);
            var costs = new Dictionary<ResourceType, int>
            {
                { ResourceType.Gold, 500 },
                { ResourceType.StratagemPoints, 3 }
            };

            bool hasEnough = faction.Gold >= costs[ResourceType.Gold] &&
                             faction.StratagemPoints >= costs[ResourceType.StratagemPoints];

            Assert.IsTrue(hasEnough);
        }

        [Test]
        public void MultipleResources_OneMissing()
        {
            var faction = TestHelpers.CreateTestFaction(gold: 200, food: 500, stratagemPoints: 5);
            var costs = new Dictionary<ResourceType, int>
            {
                { ResourceType.Gold, 500 },
                { ResourceType.StratagemPoints, 3 }
            };

            bool hasEnoughGold = faction.Gold >= costs[ResourceType.Gold];
            bool hasEnoughSP = faction.StratagemPoints >= costs[ResourceType.StratagemPoints];
            bool hasAll = hasEnoughGold && hasEnoughSP;

            Assert.IsFalse(hasEnoughGold);
            Assert.IsTrue(hasEnoughSP);
            Assert.IsFalse(hasAll);
        }

        #endregion

        #region Resource Transfer Tests

        [Test]
        public void ResourceTransfer_GoldBetweenFactions()
        {
            var from = TestHelpers.CreateTestFaction(id: "from", gold: 1000);
            var to = TestHelpers.CreateTestFaction(id: "to", gold: 500);
            int transferAmount = 300;

            from.Gold -= transferAmount;
            to.Gold += transferAmount;

            Assert.AreEqual(700, from.Gold);
            Assert.AreEqual(800, to.Gold);
        }

        [Test]
        public void ResourceTransfer_CannotTransferMoreThanHave()
        {
            var from = TestHelpers.CreateTestFaction(id: "from", gold: 200);
            int transferAmount = 500;

            bool canTransfer = from.Gold >= transferAmount;

            Assert.IsFalse(canTransfer);
        }

        #endregion

        #region Morale Effects Tests

        [Test]
        public void MoraleLoss_FoodShortage()
        {
            int moraleLoss = Constants.Balance.MoraleLossNoSupply;

            Assert.AreEqual(10, moraleLoss);
        }

        [Test]
        public void MoraleRecovery_PerTurn()
        {
            int moraleRecovery = Constants.Balance.MoraleRecoveryPerTurn;

            Assert.AreEqual(5, moraleRecovery);
        }

        [Test]
        public void Morale_ClampToMax()
        {
            int currentMorale = 98;
            int recovery = 5;
            int maxMorale = Constants.Balance.MaxMorale;

            int newMorale = System.Math.Min(currentMorale + recovery, maxMorale);

            Assert.AreEqual(100, newMorale);
        }

        [Test]
        public void Morale_ClampToZero()
        {
            int currentMorale = 5;
            int loss = 10;

            int newMorale = System.Math.Max(currentMorale - loss, 0);

            Assert.AreEqual(0, newMorale);
        }

        #endregion
    }
}
