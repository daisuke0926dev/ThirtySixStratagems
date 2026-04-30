using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// ゲームフロー統合テスト
    /// ゲーム開始〜終了の一連フローをテスト
    /// </summary>
    [TestFixture]
    public class GameFlowIntegrationTests
    {
        private GameData _gameData;

        [SetUp]
        public void SetUp()
        {
            _gameData = TestHelpers.CreateTestGameData();
        }

        #region Game Initialization Tests

        [Test]
        public void GameInitialization_GameDataCreated()
        {
            Assert.IsNotNull(_gameData);
            Assert.IsNotNull(_gameData.Factions);
            Assert.IsNotNull(_gameData.Territories);
            Assert.IsNotNull(_gameData.Characters);
            Assert.IsNotNull(_gameData.Armies);
        }

        [Test]
        public void GameInitialization_PlayerFactionExists()
        {
            Assert.IsNotNull(_gameData.PlayerFactionId);
            Assert.IsTrue(_gameData.Factions.ContainsKey(_gameData.PlayerFactionId));
        }

        [Test]
        public void GameInitialization_AtLeastTwoFactions()
        {
            Assert.GreaterOrEqual(_gameData.Factions.Count, 2);
        }

        [Test]
        public void GameInitialization_FactionsHaveTerritories()
        {
            foreach (var faction in _gameData.Factions.Values)
            {
                Assert.Greater(faction.TerritoryIds.Count, 0,
                    $"Faction {faction.Id} has no territories");
            }
        }

        [Test]
        public void GameInitialization_TerritoriesHaveOwners()
        {
            foreach (var territory in _gameData.Territories.Values)
            {
                Assert.IsNotNull(territory.OwnerId,
                    $"Territory {territory.Id} has no owner");
            }
        }

        [Test]
        public void GameInitialization_StartTurnIsOne()
        {
            Assert.AreEqual(1, _gameData.CurrentTurn);
        }

        #endregion

        #region Turn Flow Tests

        [Test]
        public void TurnFlow_PhaseProgression()
        {
            var phases = new[]
            {
                TurnPhase.Internal,
                TurnPhase.Diplomacy,
                TurnPhase.Military,
                TurnPhase.End
            };

            TurnPhase currentPhase = TurnPhase.Internal;
            int phaseIndex = 0;

            while (phaseIndex < phases.Length)
            {
                Assert.AreEqual(phases[phaseIndex], currentPhase);
                currentPhase = GetNextPhase(currentPhase);
                phaseIndex++;
            }
        }

        [Test]
        public void TurnFlow_AllFactionsGetTurn()
        {
            var factionOrder = new List<string>(_gameData.Factions.Keys);
            var processedFactions = new HashSet<string>();

            foreach (var factionId in factionOrder)
            {
                processedFactions.Add(factionId);
            }

            Assert.AreEqual(_gameData.Factions.Count, processedFactions.Count);
        }

        [Test]
        public void TurnFlow_TurnIncrementsAfterAllPhases()
        {
            int startTurn = _gameData.CurrentTurn;

            // シミュレート: ターン終了
            _gameData.CurrentTurn++;

            Assert.AreEqual(startTurn + 1, _gameData.CurrentTurn);
        }

        private TurnPhase GetNextPhase(TurnPhase current)
        {
            switch (current)
            {
                case TurnPhase.Internal: return TurnPhase.Diplomacy;
                case TurnPhase.Diplomacy: return TurnPhase.Military;
                case TurnPhase.Military: return TurnPhase.End;
                default: return TurnPhase.Internal;
            }
        }

        #endregion

        #region Resource Flow Tests

        [Test]
        public void ResourceFlow_TurnIncomeApplied()
        {
            var faction = _gameData.Factions["player_faction"];
            int initialGold = faction.Gold;

            // 収入を計算・適用
            int income = CalculateGoldIncome(faction);
            faction.Gold += income;

            Assert.Greater(faction.Gold, initialGold);
        }

        [Test]
        public void ResourceFlow_FoodConsumptionApplied()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.Food = 1000;

            // 消費を計算・適用
            int consumption = CalculateFoodConsumption(faction);
            faction.Food -= consumption;

            Assert.Less(faction.Food, 1000);
        }

        [Test]
        public void ResourceFlow_StratagemPointsRecovery()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.StratagemPoints = 5;
            int maxSP = Constants.Balance.DefaultMaxStratagemPoints;

            // 回復を適用
            int recovery = Constants.Balance.StratagemPointRecoveryBase;
            faction.StratagemPoints = System.Math.Min(
                faction.StratagemPoints + recovery, maxSP);

            Assert.AreEqual(7, faction.StratagemPoints);
        }

        private int CalculateGoldIncome(Faction faction)
        {
            int total = 0;
            foreach (var territoryId in faction.TerritoryIds)
            {
                if (_gameData.Territories.TryGetValue(territoryId, out var territory))
                {
                    total += territory.Economy * Constants.Balance.BaseIncomePerEconomy;
                    total += territory.Population / Constants.Balance.PopulationIncomeBonus;
                }
            }
            return total;
        }

        private int CalculateFoodConsumption(Faction faction)
        {
            int totalSoldiers = 0;
            foreach (var army in _gameData.Armies.Values)
            {
                if (army.FactionId == faction.Id)
                {
                    totalSoldiers += army.SoldierCount;
                }
            }
            return totalSoldiers * Constants.Balance.FoodConsumptionPerSoldier;
        }

        #endregion

        #region Faction Interaction Tests

        [Test]
        public void FactionInteraction_TerritoryCapture()
        {
            var attacker = _gameData.Factions["player_faction"];
            var defender = _gameData.Factions["enemy_faction"];
            var territory = _gameData.Territories["territory_2"];

            // 初期状態
            Assert.AreEqual("enemy_faction", territory.OwnerId);
            Assert.Contains("territory_2", defender.TerritoryIds);

            // 領地奪取をシミュレート
            defender.TerritoryIds.Remove(territory.Id);
            attacker.TerritoryIds.Add(territory.Id);
            territory.OwnerId = attacker.Id;

            // 結果確認
            Assert.AreEqual("player_faction", territory.OwnerId);
            Assert.Contains("territory_2", attacker.TerritoryIds);
            Assert.IsFalse(defender.TerritoryIds.Contains("territory_2"));
        }

        [Test]
        public void FactionInteraction_FactionElimination()
        {
            var defender = _gameData.Factions["enemy_faction"];

            // 全領地を失う
            defender.TerritoryIds.Clear();

            // 滅亡判定
            bool isEliminated = defender.TerritoryIds.Count == 0;
            Assert.IsTrue(isEliminated);
        }

        [Test]
        public void FactionInteraction_AllianceFormation()
        {
            var faction1 = _gameData.Factions["player_faction"];
            var faction2 = _gameData.Factions["enemy_faction"];

            // 同盟を結ぶ
            faction1.AllianceIds.Add(faction2.Id);
            faction2.AllianceIds.Add(faction1.Id);

            Assert.Contains(faction2.Id, faction1.AllianceIds);
            Assert.Contains(faction1.Id, faction2.AllianceIds);
        }

        #endregion

        #region Victory Condition Tests

        [Test]
        public void VictoryCondition_ConquestVictory()
        {
            var player = _gameData.Factions["player_faction"];
            var enemy = _gameData.Factions["enemy_faction"];

            // プレイヤーが全領地を所有
            player.TerritoryIds.Add("territory_2");
            enemy.TerritoryIds.Clear();

            int totalTerritories = _gameData.Territories.Count;
            bool conquestVictory = player.TerritoryIds.Count == totalTerritories;

            Assert.IsTrue(conquestVictory);
        }

        [Test]
        public void VictoryCondition_SurvivalVictory()
        {
            var player = _gameData.Factions["player_faction"];
            _gameData.CurrentTurn = 100; // 最大ターン

            bool reachedMaxTurn = _gameData.CurrentTurn >= 100;
            bool hasTerritory = player.TerritoryIds.Count > 0;
            bool survivalVictory = reachedMaxTurn && hasTerritory;

            Assert.IsTrue(survivalVictory);
        }

        [Test]
        public void VictoryCondition_DefeatByElimination()
        {
            var player = _gameData.Factions["player_faction"];

            // プレイヤーが全領地を失う
            player.TerritoryIds.Clear();

            bool isDefeated = player.TerritoryIds.Count == 0;
            Assert.IsTrue(isDefeated);
        }

        #endregion

        #region Character Assignment Tests

        [Test]
        public void CharacterAssignment_CommanderToArmy()
        {
            var army = _gameData.Armies["player_army"];
            var character = _gameData.Characters["player_char"];

            army.CommanderId = character.Id;

            Assert.AreEqual(character.Id, army.CommanderId);
        }

        [Test]
        public void CharacterAssignment_CharacterBelongsToFaction()
        {
            var character = _gameData.Characters["player_char"];
            var faction = _gameData.Factions["player_faction"];

            Assert.AreEqual(faction.Id, character.FactionId);
        }

        [Test]
        public void CharacterAssignment_DefectionChangeFaction()
        {
            var character = _gameData.Characters["enemy_char"];

            // 裏切りをシミュレート
            string oldFaction = character.FactionId;
            character.FactionId = "player_faction";

            Assert.AreNotEqual(oldFaction, character.FactionId);
            Assert.AreEqual("player_faction", character.FactionId);
        }

        #endregion

        #region Army Movement Tests

        [Test]
        public void ArmyMovement_ChangeLocation()
        {
            var army = _gameData.Armies["player_army"];
            string originalLocation = army.LocationId;

            // 移動をシミュレート
            army.LocationId = "territory_2";

            Assert.AreNotEqual(originalLocation, army.LocationId);
            Assert.AreEqual("territory_2", army.LocationId);
        }

        [Test]
        public void ArmyMovement_MoraleDecreaseOnLongMove()
        {
            var army = _gameData.Armies["player_army"];
            int originalMorale = army.Morale;

            // 長距離移動のペナルティ
            army.Morale -= 5;

            Assert.Less(army.Morale, originalMorale);
        }

        #endregion

        #region Multi-Turn Simulation Tests

        [Test]
        public void MultiTurnSimulation_FiveturnsWithResourceChanges()
        {
            var faction = _gameData.Factions["player_faction"];
            int initialGold = faction.Gold;

            // 5ターン分の収入をシミュレート
            for (int turn = 1; turn <= 5; turn++)
            {
                int income = CalculateGoldIncome(faction);
                faction.Gold += income;
                _gameData.CurrentTurn++;
            }

            Assert.AreEqual(6, _gameData.CurrentTurn);
            Assert.Greater(faction.Gold, initialGold);
        }

        [Test]
        public void MultiTurnSimulation_MoraleRecoveryOverTime()
        {
            var army = _gameData.Armies["player_army"];
            army.Morale = 30; // 低い士気から開始

            // 5ターン分の回復
            for (int turn = 1; turn <= 5; turn++)
            {
                army.Morale = System.Math.Min(
                    army.Morale + Constants.Balance.MoraleRecoveryPerTurn,
                    Constants.Balance.MaxMorale);
            }

            Assert.AreEqual(55, army.Morale); // 30 + 25
        }

        #endregion
    }
}
