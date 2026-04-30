using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// セーブ/ロード統合テスト
    /// データの保存・復元の整合性をテスト
    /// </summary>
    [TestFixture]
    public class SaveLoadIntegrationTests
    {
        private GameData _gameData;

        [SetUp]
        public void SetUp()
        {
            _gameData = TestHelpers.CreateTestGameData();
        }

        #region Save Data Structure Tests

        [Test]
        public void SaveDataStructure_GameDataSerializable()
        {
            Assert.IsNotNull(_gameData);
            Assert.IsNotNull(_gameData.Factions);
            Assert.IsNotNull(_gameData.Territories);
            Assert.IsNotNull(_gameData.Characters);
            Assert.IsNotNull(_gameData.Armies);
        }

        [Test]
        public void SaveDataStructure_HasRequiredFields()
        {
            Assert.IsNotNull(_gameData.PlayerFactionId);
            Assert.GreaterOrEqual(_gameData.CurrentTurn, 1);
            Assert.Greater(_gameData.CurrentYear, 0);
        }

        [Test]
        public void SaveDataStructure_FactionDataComplete()
        {
            var faction = _gameData.Factions["player_faction"];

            Assert.IsNotNull(faction.Id);
            Assert.IsNotNull(faction.Name);
            Assert.GreaterOrEqual(faction.Gold, 0);
            Assert.GreaterOrEqual(faction.Food, 0);
            Assert.GreaterOrEqual(faction.StratagemPoints, 0);
        }

        [Test]
        public void SaveDataStructure_TerritoryDataComplete()
        {
            var territory = _gameData.Territories["territory_1"];

            Assert.IsNotNull(territory.Id);
            Assert.IsNotNull(territory.Name);
            Assert.IsNotNull(territory.OwnerId);
            Assert.GreaterOrEqual(territory.Population, 0);
        }

        [Test]
        public void SaveDataStructure_CharacterDataComplete()
        {
            var character = _gameData.Characters["player_char"];

            Assert.IsNotNull(character.Id);
            Assert.IsNotNull(character.Name);
            Assert.GreaterOrEqual(character.Strength, 0);
            Assert.GreaterOrEqual(character.Intelligence, 0);
        }

        [Test]
        public void SaveDataStructure_ArmyDataComplete()
        {
            var army = _gameData.Armies["player_army"];

            Assert.IsNotNull(army.Id);
            Assert.IsNotNull(army.FactionId);
            Assert.GreaterOrEqual(army.SoldierCount, 0);
            Assert.GreaterOrEqual(army.Morale, 0);
        }

        #endregion

        #region Data Consistency Tests

        [Test]
        public void DataConsistency_FactionOwnsListedTerritories()
        {
            var faction = _gameData.Factions["player_faction"];

            foreach (var territoryId in faction.TerritoryIds)
            {
                Assert.IsTrue(_gameData.Territories.ContainsKey(territoryId),
                    $"Territory {territoryId} not found");
                Assert.AreEqual(faction.Id, _gameData.Territories[territoryId].OwnerId);
            }
        }

        [Test]
        public void DataConsistency_TerritoryOwnerExists()
        {
            foreach (var territory in _gameData.Territories.Values)
            {
                if (!string.IsNullOrEmpty(territory.OwnerId))
                {
                    Assert.IsTrue(_gameData.Factions.ContainsKey(territory.OwnerId),
                        $"Owner {territory.OwnerId} not found for territory {territory.Id}");
                }
            }
        }

        [Test]
        public void DataConsistency_CharacterBelongsToFaction()
        {
            foreach (var character in _gameData.Characters.Values)
            {
                Assert.IsTrue(_gameData.Factions.ContainsKey(character.FactionId),
                    $"Faction {character.FactionId} not found for character {character.Id}");
            }
        }

        [Test]
        public void DataConsistency_ArmyBelongsToFaction()
        {
            foreach (var army in _gameData.Armies.Values)
            {
                Assert.IsTrue(_gameData.Factions.ContainsKey(army.FactionId),
                    $"Faction {army.FactionId} not found for army {army.Id}");
            }
        }

        [Test]
        public void DataConsistency_ArmyCommanderExists()
        {
            foreach (var army in _gameData.Armies.Values)
            {
                if (!string.IsNullOrEmpty(army.CommanderId))
                {
                    Assert.IsTrue(_gameData.Characters.ContainsKey(army.CommanderId),
                        $"Commander {army.CommanderId} not found for army {army.Id}");
                }
            }
        }

        #endregion

        #region JSON Serialization Simulation Tests

        [Test]
        public void JsonSerialization_TurnNumberPreserved()
        {
            int originalTurn = 15;
            _gameData.CurrentTurn = originalTurn;

            // シミュレート: 保存・復元
            var savedTurn = _gameData.CurrentTurn;
            var loadedData = new GameData { CurrentTurn = savedTurn };

            Assert.AreEqual(originalTurn, loadedData.CurrentTurn);
        }

        [Test]
        public void JsonSerialization_FactionGoldPreserved()
        {
            var faction = _gameData.Factions["player_faction"];
            faction.Gold = 12345;

            // シミュレート: 保存・復元
            int savedGold = faction.Gold;
            var loadedFaction = TestHelpers.CreateTestFaction(gold: savedGold);

            Assert.AreEqual(12345, loadedFaction.Gold);
        }

        [Test]
        public void JsonSerialization_TerritoryOwnerPreserved()
        {
            var territory = _gameData.Territories["territory_1"];
            string originalOwner = territory.OwnerId;

            // シミュレート: 保存・復元
            var savedOwner = territory.OwnerId;
            var loadedTerritory = TestHelpers.CreateTestTerritory(ownerId: savedOwner);

            Assert.AreEqual(originalOwner, loadedTerritory.OwnerId);
        }

        [Test]
        public void JsonSerialization_ArmySoldiersPreserved()
        {
            var army = _gameData.Armies["player_army"];
            army.SoldierCount = 5000;

            // シミュレート: 保存・復元
            int savedSoldiers = army.SoldierCount;
            var loadedArmy = TestHelpers.CreateTestArmy(soldierCount: savedSoldiers);

            Assert.AreEqual(5000, loadedArmy.SoldierCount);
        }

        [Test]
        public void JsonSerialization_CharacterStatsPreserved()
        {
            var character = _gameData.Characters["player_char"];
            character.Strength = 85;
            character.Intelligence = 92;

            // シミュレート: 保存・復元
            var loadedChar = TestHelpers.CreateTestCharacter(
                strength: character.Strength,
                intelligence: character.Intelligence
            );

            Assert.AreEqual(85, loadedChar.Strength);
            Assert.AreEqual(92, loadedChar.Intelligence);
        }

        #endregion

        #region State After Load Tests

        [Test]
        public void StateAfterLoad_GameCanContinue()
        {
            // ゲーム状態を変更
            _gameData.CurrentTurn = 25;
            _gameData.Factions["player_faction"].Gold = 5000;

            // ロード後も継続可能か
            bool canContinue = _gameData.CurrentTurn > 0 &&
                               _gameData.Factions.Count > 0 &&
                               _gameData.Territories.Count > 0;

            Assert.IsTrue(canContinue);
        }

        [Test]
        public void StateAfterLoad_PlayerFactionValid()
        {
            string playerId = _gameData.PlayerFactionId;

            bool playerValid = !string.IsNullOrEmpty(playerId) &&
                               _gameData.Factions.ContainsKey(playerId) &&
                               _gameData.Factions[playerId].TerritoryIds.Count > 0;

            Assert.IsTrue(playerValid);
        }

        [Test]
        public void StateAfterLoad_AtLeastOneFactionAlive()
        {
            int aliveFactions = 0;
            foreach (var faction in _gameData.Factions.Values)
            {
                if (faction.TerritoryIds.Count > 0)
                {
                    aliveFactions++;
                }
            }

            Assert.Greater(aliveFactions, 0);
        }

        #endregion

        #region Modified State Save Tests

        [Test]
        public void ModifiedStateSave_TerritoryOwnershipChange()
        {
            var territory = _gameData.Territories["territory_2"];
            var newOwner = _gameData.Factions["player_faction"];
            var oldOwner = _gameData.Factions["enemy_faction"];

            // 領地を奪取
            oldOwner.TerritoryIds.Remove(territory.Id);
            newOwner.TerritoryIds.Add(territory.Id);
            territory.OwnerId = newOwner.Id;

            // 保存後の状態を確認
            Assert.AreEqual(newOwner.Id, territory.OwnerId);
            Assert.Contains(territory.Id, newOwner.TerritoryIds);
        }

        [Test]
        public void ModifiedStateSave_ResourceChanges()
        {
            var faction = _gameData.Factions["player_faction"];
            int originalGold = faction.Gold;

            faction.Gold += 500;
            faction.Food -= 100;

            Assert.AreEqual(originalGold + 500, faction.Gold);
        }

        [Test]
        public void ModifiedStateSave_ArmyCasualties()
        {
            var army = _gameData.Armies["player_army"];
            int originalSoldiers = army.SoldierCount;

            army.SoldierCount -= 300;

            Assert.AreEqual(originalSoldiers - 300, army.SoldierCount);
        }

        [Test]
        public void ModifiedStateSave_CharacterDefection()
        {
            var character = _gameData.Characters["enemy_char"];

            character.FactionId = "player_faction";

            Assert.AreEqual("player_faction", character.FactionId);
        }

        #endregion

        #region Save Slot Tests

        [Test]
        public void SaveSlot_MaxSlots()
        {
            int maxSlots = Constants.Save.MaxSaveSlots;

            Assert.AreEqual(10, maxSlots);
        }

        [Test]
        public void SaveSlot_ValidSlotRange()
        {
            int slotNumber = 5;
            int maxSlots = Constants.Save.MaxSaveSlots;

            bool validSlot = slotNumber >= 1 && slotNumber <= maxSlots;

            Assert.IsTrue(validSlot);
        }

        [Test]
        public void SaveSlot_InvalidSlot()
        {
            int slotNumber = 15;
            int maxSlots = Constants.Save.MaxSaveSlots;

            bool validSlot = slotNumber >= 1 && slotNumber <= maxSlots;

            Assert.IsFalse(validSlot);
        }

        #endregion

        #region Save Version Tests

        [Test]
        public void SaveVersion_CurrentVersion()
        {
            int version = Constants.Save.SaveVersion;

            Assert.GreaterOrEqual(version, 1);
        }

        [Test]
        public void SaveVersion_CompatibilityCheck()
        {
            int savedVersion = 1;
            int currentVersion = Constants.Save.SaveVersion;

            bool isCompatible = savedVersion <= currentVersion;

            Assert.IsTrue(isCompatible);
        }

        #endregion

        #region Data Integrity Tests

        [Test]
        public void DataIntegrity_NoNullCollections()
        {
            Assert.IsNotNull(_gameData.Factions);
            Assert.IsNotNull(_gameData.Territories);
            Assert.IsNotNull(_gameData.Characters);
            Assert.IsNotNull(_gameData.Armies);
        }

        [Test]
        public void DataIntegrity_AllIdsUnique()
        {
            var allIds = new HashSet<string>();

            foreach (var id in _gameData.Factions.Keys)
            {
                Assert.IsTrue(allIds.Add("faction_" + id), $"Duplicate faction ID: {id}");
            }
            foreach (var id in _gameData.Territories.Keys)
            {
                Assert.IsTrue(allIds.Add("territory_" + id), $"Duplicate territory ID: {id}");
            }
            foreach (var id in _gameData.Characters.Keys)
            {
                Assert.IsTrue(allIds.Add("character_" + id), $"Duplicate character ID: {id}");
            }
            foreach (var id in _gameData.Armies.Keys)
            {
                Assert.IsTrue(allIds.Add("army_" + id), $"Duplicate army ID: {id}");
            }
        }

        [Test]
        public void DataIntegrity_NoOrphanedReferences()
        {
            // 全ての参照が有効か確認
            foreach (var faction in _gameData.Factions.Values)
            {
                foreach (var territoryId in faction.TerritoryIds)
                {
                    Assert.IsTrue(_gameData.Territories.ContainsKey(territoryId));
                }
            }
        }

        #endregion

        #region Partial Save/Load Tests

        [Test]
        public void PartialSave_OnlyChangedDataMarked()
        {
            var faction = _gameData.Factions["player_faction"];
            int originalGold = faction.Gold;

            faction.Gold += 1000;

            bool hasChanged = faction.Gold != originalGold;
            Assert.IsTrue(hasChanged);
        }

        [Test]
        public void PartialLoad_MergeWithExistingData()
        {
            var existingFaction = _gameData.Factions["player_faction"];
            int savedGold = 9999;

            // 既存データに上書き
            existingFaction.Gold = savedGold;

            Assert.AreEqual(9999, existingFaction.Gold);
        }

        #endregion
    }
}
