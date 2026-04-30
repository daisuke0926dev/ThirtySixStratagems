using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// テスト用ヘルパークラス
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// テスト用BattleUnitを作成
        /// </summary>
        public static BattleUnit CreateTestBattleUnit(
            string armyId = "test_army",
            string factionId = "test_faction",
            string armyName = "Test Army",
            int soldiers = 1000,
            int morale = 50,
            int basePower = 100,
            int terrainBonus = 0,
            string commanderId = null)
        {
            return new BattleUnit
            {
                ArmyId = armyId,
                FactionId = factionId,
                ArmyName = armyName,
                FactionName = "Test Faction",
                CurrentSoldiers = soldiers,
                InitialSoldiers = soldiers,
                Morale = morale,
                BaseCombatPower = basePower,
                TerrainBonus = terrainBonus,
                CommanderId = commanderId,
                ActiveEffects = new List<BattleEffect>()
            };
        }

        /// <summary>
        /// テスト用BattleStateを作成
        /// </summary>
        public static BattleState CreateTestBattleState(
            BattleUnit attacker = null,
            BattleUnit defender = null,
            int currentRound = 1)
        {
            return new BattleState
            {
                Attacker = attacker ?? CreateTestBattleUnit(armyId: "attacker"),
                Defender = defender ?? CreateTestBattleUnit(armyId: "defender"),
                CurrentRound = currentRound,
                TerritoryId = "test_territory"
            };
        }

        /// <summary>
        /// テスト用Factionを作成
        /// </summary>
        public static Faction CreateTestFaction(
            string id = "test_faction",
            string name = "Test Faction",
            int gold = 1000,
            int food = 500,
            int stratagemPoints = 5,
            bool isPlayer = false)
        {
            return new Faction
            {
                Id = id,
                Name = name,
                Gold = gold,
                Food = food,
                StratagemPoints = stratagemPoints,
                IsPlayer = isPlayer,
                TerritoryIds = new List<string> { "territory_1" },
                CharacterIds = new List<string>(),
                ArmyIds = new List<string>(),
                AllianceIds = new List<string>()
            };
        }

        /// <summary>
        /// テスト用Characterを作成
        /// </summary>
        public static Character CreateTestCharacter(
            string id = "test_char",
            string name = "Test Character",
            string factionId = "test_faction",
            int strength = 70,
            int intelligence = 70,
            int leadership = 70,
            int loyalty = 100)
        {
            return new Character
            {
                Id = id,
                Name = name,
                FactionId = factionId,
                Strength = strength,
                Intelligence = intelligence,
                Leadership = leadership,
                Loyalty = loyalty
            };
        }

        /// <summary>
        /// テスト用Territoryを作成
        /// </summary>
        public static Territory CreateTestTerritory(
            string id = "test_territory",
            string name = "Test Territory",
            string ownerId = "test_faction",
            int economy = 50,
            int population = 10000,
            int defense = 30)
        {
            return new Territory
            {
                Id = id,
                Name = name,
                OwnerId = ownerId,
                Economy = economy,
                Population = population,
                Defense = defense,
                AdjacentTerritoryIds = new List<string>()
            };
        }

        /// <summary>
        /// テスト用Armyを作成
        /// </summary>
        public static Army CreateTestArmy(
            string id = "test_army",
            string factionId = "test_faction",
            string commanderId = null,
            int soldierCount = 1000,
            int morale = 50)
        {
            return new Army
            {
                Id = id,
                FactionId = factionId,
                CommanderId = commanderId,
                SoldierCount = soldierCount,
                Morale = morale,
                LocationId = "test_territory"
            };
        }

        /// <summary>
        /// テスト用GameDataを作成
        /// </summary>
        public static GameData CreateTestGameData()
        {
            var gameData = new GameData
            {
                CurrentTurn = 1,
                CurrentYear = 190,
                PlayerFactionId = "player_faction",
                Factions = new Dictionary<string, Faction>(),
                Territories = new Dictionary<string, Territory>(),
                Characters = new Dictionary<string, Character>(),
                Armies = new Dictionary<string, Army>()
            };

            // プレイヤー勢力
            var playerFaction = CreateTestFaction("player_faction", "Player Faction", isPlayer: true);
            gameData.Factions["player_faction"] = playerFaction;

            // 敵勢力
            var enemyFaction = CreateTestFaction("enemy_faction", "Enemy Faction");
            gameData.Factions["enemy_faction"] = enemyFaction;

            // 領地
            var territory1 = CreateTestTerritory("territory_1", "Territory 1", "player_faction");
            var territory2 = CreateTestTerritory("territory_2", "Territory 2", "enemy_faction");
            territory1.AdjacentTerritoryIds.Add("territory_2");
            territory2.AdjacentTerritoryIds.Add("territory_1");
            gameData.Territories["territory_1"] = territory1;
            gameData.Territories["territory_2"] = territory2;

            // 武将
            var playerChar = CreateTestCharacter("player_char", "Player Character", "player_faction");
            var enemyChar = CreateTestCharacter("enemy_char", "Enemy Character", "enemy_faction");
            gameData.Characters["player_char"] = playerChar;
            gameData.Characters["enemy_char"] = enemyChar;

            // 軍
            var playerArmy = CreateTestArmy("player_army", "player_faction", "player_char");
            var enemyArmy = CreateTestArmy("enemy_army", "enemy_faction", "enemy_char");
            gameData.Armies["player_army"] = playerArmy;
            gameData.Armies["enemy_army"] = enemyArmy;

            return gameData;
        }
    }
}
